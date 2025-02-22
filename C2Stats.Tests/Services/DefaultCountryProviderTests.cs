using System.Text.Json;
using C2Stats.Services;
using Moq;

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
			// arrange
			var httpClientFactoryMoq = new Mock<IHttpClientFactory>();
			httpClientFactoryMoq.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(new HttpClient());

			var downloader = new CountryDownloader(httpClientFactoryMoq.Object);
			
			// act
			var result = await downloader.Download(cancellationToken);

			// assert
			Assert.That(result, Is.Not.Null);
			
			var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
			
			await File.WriteAllTextAsync("countries.json", json, cancellationToken);
		}
	}
}