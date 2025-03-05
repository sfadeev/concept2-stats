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

				using (var db = new DataConnection())
				{
					var result = await db.GetTable<DbCountry>().MergeOnPrimaryKey(countries, cancellationToken);

					if (logger.IsEnabled(LogLevel.Information))
					{
						var count = await db.GetTable<DbCountry>().CountAsync(cancellationToken);

						logger.LogInformation("Merged {MergedCount} country(-ies) to database, total {TotalCount} country(-ies)", result, count);	
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