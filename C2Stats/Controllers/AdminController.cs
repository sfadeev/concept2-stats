using C2Stats.Services;
using Microsoft.AspNetCore.Mvc;

namespace C2Stats.Controllers
{
	[ApiController, Route("api/admin/[action]")]
	public class AdminController(ICountryDbStorage countryDbStorage, IProfileDbStorage profileDbStorage) : ControllerBase
	{
		[HttpGet]
		public async Task<IActionResult> SyncCountries(CancellationToken cancellationToken)
		{
			var result = await countryDbStorage.SyncAll(cancellationToken);

			return Ok(result);
		}
		
		[HttpGet]
		public async Task<IActionResult> SyncProfiles(CancellationToken cancellationToken)
		{
			var result = await profileDbStorage.SyncAll(cancellationToken);

			return Ok(result);
		}		
	}
}