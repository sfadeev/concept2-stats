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
		
		public int Male { get; set; }
		
		public int Female { get; set; }
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

		public async Task<DayData> GetDay(DateOnly day, string wodType, CancellationToken cancellationToken)
		{
			using (var db = new DataConnection())
			{
				var data = await
				(
					from w in db.GetTable<DbWod>()
						.Where(x => x.Date == day && x.Type == wodType)
					join wi in db.GetTable<DbWodItem>().Where(x => x.Pace.HasValue)
						on w.Id equals wi.WodId
					join p in db.GetTable<DbProfile>()
						on wi.ProfileId equals p.Id
					select new { Pace = wi.Pace.Value, p.Sex }
				).ToListAsync(cancellationToken);

				var grouped =
					from x in data
					group x by Math.Floor(x.Pace.TotalSeconds)
					into g
					select new
					{
						Pace = g.Key, 
						MaleCount = g.Count(x => x.Sex == "M"),
						FemaleCount = g.Count(x => x.Sex == "F")
					};

				return new DayData
				{
					WodType = wodType,
					Day = day,
					Data = grouped.Select(x => new DayDatum
					{
						Pace = TimeSpan.FromSeconds(x.Pace),
						Male = x.MaleCount,
						Female = x.FemaleCount
					}).OrderBy(x => x.Pace).ToArray()
				};
			}
		}
	}
}