namespace Concept2Stats.Models
{
	public class WodResult
	{
		public bool? Success { get; set; }

		public DateOnly? Date { get; set; }
		
		public string? Type { get; set; }

		public string? Name { get; set; }

		public string? Description { get; set; }
		
		public int? TotalCount { get; set; }

		public IList<WodResultItem> Items { get; set; } = new List<WodResultItem>();
	}

	public static class WodType
	{
		public const string RowErg = "rowerg";
		
		public const string SkiErg = "skierg";
		
		public const string BikeErg = "bikeerg";
	}

	public class WodResultItem
	{
		public int? Id { get; set; }
		
		public int? Position { get; set; }

		public string? Name { get; set; }

		public int? Age { get; set; }

		public string? Location { get; set; }

		public string? Country { get; set; }
		
		public string? CountryCode { get; set; }

		public string? Affiliation { get; set; }

		public TimeSpan? ResultTime { get; set; }

		public int? ResultMeters { get; set; }

		public TimeSpan? Pace { get; set; }
	}
}