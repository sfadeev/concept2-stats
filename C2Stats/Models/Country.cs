namespace C2Stats.Models
{
	public class Country
	{
		public int Id { get; init; }
		
		public required string Code { get; init; }
		
		public string? Name { get; init; }
	}
}