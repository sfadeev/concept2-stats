using Concept2Stats.Models;
using Concept2Stats.Services;

namespace Concept2Stats.Tests.Services
{
	public class WodParserTests
	{
		[Test]
		[TestCase("../../../Content/wod-2025-01-01-rowerg-no-result.html")]
		public async Task Parse_WithNoResult_ShouldWork(string htmlPath)
		{
			// arrange
			var parser = new WodParser();
			var html = await File.ReadAllTextAsync(htmlPath);
			
			// act
			var result = parser.Parse(html);
			
			// assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result.Success, Is.False);
		}
		
		[Test]
		[TestCase("../../../Content/wod-2025-01-21-rowerg-time.html", 50)]
		public async Task Parse_WithResultInTime_ShouldWork(string htmlPath, int rowCount)
		{
			// arrange
			var parser = new WodParser();
			var html = await File.ReadAllTextAsync(htmlPath);
			
			// act
			var result = parser.Parse(html);
			
			// assert
			AssertWodResult(result, rowCount);

			Assert.That(result.Items.Count(x => x.ResultMeters.HasValue), Is.EqualTo(0));
			Assert.That(result.Items.Count(x => x.ResultTime.HasValue), Is.EqualTo(rowCount));
		}
		
		[Test]
		[TestCase("../../../Content/wod-2025-01-25-rowerg-meters.html", 50)]
		public async Task Parse_WithResultInMeters_ShouldWork(string htmlPath, int rowCount)
		{
			// arrange
			var parser = new WodParser();
			var html = await File.ReadAllTextAsync(htmlPath);
			
			// act
			var result = parser.Parse(html);
			
			// assert
			AssertWodResult(result, rowCount);
			
			Assert.That(result.Items.Count(x => x.ResultMeters.HasValue), Is.EqualTo(rowCount));
			Assert.That(result.Items.Count(x => x.ResultTime.HasValue), Is.EqualTo(0));
		}

		private static void AssertWodResult(WodResult result, int rowCount)
		{
			Assert.That(result, Is.Not.Null);
			Assert.That(result.Success, Is.True);
			Assert.That(result.Name, Is.Not.Null);
			Assert.That(result.Description, Is.Not.Null);
			Assert.That(result.Items, Has.Count.EqualTo(rowCount));
			Assert.That(result.Items.Count(x => x.Id.HasValue), Is.EqualTo(rowCount));
			Assert.That(result.Items.Select(x => x.Id!.Value).Distinct().Count(), Is.EqualTo(rowCount));
			Assert.That(result.Items.Count(x => x.Position.HasValue), Is.EqualTo(rowCount));
			Assert.That(result.Items.Count(x => x.Name != null), Is.EqualTo(rowCount));
			Assert.That(result.Items.Count(x => x.Age.HasValue), Is.EqualTo(rowCount));
			Assert.That(result.Items.Count(x => x.Pace.HasValue), Is.EqualTo(rowCount));
		}
	}
}