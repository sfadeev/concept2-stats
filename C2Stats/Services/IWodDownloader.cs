using System.Diagnostics;
using C2Stats.Entities;
using C2Stats.Models;
using Microsoft.AspNetCore.Http.Extensions;

namespace C2Stats.Services
{
	public interface IWodDownloader
	{
		Task<WodResult?> Download(DateOnly date, string wodType, 
			IWodDownloadCancellationChecker? cancellationChecker, CancellationToken cancellationToken);
	}

	public interface IWodDownloadCancellationChecker
	{
		bool ShouldCancel(WodDownloadProgressContext context);
	}

	public class GenericWodDownloadCancellationChecker(Func<WodDownloadProgressContext, bool> check) : IWodDownloadCancellationChecker
	{
		public bool ShouldCancel(WodDownloadProgressContext context)
		{
			return check(context);
		}
	}

	public class WodDownloadProgressContext
	{
		public DateOnly Date { get; init; }
		
		public required string WodType { get; init; }
		
		public int? CountryId { get; init; }
		
		public int PageNo { get; init; }
		
		public required WodResult Result { get; init; }
		
		public string? CancelReason { get; set; }
	}
	
	public class WodDownloader(ILogger<WodDownloader> logger, IHttpClientFactory httpClientFactory,
		IWodParser parser, IProfileFileStorage profileFileStorage) : IWodDownloader
	{
		public async Task<WodResult?> Download(DateOnly date, string wodType,
			IWodDownloadCancellationChecker? cancellationChecker, CancellationToken cancellationToken)
		{
			var result = await Download(date, wodType, null, null, cancellationChecker, cancellationToken);

			// download cancelled
			if (result == null) return result;
			
			var unaffCount = 0;
			var unsexCount = 0;
			
			foreach (var item in result.Items)
			{
				if (item.Id != null)
				{
					profileFileStorage.TryGetProfile(item.Id.Value, out var profile);
						
					if (item.Country == UnaffiliatedCountry.Placeholder)
					{
						if (profile?.Country != null)
						{
							item.Country = profile.Country;
						}
						else
						{
							unaffCount++;
						}
					}

					if (profile != null)
					{
						item.Sex = profile.Sex;
					}
					else
					{
						unsexCount++;
					}
				}
			}
			
			if (unaffCount > 0)
			{
				foreach (var unaffiliatedCountry in UnaffiliatedCountry.GetList())
				{
					if (unaffCount <= 0) break;
					
					var countryResult = await Download(date, wodType, unaffiliatedCountry.Id, null, cancellationChecker, cancellationToken);

					// download cancelled
					if (countryResult == null) continue;
				
					foreach (var countryItem in countryResult.Items)
					{
						var item = result.Items.SingleOrDefault(x => x.Id == countryItem.Id);

						if (item != null)
						{
							item.Country = unaffiliatedCountry.Code;
							
							unaffCount--;
						}
					}
				}
			}

			if (unsexCount > 0)
			{
				const string genderM = "M";
				const string genderF = "F";
				
				var genderResult = await Download(date, wodType, null, genderF, cancellationChecker, cancellationToken);

				// download cancelled
				if (genderResult == null) return null;

				var womenIds = genderResult.Items.Where(x => x.Id != null).Select(x => x.Id!.Value).ToList();
				
				var genderUnknown = result.Items.Where(x => x.Id != null && x.Sex == null).ToList();

				foreach (var genderItem in genderUnknown)
				{
					genderItem.Sex = womenIds.Contains(genderItem.Id!.Value) ? genderF : genderM;
				}
			}

			foreach (var item in result.Items)
			{
				if (item.Id != null)
				{
					profileFileStorage.UpdatedProfile(new DbProfile
					{
						Id = item.Id.Value,
						Name = item.Name,
						Country = item.Country,
						Sex = item.Sex,
						Location = item.Location
					});
				}
			}
			
			return result;
		}
		
		private async Task<WodResult?> Download(DateOnly date, string wodType, int? countryId, string? gender, 
			IWodDownloadCancellationChecker? cancellationChecker, CancellationToken cancellationToken)
		{
			var sw = Stopwatch.StartNew();
			
			var httpClient = httpClientFactory.CreateClient();

			var result = new WodResult();
			
			var pageNo = 1;
			int? totalPageCount = null;
			
			while (pageNo <= 300) // todo: move to settings
			{
				if (cancellationToken.IsCancellationRequested)
				{
					cancellationToken.ThrowIfCancellationRequested();
				}

				var uriBuilder = new UriBuilder(Uri.UriSchemeHttps, "log.concept2.com")
				{
					Path = $"/wod/{date:yyyy-MM-dd}/{wodType}",
					Query = new QueryBuilder()
						.Add("page", pageNo)
						.AddIfExists("country", countryId)
						.AddIfExists("gender", gender)
						.ToString()
				};
				
				logger.LogDebug("Downloading ({PageNo}/{TotalPageCount}) {Url}", pageNo, totalPageCount, uriBuilder.Uri);
				
				var response = await httpClient.GetAsync(uriBuilder.Uri, cancellationToken);
				
				response.EnsureSuccessStatusCode();
				
				var html = await response.Content.ReadAsStringAsync(cancellationToken);
				
				var page = parser.Parse(html);

				if (pageNo == 1)
				{
					result.Date = date;
					result.Type = wodType;
					result.Name = page.Name;
					result.Description = page.Description;
					result.TotalCount = page.TotalCount;
				}
				
				foreach (var item in page.Items)
				{
					// rows can move from one page to another while downloading (new rows appears at the beginning)
					if (result.Items.FirstOrDefault(x => x.Id == item.Id) == null)
					{
						result.Items.Add(item);
					}
				}
				
				if (cancellationChecker != null)
				{
					var progressContext = new WodDownloadProgressContext
					{
						Date = date,
						WodType = wodType,
						CountryId = countryId,
						PageNo = pageNo,
						Result = result
					};
					
					if (cancellationChecker.ShouldCancel(progressContext))
					{
						logger.LogDebug(
							"WoD {Date} {WodType} (country: {CountryId}) download cancelled, reason: {Reason}",
							date, wodType, countryId, progressContext.CancelReason);
						
						return null;
					}
				}
				
				if (page.TotalPageCount > pageNo)
				{	
					pageNo++;
					totalPageCount = page.TotalPageCount;
				}
				else
				{
					break;
				}
			}

			if (logger.IsEnabled(LogLevel.Debug))
			{
				logger.LogDebug(
					"WoD {Date} {WodType} (country: {CountryId}, gender: {Gender}) downloaded, elapsed {Elapsed} ({ItemCount} items)",
					date, wodType, countryId, gender, sw.Elapsed, result.Items.Count);
			}
			
			return result;
		}
	}
}