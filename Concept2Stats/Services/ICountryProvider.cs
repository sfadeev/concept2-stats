using System.Reflection;
using System.Text.Json;
using Concept2Stats.Models;

namespace Concept2Stats.Services
{
	public interface ICountryProvider
	{
		string UnaffiliatedCountryPlaceholder { get; }
		
		IEnumerable<Country> GetAllCountries();

		IEnumerable<Country> GetUnaffiliatedCountries();
		
		int? GetCountryId(string code);
	}

	public class DefaultCountryProvider : ICountryProvider
	{
		private readonly Lazy<CountryCache> _cache = new(BuildCountryCache);

		public string UnaffiliatedCountryPlaceholder => "UNAFF";

		public IEnumerable<Country> GetAllCountries()
		{
			return _cache.Value.Countries;
		}

		public IEnumerable<Country> GetUnaffiliatedCountries()
		{
			return new List<Country>
			{
				// countries with more results should be first to minimize downloads
				new() { Id = 178, Code = "RUS" },
				new() { Id = 20, Code = "BLR" }
			};
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
			var json = ReadResourceFile(Assembly.GetExecutingAssembly(), "Concept2Stats.Resources.countries.json");

			var countries = JsonSerializer.Deserialize<Country[]>(json)!;

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
			public required Country[] Countries { get; init; }
			
			public required IDictionary<string, Country> CountryCodeMap { get; init; }
		}
	}
}