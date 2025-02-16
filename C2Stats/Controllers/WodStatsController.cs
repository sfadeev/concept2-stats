using C2Stats.Services;
using Microsoft.AspNetCore.Mvc;

namespace C2Stats.Controllers
{
	[ApiController, Route("api/[controller]/[action]")]
	public class WodStatsController(IWodStatsService wodStatsService) : ControllerBase
	{
		[HttpGet()]
		public async Task<IActionResult> Year(int year, string wodType, CancellationToken cancellationToken)
		{
			var result = await wodStatsService.GetYear(year, wodType, cancellationToken);

			return Ok(result); // Returns JSON array
		}
	}
}