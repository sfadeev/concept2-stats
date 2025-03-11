using C2Stats.Services;

namespace C2Stats.Models
{
	public class AppOptions : IConfigOptions
	{
		public static string SectionName => "Settings";
		
		public string? BaseUrl { get; init; }
		
		public string ParseDirPath { get; set; } = "./data/";
		
		public int DownloadArchiveDaysFrom { get; set; } = 2;
		
		// Number of yesterdays to re-download - in case some devices were offline and submitted logs later
		public int DownloadYesterdayDays { get; set; } = 7;
	}
}