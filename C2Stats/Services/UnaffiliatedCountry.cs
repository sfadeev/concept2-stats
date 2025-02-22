using C2Stats.Entities;
using C2Stats.Models;

namespace C2Stats.Services
{
	public static class UnaffiliatedCountry
	{
		public static string Placeholder => "UNAFF";
		
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