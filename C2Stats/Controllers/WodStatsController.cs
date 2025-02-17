using C2Stats.Services;
using Microsoft.AspNetCore.Mvc;

namespace C2Stats.Controllers
{
	[ApiController, Route("api/wod/[action]")]
	public class WodController(IWodStatsService wodStatsService) : ControllerBase
	{
		[HttpGet]
		public async Task<IActionResult> Year(int year, string wodType, CancellationToken cancellationToken)
		{
			var result = await wodStatsService.GetYear(year, wodType, cancellationToken);

			return Ok(result);
		}
		
		[HttpGet]
		public async Task<IActionResult> Day(string wodType, CancellationToken cancellationToken)
		{
			var day = DateOnly.FromDateTime(DateTime.UtcNow);
			
			var result = await wodStatsService.GetDay(day, wodType, cancellationToken);

			return Ok(result);
		}
	}
}