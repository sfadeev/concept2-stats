using C2Stats.Services;
using Microsoft.AspNetCore.Mvc;

namespace C2Stats.Controllers
{
	[ApiController, Route("api/wod/[action]")]
	public class WodController(IWodStatsService wodStatsService) : ControllerBase
	{
		[HttpGet]
		public async Task<IActionResult> Profiles(string? search, CancellationToken cancellationToken)
		{
			var result = await wodStatsService.GetProfiles(search, cancellationToken);

			return Ok(result);
		}
		
		[HttpGet]
		public async Task<IActionResult> Countries(string type, int year, CancellationToken cancellationToken)
		{
			var result = await wodStatsService.GetCountries(type, year, cancellationToken);

			return Ok(result);
		}
		
		[HttpGet]
		public async Task<IActionResult> Year(string type, int year, string? country, CancellationToken cancellationToken)
		{
			var result = await wodStatsService.GetYear(type, year, country, cancellationToken);

			return Ok(result);
		}
		
		[HttpGet]
		public async Task<IActionResult> Day(string type, DateTime? date, string? country, CancellationToken cancellationToken)
		{
			var day = DateOnly.FromDateTime(date ?? DateTime.UtcNow);
			
			var result = await wodStatsService.GetDay(type, day, country, cancellationToken);

			return Ok(result);
		}
		
		[HttpGet]
		public async Task<IActionResult> WodItems(string type, DateTime? date, string? country, CancellationToken cancellationToken)
		{
			var day = DateOnly.FromDateTime(date ?? DateTime.UtcNow);
			
			var result = await wodStatsService.GetWodItems(type, day, country, cancellationToken);

			return Ok(result);
		}
	}
}