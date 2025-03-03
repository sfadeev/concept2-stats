using C2Stats.Entities;
using C2Stats.Services;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.AspNetCore.Mvc;

namespace C2Stats.Controllers
{
	[ApiController, Route("api/wod/[action]")]
	public class WodController(IWodStatsService wodStatsService) : ControllerBase
	{
		[HttpGet]
		public async Task<IActionResult> Countries(CancellationToken cancellationToken)
		{
			using (var db = new DataConnection())
			{
				var result = await db.GetTable<DbCountry>()
					.OrderBy(x => x.Name)
					.Select(x => new { x.Code, x.Name })
					.ToListAsync(cancellationToken);
				
				return Ok(result);
			}
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
	}
}