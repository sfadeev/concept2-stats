using LinqToDB;
using LinqToDB.Mapping;

namespace C2Stats.Entities
{
	[Table("wod")]
	public class DbWod
	{
		[Column("id"), PrimaryKey]
		public int Id { get; set; }
		
		[Column("date", DataType = DataType.Date), NotNull]
		public DateOnly Date { get; set; }
		
		[Column("type", DataType = DataType.VarChar, Length = 7), NotNull]
		public string? Type { get; set; }
		
		[Column("name", DataType = DataType.VarChar, Length = 256), Nullable]
		public string? Name { get; set; }
		
		[Column("description", DataType = DataType.VarChar, Length = 2048), Nullable]
		public string? Description { get; set; }
		
		[Column("total_count"), Nullable]
		public int? TotalCount { get; set; }
		
		[Column("last_modified", DataType = DataType.Timestamp), NotNull]
		public DateTime LastModified { get; set; }
	}
	
	[Table("wod_item")]
	public class DbWodItem
	{
		[Column("wod_id"), PrimaryKey(1)]
		public int WodId { get; set; }

		[Column("profile_id"), PrimaryKey(2)]
		public int ProfileId { get; set; }

		[Column("position")]
		public int Position { get; set; } 
		
		[Column("age")]
		public short? Age { get; set; }
		
		[Column("result_time", DataType = DataType.Interval)]
		public TimeSpan? ResultTime { get; set; }
		
		[Column("result_meters")]
		public int? ResultMeters { get; set; }

		[Column("pace", DataType = DataType.Interval)]
		public TimeSpan Pace { get; set; }
	}
}