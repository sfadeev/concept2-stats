using C2Stats.Entities;
using LinqToDB;
using LinqToDB.Data;

namespace C2Stats.Services
{
	public interface IProfileDbStorage
	{
		Task<int> Sync(ICollection<DbProfile> profiles, CancellationToken cancellationToken);
		
		Task<int> SyncAll(CancellationToken cancellationToken);
	}

	public class ProfileDbStorage(
		ILogger<ProfileDbStorage> logger, IProfileFileStorage profileFileStorage) : IProfileDbStorage
	{
		public async Task<int> Sync(ICollection<DbProfile> profiles, CancellationToken cancellationToken)
		{
			try
			{
				using (var db = new DataConnection())
				{
					var table = db.GetTable<DbProfile>();
					
					var result = await table
						.Merge()
						.Using(profiles)
						.OnTargetKey()
						.UpdateWhenMatched()
						.InsertWhenNotMatched()
						.MergeAsync(cancellationToken);

					if (logger.IsEnabled(LogLevel.Information))
					{
						var count = await table.CountAsync(cancellationToken);

						logger.LogInformation("Merged {MergedCount} profiles to db, total {TotalCount} profiles", result, count);	
					}
					
					return result;
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed to merge profiles to db - {ErrorMessage}", ex.Message);

				throw;
			}
		}

		public async Task<int> SyncAll(CancellationToken cancellationToken)
		{
			return await Sync(profileFileStorage.GetProfiles(), cancellationToken);
		}
	}
}