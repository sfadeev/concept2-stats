using System.Diagnostics;
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
		IWodParser parser, ICountryProvider countryProvider, IProfileFileStorage profileFileStorage) : IWodDownloader
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
					
					// UNAFF, AIN etc
					if (item.Country != null && countryProvider.GetCountryId(item.Country) == null)
					{
						if (profile?.Country != null && profile.Country != item.Country)
						{
							item.Country = profile.Country;
						}
						else
						{
							unaffCount++;
						}
					}
					
					if (profile?.Sex != null)
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

						if (item?.Country != null && countryProvider.GetCountryId(item.Country) == null)
						{
							item.Country = unaffiliatedCountry.Code;
							
							unaffCount--;
						}
					}
				}
			}

			if (unaffCount > 0 && logger.IsEnabled(LogLevel.Warning))
			{
				foreach (var item in result.Items)
				{
					if (item.Country != null && countryProvider.GetCountryId(item.Country) == null)
					{
						logger.LogWarning(
							"Unknown country {Country} in profile {Id} {Name} found in {Date} / {WodType}",
							item.Country, item.Id, item.Name, date, wodType);
					}
				}
			}

			if (unsexCount > 0)
			{
				const string genderM = "M";
				const string genderF = "F";
				
				var femaleResult = await Download(date, wodType, null, genderF, cancellationChecker, cancellationToken);

				// download cancelled
				if (femaleResult != null)
				{
					var femaleIds = femaleResult.Items.Where(x => x.Id != null).Select(x => x.Id!.Value).ToList();
				
					var genderUnknown = result.Items.Where(x => x.Id != null && x.Sex == null).ToList();

					foreach (var item in genderUnknown)
					{
						item.Sex = femaleIds.Contains(item.Id!.Value) ? genderF : genderM;
					}
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