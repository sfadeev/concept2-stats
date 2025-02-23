using Quartz;

namespace C2Stats.Services.Jobs
{
	[DisallowConcurrentExecution]
	public class DownloadYesterdayWodJob(ILogger<DownloadYesterdayWodJob> logger,
		IHealthcheckService healthcheckService, IWodFileStorage wodFileStorage) 
		: AbstractJob(logger, healthcheckService)
	{
		// number of yesterdays to re-download - in case some devices were offline and submitted logs later
		private const int MaxDayCount = 3;
		
		protected override async Task ExecuteAsync(CancellationToken cancellationToken)
		{
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