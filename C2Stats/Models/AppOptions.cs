using C2Stats.Services;

namespace C2Stats.Models
{
	public class AppOptions : IConfigOptions
	{
		public static string SectionName => "Settings";
		
		public string? BaseUrl { get; init; }
		
		public string ParseDirPath { get; set; } = "./data/";
	}
}