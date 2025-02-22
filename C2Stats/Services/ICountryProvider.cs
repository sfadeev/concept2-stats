using System.Reflection;
using System.Text.Json;
using C2Stats.Entities;

namespace C2Stats.Services
{
	public interface ICountryProvider
	{
		IEnumerable<DbCountry> GetAllCountries();
		
		int? GetCountryId(string code);
	}

	public class DefaultCountryProvider : ICountryProvider
	{
		private readonly Lazy<CountryCache> _cache = new(BuildCountryCache);

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

		private static CountryCache BuildCountryCache()
		{
			var json = ReadResourceFile(Assembly.GetExecutingAssembly(), "C2Stats.Resources.countries.json");

			var countries = JsonSerializer.Deserialize<DbCountry[]>(json)!;

			return new CountryCache
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

		private class CountryCache
		{
			public required DbCountry[] Countries { get; init; }
			
			public required IDictionary<string, DbCountry> CountryCodeMap { get; init; }
		}
	}
}