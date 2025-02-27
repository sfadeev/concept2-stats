using C2Stats.Services;
using Microsoft.AspNetCore.Mvc;

namespace C2Stats.Controllers
{
	[ApiController, Route("api/wod/[action]")]
	public class WodController(IWodStatsService wodStatsService) : ControllerBase
	{
		[HttpGet]
		public async Task<IActionResult> Year(string type, int year, CancellationToken cancellationToken)
		{
			var result = await wodStatsService.GetYear(type, year, cancellationToken);

			return Ok(result);
		}
		
		[HttpGet]
		public async Task<IActionResult> Day(string type, DateTime? date, CancellationToken cancellationToken)
		{
			var day = DateOnly.FromDateTime(date ?? DateTime.UtcNow);
			
			var result = await wodStatsService.GetDay(type, day, cancellationToken);

			return Ok(result);
		}
	}
}