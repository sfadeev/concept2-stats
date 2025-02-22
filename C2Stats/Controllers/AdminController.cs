using C2Stats.Services;
using Microsoft.AspNetCore.Mvc;

namespace C2Stats.Controllers
{
	[ApiController, Route("api/admin/[action]")]
	public class AdminController(IProfileDbStorage profileDbStorage) : ControllerBase
	{
		[HttpGet]
		public async Task<IActionResult> SyncProfiles(CancellationToken cancellationToken)
		{
			await profileDbStorage.SyncAll(cancellationToken);

			return Ok();
		}		
	}
}