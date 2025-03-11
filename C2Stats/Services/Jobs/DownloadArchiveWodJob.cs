using C2Stats.Models;
using Microsoft.Extensions.Options;
using Quartz;

namespace C2Stats.Services.Jobs
{
	[DisallowConcurrentExecution]
	public class DownloadArchiveWodJob(ILogger<DownloadArchiveWodJob> logger, IOptions<AppOptions> appOptions,
		IHealthcheckService healthcheckService, IWodFileStorage wodFileStorage) 
		: AbstractJob(logger, healthcheckService)
	{
		private static readonly DateOnly FirstWodDate = new(2022, 7, 8); // Jul 8 2022 is the date of first WoD

		private const int MaxErrorsCount = 10;

		protected override async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			var options = appOptions.Value;
			
			// begin from day before yesterday - current date will be downloaded by other jobs
			var now = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-options.DownloadArchiveDaysFrom);
			
			var errorsCount = 0;
			
			while (now >= FirstWodDate)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					cancellationToken.ThrowIfCancellationRequested();
				}

				if (errorsCount >= MaxErrorsCount)
				{
					logger.LogDebug("Maximum allowed errors count achieved ({ErrorsCount}), job cancelled till next run.", errorsCount);
						
					break;
				}

				try
				{
					await wodFileStorage.DownloadAndStoreIfNotExists(now, cancellationToken);
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "Failed to download WoDs as of {Date}", now);

					errorsCount++;
				}

				now = now.AddDays(-1);
			}
		}
	}
}