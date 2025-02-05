using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Concept2Stats.Models;
using Microsoft.Extensions.Options;

namespace Concept2Stats.Services
{
	public interface IWodFileStorage
	{
		Task DownloadAndStoreActual(DateOnly date, CancellationToken cancellationToken);
		
		Task DownloadAndStoreIfNotExists(DateOnly date, CancellationToken cancellationToken);
	}
	
	public class WodFileStorage(ILogger<WodFileStorage> logger,
		IOptions<AppOptions> appOptions, IWodDownloader wodDownloader) : IWodFileStorage
	{
		private static readonly string[] DownloadedWodTypes = [ WodType.RowErg, WodType.BikeErg, WodType.SkiErg ];
		
		private static readonly JsonSerializerOptions JsonOptions = new()
		{
			WriteIndented = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
		};

		public Task DownloadAndStoreActual(DateOnly date, CancellationToken cancellationToken)
		{
			return DownloadAndStoreInParallel(date, true, cancellationToken);
		}

		public Task DownloadAndStoreIfNotExists(DateOnly date, CancellationToken cancellationToken)
		{
			return DownloadAndStoreInParallel(date, false, cancellationToken);
		}

		private async Task DownloadAndStoreInParallel(DateOnly date, bool overwriteIfExists, CancellationToken cancellationToken)
		{
			var tasks = DownloadedWodTypes
				.Select(x => DownloadAndStoreInternal(date, x, overwriteIfExists, cancellationToken));
				
			await Task.WhenAll(tasks);
		}
		
		private async Task DownloadAndStoreInternal(DateOnly date, string wodType, bool overwriteIfExists, CancellationToken cancellationToken)
		{
			var sw = Stopwatch.StartNew();
			
			var path = GetFilePath(date, wodType);

			if (overwriteIfExists)
			{
				logger.LogDebug("Force download file {Path}.", path);
			}
			else if (File.Exists(path) == false)
			{
				logger.LogDebug("File {Path} does not exists, starting download.", path);
			}
			else
			{
				return;
			}

			WodResult? currentWod = null;
			IWodDownloadCancellationChecker? cancellationChecker = null;
			
			if (File.Exists(path))
			{
				var currentJson = await File.ReadAllTextAsync(path, cancellationToken);
				
				currentWod = JsonSerializer.Deserialize<WodResult>(currentJson, JsonOptions);

				if (currentWod != null)
				{
					cancellationChecker = new WodResultItemCountDownloadCancellationChecker(currentWod.Items.Count);
				}
			}
			
			var wod = await wodDownloader.Download(date, wodType, cancellationChecker, cancellationToken);

			if (currentWod == null || currentWod.Items.Count != wod.Items.Count)
			{
				Directory.CreateDirectory(Path.GetDirectoryName(path)!);
			
				var json = JsonSerializer.Serialize(wod, JsonOptions);

				await File.WriteAllTextAsync(path, json, cancellationToken);

				if (logger.IsEnabled(LogLevel.Information))
				{
					logger.LogInformation(
						"File {Path} saved, elapsed {Elapsed} ({ItemCount} items, {FileSize} bytes)",
						path, sw.Elapsed, wod.Items.Count, json.Length);	
				}
			}
		}
		
		private string GetFilePath(DateOnly date, string wodType)
		{
			var options = appOptions.Value;
			
			return Path.Combine(options.ParseDirPath, $"wod/{date.Year}/{date.Year}-{date.Month:00}-{date.Day:00}-{wodType}.json");
		}

		private class WodResultItemCountDownloadCancellationChecker(int currentCount) : IWodDownloadCancellationChecker
		{
			public bool ShouldCancel(WodDownloadProgressContext context, out string? reason)
			{
				if (context.Result.TotalCount == currentCount)
				{
					reason = $"WoD result has same items count ({currentCount}).";
					return true;
				}
				
				reason = null;
				return false;
			}
		}
	}
}