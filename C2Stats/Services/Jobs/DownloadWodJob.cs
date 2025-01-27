using System.Text.Json;
using System.Text.Json.Serialization;
using C2Stats.Models;
using Microsoft.Extensions.Options;
using Quartz;

namespace C2Stats.Services.Jobs
{
	[DisallowConcurrentExecution]
	public class DownloadWodJob(ILogger<DownloadWodJob> logger,
		IHealthcheckService healthcheckService, IMessageSender messageSender) : IJob
	{
		public async Task Execute(IJobExecutionContext context)
		{
			await ExecuteAsync(context.CancellationToken);
		}

		private async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			await healthcheckService.Success(cancellationToken);
		}
	}
	
	[DisallowConcurrentExecution]
	public class DownloadWodArchiveJob(ILogger<DownloadWodJob> logger, IOptions<AppOptions> appOptions,
		IHealthcheckService healthcheckService, IMessageSender messageSender,
		IWodDownloader wodDownloader) : IJob
	{
		private static readonly JsonSerializerOptions JsonOptions = new()
		{
			WriteIndented = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
		};
		
		public async Task Execute(IJobExecutionContext context)
		{
			await ExecuteAsync(context.CancellationToken);
		}

		private async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			await healthcheckService.Success(cancellationToken);

			var options = appOptions.Value;

			var beginDate = DateOnly.FromDateTime(DateTime.Now);
			var endDate = new DateOnly(2024, 1, 1);

			var wodType = WodType.RowErg;
			var now = beginDate;
			
			while (now >= endDate)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					cancellationToken.ThrowIfCancellationRequested();
				}
				
				try
				{
					var path = GetFilePath(options.ParseDirPath, now, wodType);

					if (File.Exists(path) == false)
					{
						logger.LogDebug("File {Path} not exists, starting download.", path);

						var wod = await wodDownloader.Download(now, wodType, cancellationToken);

						var wodJson = JsonSerializer.Serialize(wod, JsonOptions);

						Directory.CreateDirectory(Path.GetDirectoryName(path)!);

						await File.WriteAllTextAsync(path, wodJson, cancellationToken);
						
						logger.LogDebug("File {Path} successfully stored.", path);
					}
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "Failed to download wod {WodType} as of {Date}", wodType, now);
				}
				
				now = now.AddDays(-1);
			}
		}

		public static string GetFilePath(string basePath, DateOnly date, string wodType)
		{
			return Path.Combine(basePath, $"wod/{date.Year}/{date.Year}-{date.Month:00}-{date.Day:00}-{wodType}.json");
		}
	}
}