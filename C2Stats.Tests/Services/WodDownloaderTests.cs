using C2Stats.Models;
using C2Stats.Services;
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
			var httpClientMoq = new Mock<IHttpClientFactory>();
			httpClientMoq.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(new HttpClient());
			
			var cancellationToken = CancellationToken.None;
			var downloader = new WodDownloader(httpClientMoq.Object, new WodParser());
			
			// act
			var result = await downloader.Download(DateOnly.Parse(date), wodType, cancellationToken);
			
			// assert
			Assert.That(result, Is.Not.Null);

			/*var resultJson = JsonSerializer.Serialize(result, new JsonSerializerOptions
			{
				WriteIndented = true,
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
			});
			
			File.WriteAllText($"{TestContext.CurrentContext.TestDirectory}\\{date}-{wodType}.json", resultJson);*/
		}
	}
}