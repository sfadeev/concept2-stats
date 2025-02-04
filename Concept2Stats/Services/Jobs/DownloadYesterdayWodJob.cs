using System.Diagnostics;
using Quartz;

namespace Concept2Stats.Services.Jobs
{
	[DisallowConcurrentExecution]
	public class DownloadYesterdayWodJob(ILogger<DownloadYesterdayWodJob> logger,
		IHealthcheckService healthcheckService, IWodFileStorage wodFileStorage) : IJob
	{
		// number of yesterdays to re-download - in case some devices were offline and submitted logs later
		private const int MaxDayCount = 2;
		
		public async Task Execute(IJobExecutionContext context)
		{
			var sw = Stopwatch.StartNew();
			
			await ExecuteAsync(context.CancellationToken);
			
			logger.LogDebug("Job completed, elapsed {Elapsed}", sw.Elapsed);
		}

		private async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			await healthcheckService.Success(cancellationToken);
			
			var dates = Enumerable.Range(1, MaxDayCount)
				.Select(i => DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-i))
				.ToList();
			
			foreach (var date in dates)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					cancellationToken.ThrowIfCancellationRequested();
				}
				
				try
				{
					await wodFileStorage.DownloadAndStoreActual(date, cancellationToken);
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "Failed to download WoDs as of {Date}", date);
				}
			}
		}
	}
}