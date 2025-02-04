using System.Diagnostics;
using Quartz;

namespace Concept2Stats.Services.Jobs
{
	[DisallowConcurrentExecution]
	public class DownloadCurrentWodJob(ILogger<DownloadCurrentWodJob> logger,
		IHealthcheckService healthcheckService, IWodFileStorage wodFileStorage) : IJob
	{
		public async Task Execute(IJobExecutionContext context)
		{
			var sw = Stopwatch.StartNew();
			
			await ExecuteAsync(context.CancellationToken);
			
			logger.LogDebug("Job completed, elapsed {Elapsed}", sw.Elapsed);
		}

		private async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			await healthcheckService.Success(cancellationToken);

			foreach (var date in DateTimeHelper.GetDatesInAllTimeZones(DateTime.UtcNow))
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