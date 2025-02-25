using LinqToDB;
using LinqToDB.Configuration;
using LinqToDB.Data;

namespace C2Stats.Tests
{
	[SetUpFixture]
	public class TestSetup
	{
		[OneTimeSetUp]
		public void GlobalSetup()
		{
			// Code to run before any tests execute
			
			DataConnection.DefaultSettings = new LinqToDBSettings(
				"Default",
				ProviderName.PostgreSQL,
				Environment.GetEnvironmentVariable("C2_STATS_CONNECTION_STRING")!
			);
		}

		[OneTimeTearDown]
		public void GlobalTeardown()
		{
			// Code to clean up after all tests run
		}
	}
}