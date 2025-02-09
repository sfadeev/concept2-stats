using C2Stats.Models;
using C2Stats.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace C2Stats.Tests.Services
{
	public class WodDownloaderTests
	{
		[Test, Ignore("Slow")]
		[TestCase("2025-01-24", WodType.RowErg)]
		[TestCase("2025-01-25", WodType.SkiErg)]
		public async Task Download_FromWeb_ShouldWork(string date, string wodType)
		{
			// arrange
			var httpClientFactoryMoq = new Mock<IHttpClientFactory>();
			httpClientFactoryMoq.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(new HttpClient());
			
			var cancellationToken = CancellationToken.None;
			var downloader = new WodDownloader(NullLogger<WodDownloader>.Instance, httpClientFactoryMoq.Object,
				new WodParser(), new DefaultCountryProvider());
			
			// act
			var result = await downloader.Download(DateOnly.Parse(date), wodType, null, cancellationToken);
			
			// assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result.TotalCount, Is.Not.Null);
			Assert.That(result.TotalCount, Is.GreaterThan(0));
			Assert.That(result.Items.Count, Is.GreaterThan(0));
			Assert.That(result.Items.Count(x => x.CountryCode == null), Is.EqualTo(0));
		}
	}
}