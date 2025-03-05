using System.Reflection;
using System.Text.Json;
using C2Stats.Entities;
using LinqToDB;
using LinqToDB.Data;

namespace C2Stats.Services
{
	public interface ICountryProvider
	{
		IEnumerable<DbCountry> GetAllCountries();
		
		int? GetCountryId(string code);
	}

	public class DbCountryProvider : ICountryProvider
	{
		private readonly Lazy<Cache> _cache = new(BuildCache);
		
		public IEnumerable<DbCountry> GetAllCountries()
		{
			return _cache.Value.Countries;
		}
		
		public int? GetCountryId(string code)
		{
			if (_cache.Value.CountryCodeMap.TryGetValue(code, out var country))
			{
				return country.Id;
			}
			
			return null;
		}
		
		private static Cache BuildCache()
		{
			using (var db = new DataConnection())
			{
				var countries = db.GetTable<DbCountry>().ToList();
				
				return new Cache
				{
					Countries = countries,
					CountryCodeMap = countries.ToDictionary(x => x.Code, x => x)
				};
			}
		}
		
		private class Cache
		{
			public required IList<DbCountry> Countries { get; init; }
			
			public required IDictionary<string, DbCountry> CountryCodeMap { get; init; }
		}
	}
	
	public class DefaultCountryProvider : ICountryProvider
	{
		private readonly Lazy<Cache> _cache = new(BuildCache);

		public IEnumerable<DbCountry> GetAllCountries()
		{
			return _cache.Value.Countries;
		}
		
		public int? GetCountryId(string code)
		{
			if (_cache.Value.CountryCodeMap.TryGetValue(code, out var country))
			{
				return country.Id;
			}
			
			return null;
		}

		private static Cache BuildCache()
		{
			var json = ReadResourceFile(Assembly.GetExecutingAssembly(), "C2Stats.Resources.countries.json");

			var countries = JsonSerializer.Deserialize<DbCountry[]>(json)!;

			return new Cache
			{
				Countries = countries,
				CountryCodeMap = countries.ToDictionary(x => x.Code, x => x)
			};
		}

		private static string ReadResourceFile(Assembly assembly, string filename)
		{
			using (var stream = assembly.GetManifestResourceStream(filename)!)
			{
				using (var reader = new StreamReader(stream))
				{
					return reader.ReadToEnd();
				}
			}
		}

		private class Cache
		{
			public required IList<DbCountry> Countries { get; init; }
			
			public required IDictionary<string, DbCountry> CountryCodeMap { get; init; }
		}
	}
}