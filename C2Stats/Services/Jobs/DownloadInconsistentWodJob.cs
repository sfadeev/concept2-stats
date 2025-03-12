using C2Stats.Entities;
using C2Stats.Models;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.Options;
using Quartz;

namespace C2Stats.Services.Jobs
{
	[DisallowConcurrentExecution]
	public class DownloadInconsistentWodJob(ILogger<DownloadInconsistentWodJob> logger, IOptions<AppOptions> appOptions,
		IHealthcheckService healthcheckService, IWodFileStorage wodFileStorage) : AbstractJob(logger, healthcheckService)
	{
		protected override async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			var wods = await GetInconsistentByTotalCount(cancellationToken);

			if (logger.IsEnabled(LogLevel.Debug))
			{
				logger.LogDebug("Inconsistent by TotalCount WoDs ({Count}) {WoDs}",
					wods.Count, wods.Select(x => new { x.Date, x.Type, x.TotalCount, x.WodItemCount }));
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

		/// <summary>
		/// WoD total count not equal count if WoD items
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		private async Task<ICollection<InconsistentByTotalCount>> GetInconsistentByTotalCount(CancellationToken cancellationToken)
		{
			var options = appOptions.Value;

			var notAfter = DateOnly.FromDateTime(DateTime.Today).AddDays(-options.DownloadYesterdayDays);
			
			using (var db = new DataConnection())
			{
				var query =
					from w in db.GetTable<DbWod>()
					join wig in from wi in db.GetTable<DbWodItem>()
						group wi by wi.WodId
						into wig
						select new { WodId = wig.Key, WodItemCount = wig.Count() }
						on w.Id equals wig.WodId
					where w.Date <= notAfter && w.TotalCount != wig.WodItemCount
					orderby w.Id descending
					select new InconsistentByTotalCount
					{
						Date = w.Date,
						Type = w.Type,
						TotalCount = w.TotalCount,
						WodItemCount = wig.WodItemCount
					};

				return await query.ToListAsync(cancellationToken);
			}
		}

		private class InconsistentByTotalCount
		{
			public DateOnly Date { get; set; }
			
			public string? Type { get; set; }
			
			public int? TotalCount { get; set; }
			
			public int WodItemCount { get; set; }
		}
	}
}