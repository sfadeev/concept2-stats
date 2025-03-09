namespace C2Stats.Models
{
	public class Wod
	{
		public int Id { get; set; }
		
		public DateOnly Date { get; set; }
		
		public string? Type { get; set; }
		
		public string? Name { get; set; }
		
		public string? Description { get; set; }
		
		public int? TotalCount { get; set; }
	}
}