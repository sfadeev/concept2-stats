using C2Stats.Models;
using Microsoft.Extensions.Options;
using Quartz;

namespace C2Stats.Services.Jobs
{
	[DisallowConcurrentExecution]
	public class DownloadYesterdayWodJob(ILogger<DownloadYesterdayWodJob> logger, IOptions<AppOptions> appOptions,
		IHealthcheckService healthcheckService, IWodFileStorage wodFileStorage) 
		: AbstractJob(logger, healthcheckService)
	{
		protected override async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			var options = appOptions.Value;
			
			var dates = Enumerable.Range(1, options.DownloadYesterdayDays)
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