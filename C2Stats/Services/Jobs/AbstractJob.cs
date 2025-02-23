using System.Diagnostics;
using Quartz;

namespace C2Stats.Services.Jobs
{
	public abstract class AbstractJob(ILogger logger, IHealthcheckService healthcheckService) : IJob
	{
		public async Task Execute(IJobExecutionContext context)
		{
			await healthcheckService.Success(context.CancellationToken);
			
			var sw = Stopwatch.StartNew();
			
			await ExecuteAsync(context.CancellationToken);
			
			logger.LogDebug("Job {Job} completed, elapsed {Elapsed}", context.JobDetail.Key, sw.Elapsed);
		}

		protected abstract Task ExecuteAsync(CancellationToken cancellationToken);
	}
}