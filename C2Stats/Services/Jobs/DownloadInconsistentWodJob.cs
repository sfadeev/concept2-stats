using C2Stats.Entities;
using LinqToDB;
using LinqToDB.Data;
using Quartz;

namespace C2Stats.Services.Jobs
{
	[DisallowConcurrentExecution]
	public class DownloadInconsistentWodJob(ILogger<DownloadInconsistentWodJob> logger,
		IHealthcheckService healthcheckService, IWodFileStorage wodFileStorage) : AbstractJob(logger, healthcheckService)
	{
		protected override async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			var wods = await GetInconsistentByTotalCount(cancellationToken);

			if (logger.IsEnabled(LogLevel.Debug))
			{
				logger.LogDebug("Inconsistent by TotalCount WoDs ({Count}) {WoDs}",
					wods.Count, wods.Select(x => new { x.Date, x.Type }));
			}
			
			foreach (var wod in wods)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					cancellationToken.ThrowIfCancellationRequested();
				}
				
				try
				{
					await wodFileStorage.DownloadAndStoreActual(wod.Date, [ wod.Type! ], cancellationToken);
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "Failed to download WoDs {Date}", new { wod.Date, wod.Type });
				}
			}
		}

		private static async Task<ICollection<DbWod>> GetInconsistentByTotalCount(CancellationToken cancellationToken)
		{
			// WoD total count not equal count if WoD items
			using (var db = new DataConnection())
			{
				var query =
					from w in db.GetTable<DbWod>()
					join wig in from wi in db.GetTable<DbWodItem>()
						group wi by wi.WodId
						into wig
						select new { WodId = wig.Key, WodItemCount = wig.Count() }
						on w.Id equals wig.WodId
					where w.TotalCount != wig.WodItemCount
					select w;

				return await query.ToListAsync(cancellationToken);
			}
		}
	}
}