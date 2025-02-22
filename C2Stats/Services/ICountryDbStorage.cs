using C2Stats.Entities;
using LinqToDB;
using LinqToDB.Data;

namespace C2Stats.Services
{
	public interface ICountryDbStorage
	{
		Task<int> SyncAll(CancellationToken cancellationToken);
	}
	
	public class CountryDbStorage(
		ILogger<CountryDbStorage> logger, ICountryDownloader countryDownloader) : ICountryDbStorage
	{
		public async Task<int> SyncAll(CancellationToken cancellationToken)
		{
			try
			{
				var countries = await countryDownloader.Download(cancellationToken);

				countries.Add(new DbCountry
				{
					Id = -1, 
					Code = UnaffiliatedCountry.Placeholder, 
					Name = "UNAFFILIATED"
				});

				using (var db = new DataConnection())
				{
					var table = db.GetTable<DbCountry>();
					
					var result = await table
						.Merge()
						.Using(countries)
						.OnTargetKey()
						.UpdateWhenMatched()
						.InsertWhenNotMatched()
						.MergeAsync(cancellationToken);

					if (logger.IsEnabled(LogLevel.Information))
					{
						var count = await table.CountAsync(cancellationToken);

						logger.LogInformation("Merged {MergedCount} countries to db, total {TotalCount} countries", result, count);	
					}
					
					return result;
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed to merge countries to db - {ErrorMessage}", ex.Message);

				throw;
			}
		}
	}
}