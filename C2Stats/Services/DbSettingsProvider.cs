using LinqToDB;
using LinqToDB.Configuration;

namespace C2Stats.Services
{
	public class DbSettingsProvider(IConfiguration configuration) : ILinqToDBSettings
	{
		public IEnumerable<IDataProviderSettings> DataProviders => [];

		public string DefaultConfiguration => "Default";

		public string DefaultDataProvider => ProviderName.PostgreSQL;

		public IEnumerable<IConnectionStringSettings> ConnectionStrings
		{
			get
			{
				var connectionStrings = configuration.GetSection("ConnectionStrings").GetChildren();
				
				foreach (var connectionString in connectionStrings)
				{
					yield return new ConnectionStringSettings(connectionString.Key, connectionString.Value!, DefaultDataProvider);
				}
			}
		}
	}
}