using Quartz;

namespace C2Stats.Services.Jobs
{
	[DisallowConcurrentExecution]
	public class DownloadCurrentWodJob(ILogger<DownloadCurrentWodJob> logger,
		IHealthcheckService healthcheckService, ITimeZoneDateProvider timeZoneDateProvider, IWodFileStorage wodFileStorage) 
		: AbstractJob(logger, healthcheckService)
	{
		protected override async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			foreach (var date in timeZoneDateProvider.GetDatesInAllTimeZones(DateTime.UtcNow))
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