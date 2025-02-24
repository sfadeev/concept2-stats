using C2Stats.Models;
using C2Stats.Services;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
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
			var mediatorMoq = new Mock<IMediator>();
			var httpClientFactoryMoq = new Mock<IHttpClientFactory>();
			httpClientFactoryMoq.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(new HttpClient());
			
			var cancellationToken = CancellationToken.None;
			var options = Options.Create(new AppOptions());
			var downloader = new WodDownloader(NullLogger<WodDownloader>.Instance, httpClientFactoryMoq.Object,
				new WodParser(), new ProfileFileStorage(NullLogger<ProfileFileStorage>.Instance, options, mediatorMoq.Object));
			
			// act
			var result = await downloader.Download(DateOnly.Parse(date), wodType, null, cancellationToken);
			
			// assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result.TotalCount, Is.Not.Null);
			Assert.That(result.TotalCount, Is.GreaterThan(0));
			Assert.That(result.Items.Count, Is.GreaterThan(0));
			Assert.That(result.Items.Count(x => x.Country == UnaffiliatedCountry.Placeholder), Is.EqualTo(0));
		}

		[Test, Ignore("Slow")]
		[TestCase("2025-01-24", WodType.RowErg)]
		public async Task Download_FromWeb_Test2(string date, string wodType)
		{
			// arrange
			var mediatorMoq = new Mock<IMediator>();
			var httpClientFactoryMoq = new Mock<IHttpClientFactory>();
			httpClientFactoryMoq.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(new HttpClient());
			
			var cancellationToken = CancellationToken.None;
			var options = Options.Create(new AppOptions());
			var downloader = new WodDownloader(NullLogger<WodDownloader>.Instance, httpClientFactoryMoq.Object,
				new WodParser(), new ProfileFileStorage(NullLogger<ProfileFileStorage>.Instance, options, mediatorMoq.Object));
			
			// act
			var result = await downloader.Download(DateOnly.Parse(date), wodType, null, cancellationToken);
			
			// assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result.TotalCount, Is.EqualTo(1989));
			Assert.That(result.Items.Count(x => x.Country == "RUS"), Is.EqualTo(11));
			Assert.That(result.Items.Count(x => x.Country == "BLR"), Is.EqualTo(1));
			Assert.That(result.Items.Count(x => x.Country == UnaffiliatedCountry.Placeholder), Is.EqualTo(0));
			Assert.That(result.Items.Count(x => x.Sex == null), Is.EqualTo(0));
		}
	}
}