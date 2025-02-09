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
		IWodParser parser, ICountryProvider countryProvider) : IWodDownloader
	{
		public async Task<WodResult?> Download(DateOnly date, string wodType,
			IWodDownloadCancellationChecker? cancellationChecker, CancellationToken cancellationToken)
		{
			var result = await Download(date, wodType, null, cancellationChecker, cancellationToken);

			// download cancelled
			if (result == null) return result;
			
			var unaffCount = 0;
			
			foreach (var item in result.Items)
			{
				if (item.Country == countryProvider.UnaffiliatedCountryPlaceholder)
				{
					unaffCount++;
				}
				else
				{
					item.CountryCode = item.Country;
				}
			}
			
			if (unaffCount > 0)
			{
				foreach (var unaffiliatedCountry in countryProvider.GetUnaffiliatedCountries())
				{
					if (unaffCount <= 0) break;
					
					var countryResult = await Download(date, wodType, unaffiliatedCountry.Id, cancellationChecker, cancellationToken);

					// download cancelled
					if (countryResult == null) continue;
				
					foreach (var countryItem in countryResult.Items)
					{
						var item = result.Items.SingleOrDefault(x => x.Id == countryItem.Id);

						if (item != null)
						{
							item.CountryCode = unaffiliatedCountry.Code;
							
							unaffCount--;
						}
					}
				}
			}

			return result;
		}
		
		private async Task<WodResult?> Download(DateOnly date, string wodType, int? countryId, 
			IWodDownloadCancellationChecker? cancellationChecker, CancellationToken cancellationToken)
		{
			var sw = Stopwatch.StartNew();
			
			var httpClient = httpClientFactory.CreateClient();

			var result = new WodResult();
			
			var pageNo = 1;
			
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
						.Add("pageNo", pageNo)
						.AddIfExists("country", countryId).ToString()
				};
				
				logger.LogDebug("Downloading {Url}", uriBuilder.Uri);
				
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
				}
				else
				{
					break;
				}
			}

			if (logger.IsEnabled(LogLevel.Debug))
			{
				logger.LogDebug(
					"WoD {Date} {WodType} (country: {CountryId}) downloaded, elapsed {Elapsed} ({ItemCount} items)",
					date, wodType, countryId, sw.Elapsed, result.Items.Count);
			}
			
			return result;
		}
	}
}