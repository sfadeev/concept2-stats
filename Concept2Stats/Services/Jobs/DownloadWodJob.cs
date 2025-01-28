using System.Text.Json;
using System.Text.Json.Serialization;
using Concept2Stats.Models;
using Microsoft.Extensions.Options;
using Quartz;

namespace Concept2Stats.Services.Jobs
{
	[DisallowConcurrentExecution]
	public class DownloadWodJob(IHealthcheckService healthcheckService) : IJob
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
	public class DownloadWodArchiveJob(ILogger<DownloadWodArchiveJob> logger, IOptions<AppOptions> appOptions,
		IHealthcheckService healthcheckService, IWodDownloader wodDownloader) : IJob
	{
		public static readonly DateOnly FirstWodDate = new(2022, 7, 8); // Jul 8 2022 is the date of first WoD

		public static readonly string[] DownloadedWodTypes = [ WodType.RowErg ];
		
		public static readonly int MaxErrorsCount = 10;
		
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
			
			// begin from yesterday - current date will be downloaded by other job
			var now = DateOnly.FromDateTime(DateTime.Today).AddDays(-1);
			
			var errorsCount = 0;
			
			while (now >= FirstWodDate)
			{
				foreach (var wodType in DownloadedWodTypes)
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
						await DownloadAndStore(now, wodType, cancellationToken);
					}
					catch (Exception ex)
					{
						logger.LogError(ex, "Failed to download wod {WodType} as of {Date}", wodType, now);

						errorsCount++;
					}
				}

				now = now.AddDays(-1);
			}
		}

		private async Task DownloadAndStore(DateOnly now, string wodType, CancellationToken cancellationToken)
		{
			var options = appOptions.Value;
			
			var path = GetFilePath(options.ParseDirPath, now, wodType);

			if (File.Exists(path) == false)
			{
				logger.LogDebug("File {Path} does not exists, starting download.", path);

				var wod = await wodDownloader.Download(now, wodType, cancellationToken);

				var wodJson = JsonSerializer.Serialize(wod, JsonOptions);

				Directory.CreateDirectory(Path.GetDirectoryName(path)!);

				await File.WriteAllTextAsync(path, wodJson, cancellationToken);

				logger.LogDebug("File {Path} successfully stored, {ItemCount} items, {FileSize}.", path, wod.Items.Count, wodJson.Length);
			}
		}

		public static string GetFilePath(string basePath, DateOnly date, string wodType)
		{
			return Path.Combine(basePath, $"wod/{date.Year}/{date.Year}-{date.Month:00}-{date.Day:00}-{wodType}.json");
		}
	}
}