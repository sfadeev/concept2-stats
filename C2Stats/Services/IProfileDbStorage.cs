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

	public class ProfileDbStorage(ILogger<ProfileDbStorage> logger, IProfileFileStorage profileFileStorage) : IProfileDbStorage
	{
		public async Task<int> Sync(ICollection<DbProfile> profiles, CancellationToken cancellationToken)
		{
			try
			{
				using (var db = new DataConnection())
				{
					var result = await db.GetTable<DbProfile>().MergeOnPk(profiles, cancellationToken);

					if (logger.IsEnabled(LogLevel.Information))
					{
						var count = await db.GetTable<DbProfile>().CountAsync(cancellationToken);

						logger.LogInformation("Merged {MergedCount} profile(s) to database, total {TotalCount} profile(s)", result, count);	
					}
					
					return result;
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed to save profiles to database - {ErrorMessage}", ex.Message);

				return -1;
			}
		}

		public async Task<int> SyncAll(CancellationToken cancellationToken)
		{
			return await Sync(profileFileStorage.GetProfiles(), cancellationToken);
		}
	}
}