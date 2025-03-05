using C2Stats.Entities;

namespace C2Stats.Services
{
	public static class UnaffiliatedCountry
	{
		public static IEnumerable<DbCountry> GetList()
		{
			return new List<DbCountry>
			{
				// countries with more results should be first to minimize downloads
				new() { Id = 178, Code = "RUS" },
				new() { Id = 20, Code = "BLR" }
			};
		}
	}
}