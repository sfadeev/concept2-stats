using System.Diagnostics;
using Concept2Stats.Models;

namespace Concept2Stats.Services
{
	public interface IWodDownloader
	{
		Task<WodResult> Download(DateOnly date, string wodType, CancellationToken cancellationToken);
	}
	
	public class WodDownloader(ILogger<WodDownloader> logger, IHttpClientFactory httpClientFactory,
		IWodParser parser, ICountryProvider countryProvider) : IWodDownloader
	{
		public async Task<WodResult> Download(DateOnly date, string wodType, CancellationToken cancellationToken)
		{
			var result = await Download(date, wodType, null, cancellationToken);

			foreach (var unaffiliatedCountry in countryProvider.GetUnaffiliatedCountries())
			{
				var countryResult = await Download(date, wodType, unaffiliatedCountry.Id, cancellationToken);

				foreach (var countryItem in countryResult.Items)
				{
					var item = result.Items.SingleOrDefault(x => x.Id == countryItem.Id);
					
					if (item != null) item.CountryCode = unaffiliatedCountry.Code;
				}
			}

			foreach (var item in result.Items)
			{
				if (item.CountryCode == null && item.Country != "UNAFF")
				{
					item.CountryCode = item.Country;
				}
			}

			return result;
		}
		
		private async Task<WodResult> Download(DateOnly date, string wodType, int? countryId, CancellationToken cancellationToken)
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
				
				var url = $"https://log.concept2.com/wod/{date:yyyy-MM-dd}/{wodType}?page={pageNo}&country={countryId}";
				
				var response = await httpClient.GetAsync(url, cancellationToken);
				
				response.EnsureSuccessStatusCode();
				
				var html = await response.Content.ReadAsStringAsync(cancellationToken);
				
				// todo: remap UNAFF countries
				var page = parser.Parse(html);

				if (pageNo == 1)
				{
					result.Date = date;
					result.Type = wodType;
					result.Name = page.Name;
					result.Description = page.Description;
				}
				
				if (page.Success == true)
				{
					foreach (var item in page.Items)
					{
						if (result.Items.FirstOrDefault(x => x.Id == item.Id) == null)
						{
							result.Items.Add(item);
						}
					}
					
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