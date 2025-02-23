using C2Stats.Entities;
using C2Stats.Models;
using LinqToDB;
using LinqToDB.Data;

namespace C2Stats.Services
{
	public interface IWodDbStorage
	{
		Task<int> Sync(WodResult wod, CancellationToken cancellationToken);
	}

	public class WodDbStorage(ILogger<WodDbStorage> logger) : IWodDbStorage
	{
		public async Task<int> Sync(WodResult wod, CancellationToken cancellationToken)
		{
			try
			{
				var dbWod = new DbWod
				{
					Id = BuildWodId(wod),
					Date = wod.Date!.Value,
					Type = wod.Type,
					Name = wod.Name,
					Description = wod.Description,
					TotalCount = wod.TotalCount,
					LastModified = DateTime.UtcNow
				};

				var dbWodItems = wod.Items.Select(x => new DbWodItem
				{
					WodId = dbWod.Id,
					ProfileId = x.Id!.Value,
					Position = x.Position!.Value,
					Age = (short?)x.Age,
					ResultTime = x.ResultTime,
					ResultMeters = x.ResultMeters,
					Pace = x.Pace!.Value
				}).ToList();
				
				using (var db = new DataConnection())
				{
					await db.GetTable<DbWod>().MergeOnPk([dbWod], cancellationToken);
					
					var result = await db.GetTable<DbWodItem>().MergeOnPk(dbWodItems, cancellationToken);
					
					if (logger.IsEnabled(LogLevel.Information))
					{
						var count = await db.GetTable<DbWodItem>().Where(x => x.WodId == dbWod.Id).CountAsync(cancellationToken);

						logger.LogInformation(
							"WoD {Date} {WodType} saved to database - merged {MergedCount} item(s), total {TotalCount} item(s)",
							wod.Date, wod.Type, result, count);	
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
		
		public static int BuildWodId(WodResult wod)
		{
			var date = wod.Date!.Value;

			var wodTypeId = wod.Type switch
			{
				WodType.RowErg => 1,
				WodType.BikeErg => 2,
				WodType.SkiErg => 3,
				_ => throw new ArgumentOutOfRangeException(nameof(wod.Type), wod.Type, "Unknown wod type")
			};

			return date.Year * 100000 + date.Month * 1000 + date.Day * 10 + wodTypeId;
		}
	}
}