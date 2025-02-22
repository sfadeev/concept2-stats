using System.Web;
using C2Stats.Entities;
using HtmlAgilityPack;

namespace C2Stats.Services
{
	public interface ICountryDownloader
	{
		Task<ICollection<DbCountry>> Download(CancellationToken cancellationToken);
	}

	public class CountryDownloader(IHttpClientFactory httpClientFactory) : ICountryDownloader
	{
		public async Task<ICollection<DbCountry>> Download(CancellationToken cancellationToken)
		{
			var unaffiliatedCountryCodes = UnaffiliatedCountry.GetList().ToDictionary(x => x.Id, x => x.Code);

			var httpClient = httpClientFactory.CreateClient();
			
			var countryCodes = await DownloadCountries(httpClient, "https://log.concept2.com/rankings", 
				"//section[@class='content']//select[@id='country_id']", cancellationToken);
			
			var countryNames = await DownloadCountries(httpClient, "https://log.concept2.com/signup",
				"//div[@class='registration']//select[@id='country_id']", cancellationToken);
			
			// Assert.That(countryCodes, Is.Not.Null);
			// Assert.That(countryNames, Is.Not.Null);
			
			var result = new List<DbCountry>();

			foreach (var (id, name) in countryNames)
			{
				string code;

				if (countryCodes.TryGetValue(id, out var countryCode))
				{
					code = countryCode;
				}
				else if (unaffiliatedCountryCodes.TryGetValue(id, out var unaffCountryCode))
				{
					code = unaffCountryCode;
				}
				else
				{
					throw new KeyNotFoundException("Could not find country code for country id " + id);
				}
				
				result.Add(new DbCountry
				{
					Id = id,
					Code = code,
					Name = name
				});
			}
			
			return result;
		}
		
		private async Task<IDictionary<int, string>> DownloadCountries(
			HttpClient httpClient, string url, string xpath, CancellationToken cancellationToken = default)
		{
			var result = new Dictionary<int, string>();

			var response = await httpClient.GetAsync(url, cancellationToken);
				
			response.EnsureSuccessStatusCode();
			
			var html = await response.Content.ReadAsStringAsync(cancellationToken);
			
			var doc = new HtmlDocument();
			
			doc.LoadHtml(html);
			
			var select = doc.DocumentNode.SelectSingleNode(xpath);

			foreach (var option in select.ChildNodes)
			{
				var idValue = option.Attributes["value"].Value;

				if (int.TryParse(idValue, out var id) && id > 0)
				{
					result[id] = HttpUtility.HtmlDecode(option.InnerText);
				}
			}
			
			return result;
		}
	}
}