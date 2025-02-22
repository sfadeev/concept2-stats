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
					var result = await db.GetTable<DbProfile>()
						.Merge()
						.Using(profiles)
						.OnTargetKey()
						.UpdateWhenMatched()
						.InsertWhenNotMatched()
						.MergeAsync(cancellationToken);
					
					logger.LogInformation("Merged {Count} profiles to db", result);
					
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