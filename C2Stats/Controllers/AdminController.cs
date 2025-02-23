using C2Stats.Entities;
using C2Stats.Services;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.AspNetCore.Mvc;

namespace C2Stats.Controllers
{
	[ApiController, Route("api/admin/[action]")]
	public class AdminController(
		ICountryDbStorage countryDbStorage, 
		IProfileDbStorage profileDbStorage,
		IWodFileStorage wodFileStorage, IWodDbStorage wodDbStorage) : ControllerBase
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
		
		[HttpGet]
		public async Task<IActionResult> SyncWods(CancellationToken cancellationToken)
		{
			var wodFiles = await wodFileStorage.GetWodFiles(cancellationToken);

			foreach (var wodFile in wodFiles)
			{
				var wodId = wodDbStorage.BuildWodId(wodFile.Date, wodFile.Type);

				DbWod? dbWod;
				
				using (var db = new DataConnection())
				{
					dbWod = await db.GetTable<DbWod>().Where(x => x.Id == wodId).FirstOrDefaultAsync(cancellationToken);
				}

				if (dbWod?.LastModified == null || dbWod.LastModified < wodFile.LastModified)
				{
					var wodResult = await wodFileStorage.GetWodResult(wodFile.Date, wodFile.Type, cancellationToken);
					
					await wodDbStorage.Sync(wodResult, cancellationToken);
				}
			}

			return Ok(wodFiles);
		}		
	}
}