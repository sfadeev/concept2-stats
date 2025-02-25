using C2Stats.Entities;
using LinqToDB;
using LinqToDB.Data;

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
		
		public int? Value { get; set; }
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
	
	public class WodStatsService : IWodStatsService
	{
		public async Task<CalendarData> GetYear(int year, string wodType, CancellationToken cancellationToken)
		{
			using (var db = new DataConnection())
			{
				var data = await db.GetTable<DbWod>()
					.Where(x => /*x.Date.Year == year &&*/ x.Type == wodType && x.TotalCount != null)
					.Select(x => new CalendarDatum { Day = x.Date, Value = x.TotalCount })
					.ToArrayAsync(cancellationToken);
				
				return new CalendarData
				{
					WodType = wodType,
					From = data.Min(x => x.Day),
					To = data.Max(x => x.Day),
					Data = data
				};
			}
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