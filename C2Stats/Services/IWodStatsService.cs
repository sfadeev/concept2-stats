namespace C2Stats.Services
{
	public interface IWodStatsService
	{
		Task<CalendarData> GetYear(int year, string wodType, CancellationToken cancellationToken);
		
		Task<DayData> GetDay(DateOnly day, string wodType, CancellationToken cancellationToken);
	}

	public class CalendarData
	{
		public string? WodType { get; set; }
		
		public DateOnly From { get; set; }
		
		public DateOnly To { get; set; }
		
		public required CalendarDatum[] Data { get; set; }
	}
	
	public class CalendarDatum
	{
		public DateOnly Day { get; set; }
		
		public int Value { get; set; }
	}

	public class DayData
	{
		public string? WodType { get; set; }
		
		public DateOnly Day { get; set; }
		
		public required DayDatum[] Data { get; set; }
	}

	public class DayDatum
	{
		public TimeSpan Pace { get; set; }
		
		public int Count { get; set; }
	}
	
	public class DefaultWodStatsService : IWodStatsService
	{
		public Task<CalendarData> GetYear(int year, string wodType, CancellationToken cancellationToken)
		{
			var data = new List<CalendarDatum>();

			var random = new Random();

			var day = new DateOnly(year, 1, 1);

			while (day.Year == year)
			{
				data.Add(new CalendarDatum { Day = day, Value = random.Next(100) });

				day = day.AddDays(1);
			}
			
			var result = new CalendarData
			{
				WodType = wodType,
				From = new DateOnly(year, 1, 1),
				To = new DateOnly(year, 12, 31),
				Data = data.ToArray()
			};
			
			return Task.FromResult(result);
		}

		public Task<DayData> GetDay(DateOnly day, string wodType, CancellationToken cancellationToken)
		{
			var data = new List<DayDatum>();
			
			var random = new Random();

			var pace = new TimeSpan(0, 0, 1, 20);
			
			for (var i = 0; i < 100; i++)
			{
				data.Add(new DayDatum { Pace = pace, Count = random.Next(100) });
				
				pace = pace.Add(TimeSpan.FromSeconds(1));
			}
			
			var result = new DayData
			{
				WodType = wodType,
				Day = day,
				Data = data.ToArray()
			};
			
			return Task.FromResult(result);
		}
	}
}