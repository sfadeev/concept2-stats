using C2Stats.Models;
using C2Stats.Services;

namespace C2Stats.Tests.Services
{
	public class WodStatsServiceTests
	{
		[Test, CancelAfter(1000)]
		public async Task GetYear_NormalState_ShouldWork(CancellationToken cancellationToken = default)
		{
			// arrange
			var service = new WodStatsService();
			
			// act
			var result = await service.GetYear(WodType.RowErg, 2025, null, cancellationToken);
			
			// assert
			Assert.That(result, Is.Not.Null);
		}
		
		[Test, CancelAfter(1000)]
		public async Task GetDay_NormalState_ShouldWork(CancellationToken cancellationToken = default)
		{
			// arrange
			var service = new WodStatsService();
			
			// act
			var result = await service.GetDay(WodType.RowErg, DateOnly.FromDateTime(DateTime.UtcNow), null, cancellationToken);
			
			// assert
			Assert.That(result, Is.Not.Null);
		}
	}
}