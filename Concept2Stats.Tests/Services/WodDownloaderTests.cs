using Concept2Stats.Models;
using Concept2Stats.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Concept2Stats.Tests.Services
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
			var cancellationChecker = new TestWodDownloadCancellationChecker(3);
			var downloader = new WodDownloader(NullLogger<WodDownloader>.Instance, httpClientFactoryMoq.Object,
				new WodParser(), new DefaultCountryProvider());
			
			// act
			var result = await downloader.Download(DateOnly.Parse(date), wodType, cancellationChecker, cancellationToken);
			
			// assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result.TotalCount, Is.Not.Null);
			Assert.That(result.TotalCount, Is.GreaterThan(0));
			Assert.That(result.Items.Count, Is.GreaterThan(0));
			Assert.That(result.Items.Count(x => x.CountryCode == null), Is.EqualTo(0));
		}
		
		private class TestWodDownloadCancellationChecker(int maxPages) : IWodDownloadCancellationChecker
		{
			public bool ShouldCancel(WodDownloadProgressContext context, out string? reason)
			{
				if (context.PageNo >= maxPages)
				{
					reason = $"No more than {maxPages} pages should be downloaded.";
					return true;
				}
				
				reason = null;
				return false;
			}
		}
	}
}