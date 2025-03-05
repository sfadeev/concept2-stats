using C2Stats.Models;
using C2Stats.Services;

namespace C2Stats.Tests.Services
{
	public class WodParserTests
	{
		[Test]
		[TestCase("../../../Content/wod-2025-01-01-rowerg-no-result.html", 1609, 33)]
		public async Task Parse_WithNoResult_ShouldWork(string htmlPath, int totalCount, int totalPageNum)
		{
			// arrange
			var parser = new WodParser();
			var html = await File.ReadAllTextAsync(htmlPath);
			
			// act
			var result = parser.Parse(html);
			
			// assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result.Has404Error, Is.True);
			Assert.That(result.Has500Error, Is.Null);
			Assert.That(result.TotalCount, Is.EqualTo(totalCount));
			Assert.That(result.TotalPageCount, Is.EqualTo(totalPageNum));
		}
		
		[Test]
		[TestCase("../../../Content/wod-2025-01-01-rowerg-wrong.html")]
		public async Task Parse_WithWrong_ShouldWork(string htmlPath)
		{
			// arrange
			var parser = new WodParser();
			var html = await File.ReadAllTextAsync(htmlPath);
			
			// act
			var result = parser.Parse(html);
			
			// assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result.Has404Error, Is.Null);
			Assert.That(result.Has500Error, Is.True);
			Assert.That(result.TotalCount, Is.EqualTo(null));
			Assert.That(result.TotalCount, Is.EqualTo(null));
			Assert.That(result.TotalPageCount, Is.EqualTo(null));
		}
		
		[Test]
		[TestCase("../../../Content/wod-2025-01-21-rowerg-time.html", 50, 1847, 37)]
		public async Task Parse_WithResultInTime_ShouldWork(string htmlPath, int rowCount, int totalCount, int totalPageNum)
		{
			// arrange
			var parser = new WodParser();
			var html = await File.ReadAllTextAsync(htmlPath);
			
			// act
			var result = parser.Parse(html);
			
			// assert
			AssertWodResult(result, rowCount, totalCount, totalPageNum);

			Assert.That(result.Items.Count(x => x.ResultMeters.HasValue), Is.EqualTo(0));
			Assert.That(result.Items.Count(x => x.ResultTime.HasValue), Is.EqualTo(rowCount));
		}
		
		[Test]
		[TestCase("../../../Content/wod-2025-01-25-rowerg-meters.html", 50, 1704, 35)]
		public async Task Parse_WithResultInMeters_ShouldWork(string htmlPath, int rowCount, int totalCount, int totalPageNum)
		{
			// arrange
			var parser = new WodParser();
			var html = await File.ReadAllTextAsync(htmlPath);
			
			// act
			var result = parser.Parse(html);
			
			// assert
			AssertWodResult(result, rowCount, totalCount, totalPageNum);
			
			Assert.That(result.Items.Count(x => x.ResultMeters.HasValue), Is.EqualTo(rowCount));
			Assert.That(result.Items.Count(x => x.ResultTime.HasValue), Is.EqualTo(0));
		}
		
		[Test]
		[TestCase("../../../Content/wod-2022-09-06-rowerg-only-pace.html", 50, 1862, 38)]
		public async Task Parse_WithResultOnlyPace_ShouldWork(string htmlPath, int rowCount, int totalCount, int totalPageNum)
		{
			// arrange
			var parser = new WodParser();
			var html = await File.ReadAllTextAsync(htmlPath);
			
			// act
			var result = parser.Parse(html);
			
			// assert
			AssertWodResult(result, rowCount, totalCount, totalPageNum, 49);
			
			Assert.That(result.Items.Count(x => x.ResultMeters.HasValue), Is.EqualTo(0));
			Assert.That(result.Items.Count(x => x.ResultTime.HasValue), Is.EqualTo(0));
		}

		private static void AssertWodResult(WodResult result, int rowCount, int totalCount, int totalPageNum, int? ageCount = null)
		{
			Assert.That(result, Is.Not.Null);
			Assert.That(result.Has404Error, Is.Null);
			Assert.That(result.Has500Error, Is.Null);
			Assert.That(result.Name, Is.Not.Null);
			Assert.That(result.Description, Is.Not.Null);
			Assert.That(result.TotalCount, Is.EqualTo(totalCount));
			Assert.That(result.TotalPageCount, Is.EqualTo(totalPageNum));
			Assert.That(result.Items, Has.Count.EqualTo(rowCount));
			Assert.That(result.Items.Count(x => x.Id.HasValue), Is.EqualTo(rowCount));
			Assert.That(result.Items.Select(x => x.Id!.Value).Distinct().Count(), Is.EqualTo(rowCount));
			Assert.That(result.Items.Count(x => x.Position.HasValue), Is.EqualTo(rowCount));
			Assert.That(result.Items.Count(x => x.Name != null), Is.EqualTo(rowCount));
			Assert.That(result.Items.Count(x => x.Age.HasValue), Is.EqualTo(ageCount ?? rowCount));
			Assert.That(result.Items.Count(x => x.Pace.HasValue), Is.EqualTo(rowCount));
		}
	}
}