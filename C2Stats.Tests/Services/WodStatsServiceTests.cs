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
			var result = await service.GetYear(2025, WodType.RowErg, cancellationToken);
			
			// assert
			Assert.That(result, Is.Not.Null);
		}
	}
}