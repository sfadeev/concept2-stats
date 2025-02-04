using System.Diagnostics;
using Quartz;

namespace Concept2Stats.Services.Jobs
{
	[DisallowConcurrentExecution]
	public class DownloadArchiveWodJob(ILogger<DownloadArchiveWodJob> logger,
		IHealthcheckService healthcheckService, IWodFileStorage wodFileStorage) : IJob
	{
		private static readonly DateOnly FirstWodDate = new(2022, 7, 8); // Jul 8 2022 is the date of first WoD

		private const int MaxErrorsCount = 10;

		public async Task Execute(IJobExecutionContext context)
		{
			var sw = Stopwatch.StartNew();
			
			await ExecuteAsync(context.CancellationToken);
			
			logger.LogDebug("Job completed, elapsed {Elapsed}", sw.Elapsed);
		}

		private async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			await healthcheckService.Success(cancellationToken);
			
			// begin from day before yesterday - current date will be downloaded by other jobs
			var now = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-2);
			
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