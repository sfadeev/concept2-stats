using System.Text.Json;
using System.Web;
using C2Stats.Models;
using C2Stats.Services;
using HtmlAgilityPack;

namespace C2Stats.Tests.Services
{
	public class DefaultCountryProviderTests
	{
		[Test]
		public void GetCountries_ShouldReturnCountries()
		{
			// arrange
			var provider = new DefaultCountryProvider();
			
			// act
			var countries = provider.GetAllCountries().ToList();
			
			// assert
			Assert.That(countries, Is.Not.Null);
			Assert.That(countries, Has.Count.EqualTo(223));
		}
		
		[Test]
		[TestCase("AUS", 13)]
		[TestCase("BLR", 20)]
		[TestCase("RUS", 178)]
		[TestCase("USA", 227)]
		public void GetCountryId_ShouldReturnId_ForCode(string code, int expectedId)
		{
			// arrange
			var provider = new DefaultCountryProvider();
			
			// act
			var id = provider.GetCountryId(code);
			
			// assert
			Assert.That(id, Is.Not.Null);
			Assert.That(id, Is.EqualTo(expectedId));
		}
		
		[Test, Ignore("Initial"), CancelAfter(10_000)]
		public async Task DownloadAndSerializeCountries(CancellationToken cancellationToken)
		{
			var provider = new DefaultCountryProvider();

			var unaffiliatedCountryCodes = provider.GetUnaffiliatedCountries().ToDictionary(x => x.Id, x => x.Code);

			var countryCodes = await DownloadCountries("https://log.concept2.com/rankings", 
				"//section[@class='content']//select[@id='country_id']", cancellationToken);
			
			var countryNames = await DownloadCountries("https://log.concept2.com/signup",
				"//div[@class='registration']//select[@id='country_id']", cancellationToken);
			
			Assert.That(countryCodes, Is.Not.Null);
			Assert.That(countryNames, Is.Not.Null);
			
			var countries = new List<Country>();

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
				
				countries.Add(new Country
				{
					Id = id,
					Code = code,
					Name = name
				});
			}
			
			Assert.That(countries, Is.Not.Null);
			
			var json = JsonSerializer.Serialize(countries, new JsonSerializerOptions { WriteIndented = true });
			
			await File.WriteAllTextAsync("countries.json", json, cancellationToken);
		}

		private static async Task<IDictionary<int, string>> DownloadCountries(
			string url, string xpath, CancellationToken cancellationToken = default)
		{
			var result = new Dictionary<int, string>();
			
			using (var httpClient = new HttpClient())
			{
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
			}
			
			return result;
		}
	}
}