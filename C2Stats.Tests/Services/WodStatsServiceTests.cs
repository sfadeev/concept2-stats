using C2Stats.Models;
using C2Stats.Services;

namespace C2Stats.Tests.Services
{
	public class WodStatsServiceTests
	{
		[Test, CancelAfter(1000)]
		[TestCase(WodType.RowErg, 2025)]
		public async Task GetCountries_NormalState_ShouldWork(string type, int year, CancellationToken cancellationToken = default)
		{
			// arrange
			var service = new WodStatsService();
			
			// act
			var result = await service.GetCountries(type, year, cancellationToken);
			
			// assert
			Assert.That(result, Is.Not.Null);
		}
		
		[Test, CancelAfter(1000)]
		[TestCase(null)]
		[TestCase("RUS")]
		public async Task GetYear_NormalState_ShouldWork(string? country, CancellationToken cancellationToken = default)
		{
			// arrange
			var service = new WodStatsService();
			
			// act
			var result = await service.GetYear(WodType.RowErg, 2025, country, cancellationToken);
			
			// assert
			Assert.That(result, Is.Not.Null);
		}
		
		[Test, CancelAfter(1000)]
		[TestCase(null)]
		[TestCase("RUS")]
		public async Task GetDay_NormalState_ShouldWork(string? country, CancellationToken cancellationToken = default)
		{
			// arrange
			var service = new WodStatsService();
			
			// act
			var result = await service.GetDay(WodType.RowErg, DateOnly.FromDateTime(DateTime.UtcNow), country, cancellationToken);
			
			// assert
			Assert.That(result, Is.Not.Null);
		}
	}
}