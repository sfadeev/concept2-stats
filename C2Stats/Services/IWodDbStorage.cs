using C2Stats.Entities;
using C2Stats.Models;
using LinqToDB;
using LinqToDB.Data;

namespace C2Stats.Services
{
	public interface IWodDbStorage
	{
		Task<int> Sync(WodResult wod, CancellationToken cancellationToken);
		
		int BuildWodId(DateOnly date, string wodType);
	}

	public class WodDbStorage(ILogger<WodDbStorage> logger) : IWodDbStorage
	{
		public async Task<int> Sync(WodResult wod, CancellationToken cancellationToken)
		{
			try
			{
				var dbWod = new DbWod
				{
					Id = BuildWodId(wod.Date!.Value, wod.Type!),
					Date = wod.Date!.Value,
					Type = wod.Type,
					Name = wod.Name,
					Description = wod.Description,
					TotalCount = wod.TotalCount,
					LastModified = DateTime.UtcNow
				};

				var dbWodItems = wod.Items.Select((x, i) => new DbWodItem
				{
					WodId = dbWod.Id,
					ProfileId = x.Id!.Value,
					No = i + 1,
					Position = x.Position!.Value,
					Age = (short?)x.Age,
					ResultTime = x.ResultTime,
					ResultMeters = x.ResultMeters,
					Pace = x.Pace
				}).ToList();
				
				using (var db = new DataConnection())
				{
					await db.GetTable<DbWod>().MergeOnPrimaryKey([dbWod], cancellationToken);
					
					var result = await db.GetTable<DbWodItem>().MergeOnPrimaryKey(dbWodItems, cancellationToken);
					
					if (logger.IsEnabled(LogLevel.Information))
					{
						logger.LogInformation(
							"WoD {Date} {WodType} saved to database - merged {MergedCount} item(s)", wod.Date, wod.Type, result);	
					}
					
					return result;
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed to save WoD to database - {ErrorMessage}", ex.Message);
				
				return -1;
			}
		}

		public int BuildWodId(DateOnly date, string wodType)
		{
			return date.Year * 100000 + date.Month * 1000 + date.Day * 10 + wodType switch
			{
				WodType.RowErg => 1,
				WodType.BikeErg => 2,
				WodType.SkiErg => 3,
				_ => throw new ArgumentOutOfRangeException(nameof(wodType), wodType, "Unknown wod type")
			};
		}
	}
}