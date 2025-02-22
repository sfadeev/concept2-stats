using LinqToDB;
using LinqToDB.Mapping;

namespace C2Stats.Entities
{
	[Table("country")]
	public class DbCountry
	{
		[Column("code", DataType = DataType.VarChar, Length = 5), PrimaryKey]
		public required string Code { get; init; }
		
		[Column("id"), NotNull]
		public int Id { get; init; }
		
		[Column("name", DataType = DataType.VarChar, Length = 64), Nullable]
		public string? Name { get; init; }
	}
}