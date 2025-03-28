using C2Stats.Models;
using C2Stats.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace C2Stats.Tests.Services
{
	public class WodDbStorageTests
	{
		[Test]
		[TestCase("2025-01-23", WodType.RowErg, 2025_01_23_1)]
		[TestCase("2025-02-24", WodType.BikeErg, 2025_02_24_2)]
		[TestCase("2025-03-25", WodType.SkiErg, 2025_03_25_3)]
		public void BuildWodId_NormalState_ShouldWork(string date, string wodType, int expectedId)
		{
			// arrange
			var service = new WodDbStorage(NullLogger<WodDbStorage>.Instance);
			
			// act
			var id = service.BuildWodId(DateOnly.Parse(date), wodType);
			
			// assert
			Assert.That(id, Is.EqualTo(expectedId));
		}
	}
}