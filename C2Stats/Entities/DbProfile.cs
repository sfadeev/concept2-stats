using LinqToDB;
using LinqToDB.Mapping;

namespace C2Stats.Entities
{
	[Table("profile")]
	public class DbProfile
	{
		[Column("id"), PrimaryKey]
		public int Id { get; set; }
		
		[Column("name", DataType = DataType.VarChar, Length = 256), Nullable]
		public string? Name { get; set; }
		
		[Column("country", DataType = DataType.VarChar, Length = 5), NotNull]
		public string? Country { get; set; }
		
		[Column("sex", DataType = DataType.VarChar, Length = 1), NotNull]
		public string? Sex { get; set; }
		
		[Column("location", DataType = DataType.VarChar, Length = 256), Nullable]
		public string? Location { get; set; }
	}
}