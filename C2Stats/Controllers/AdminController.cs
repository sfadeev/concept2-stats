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
			await countryDbStorage.SyncAll(cancellationToken);

			return Ok();
		}
		
		[HttpGet]
		public async Task<IActionResult> SyncProfiles(CancellationToken cancellationToken)
		{
			await profileDbStorage.SyncAll(cancellationToken);

			return Ok();
		}		
	}
}