using System.Diagnostics;
using Quartz;

namespace Concept2Stats.Services.Jobs
{
	public abstract class AbstractJob(ILogger logger) : IJob
	{
		public async Task Execute(IJobExecutionContext context)
		{
			var sw = Stopwatch.StartNew();
			
			await ExecuteAsync(context.CancellationToken);
			
			logger.LogDebug("Job {Job} completed, elapsed {Elapsed}", context.JobDetail.Key, sw.Elapsed);
		}

		protected abstract Task ExecuteAsync(CancellationToken cancellationToken);
	}
}