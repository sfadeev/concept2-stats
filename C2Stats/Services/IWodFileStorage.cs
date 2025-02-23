using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using C2Stats.Models;
using C2Stats.Notifications;
using MediatR;
using Microsoft.Extensions.Options;

namespace C2Stats.Services
{
	public interface IWodFileStorage
	{
		Task DownloadAndStoreActual(DateOnly date, CancellationToken cancellationToken);
		
		Task DownloadAndStoreIfNotExists(DateOnly date, CancellationToken cancellationToken);
		
		Task<ICollection<WodFileInfo>> GetWodFiles(CancellationToken cancellationToken);
		
		Task<WodResult> GetWodResult(DateOnly date, string wodType, CancellationToken cancellationToken);
	}
	
	public class WodFileInfo
	{
		public required string Path { get; set; }
		
		public DateOnly Date { get; set; }
		
		public required string Type { get; set; }
		
		public DateTime? LastModified { get; set; }
	}
	
	public class WodFileStorage(ILogger<WodFileStorage> logger,
		IOptions<AppOptions> appOptions, IWodDownloader wodDownloader, IPublisher mediator) : IWodFileStorage
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

		public Task<ICollection<WodFileInfo>> GetWodFiles(CancellationToken cancellationToken)
		{
			var options = appOptions.Value;

			var wodDir = Path.Combine(options.ParseDirPath, "wod/");
			
			var files = Directory.EnumerateFiles(wodDir, "*.json", SearchOption.AllDirectories);

			var result = new List<WodFileInfo>();
			
			foreach (var file in files)
			{
				var fi = new FileInfo(file);

				if (DateOnly.TryParse(fi.Name[..10], out var date))
				{
					result.Add(new WodFileInfo
					{
						Path = file,
						Date = date,
						Type = fi.Name.Substring(
							fi.Name.LastIndexOf('-') + 1, 
							fi.Name.IndexOf('.') - fi.Name.LastIndexOf('-') - 1),
						LastModified = fi.LastWriteTime
					});
				}
			}

			return Task.FromResult<ICollection<WodFileInfo>>(result);
		}

		public async Task<WodResult> GetWodResult(DateOnly date, string wodType, CancellationToken cancellationToken)
		{
			var path = GetFilePath(date, wodType);
			
			var json = await File.ReadAllTextAsync(path, cancellationToken);
				
			var wod = JsonSerializer.Deserialize<WodResult>(json, JsonOptions);
			
			return wod!;
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

			IWodDownloadCancellationChecker? cancellationChecker = null;
			
			if (File.Exists(path))
			{
				var currentJson = await File.ReadAllTextAsync(path, cancellationToken);
				
				var currentWod = JsonSerializer.Deserialize<WodResult>(currentJson, JsonOptions);

				if (currentWod != null)
				{
					cancellationChecker = new GenericWodDownloadCancellationChecker(context =>
					{
						if (context.CountryId == null && (context.Result.TotalCount ?? 0) == currentWod.Items.Count)
						{
							context.CancelReason = $"WoD result have the same items as current saved result ({context.Result.TotalCount}).";
							
							return true;
						}
						
						return false;
					});
				}
			}
			
			var wod = await wodDownloader.Download(date, wodType, cancellationChecker, cancellationToken);

			if (wod != null) // downloaded and should be saved
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
				
				await mediator.Publish(new WodFileUpdated { Wod = wod }, cancellationToken);
			}
		}
		
		private string GetFilePath(DateOnly date, string wodType)
		{
			var options = appOptions.Value;
			
			return Path.Combine(options.ParseDirPath, $"wod/{date.Year}/{date.Year}-{date.Month:00}-{date.Day:00}-{wodType}.json");
		}
	}
}