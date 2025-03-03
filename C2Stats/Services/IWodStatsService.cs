using C2Stats.Entities;
using LinqToDB;
using LinqToDB.Data;

namespace C2Stats.Services
{
	public interface IWodStatsService
	{
		Task<CalendarData> GetYear(string type, int year, string? country, CancellationToken cancellationToken);
		
		Task<DayData> GetDay(string type, DateOnly day, string? country, CancellationToken cancellationToken);
	}

	public class CalendarData
	{
		public string? Type { get; set; }
		
		public DateOnly? From { get; set; }
		
		public DateOnly? To { get; set; }
		
		public required CalendarDatum[] Data { get; set; }
	}
	
	public class CalendarDatum
	{
		public DateOnly Day { get; set; }
		
		public int? Value { get; set; }
	}

	public class DayData
	{
		public string? Type { get; set; }
		
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
		public async Task<CalendarData> GetYear(string type, int year, string? country, CancellationToken cancellationToken)
		{
			using (var db = new DataConnection())
			{
				var data = await (
						from g in
							from w in db.GetTable<DbWod>().Where(x => x.Date.Year == year && x.Type == type)
							join wi in db.GetTable<DbWodItem>()
								on w.Id equals wi.WodId
							join p in db.GetTable<DbProfile>().Where(x => country == null || x.Country == country)
								on wi.ProfileId equals p.Id
							group w by w.Id
							into g
							select new { Id = g.Key, Count = g.Count() }
						join w in db.GetTable<DbWod>() on g.Id equals w.Id
						select new CalendarDatum { Day = w.Date, Value = g.Count })
					.ToArrayAsync(cancellationToken);
				
				return new CalendarData
				{
					Type = type,
					From = data.Length > 0 ? data.Min(x => x.Day) : null,
					To = data.Length > 0 ? data.Max(x => x.Day) : null,
					Data = data
				};
			}
		}

		public async Task<DayData> GetDay(string type, DateOnly day, string? country, CancellationToken cancellationToken)
		{
			using (var db = new DataConnection())
			{
				var data = await
				(
					from w in db.GetTable<DbWod>()
						.Where(x => x.Date == day && x.Type == type)
					join wi in db.GetTable<DbWodItem>().Where(x => x.Pace.HasValue)
						on w.Id equals wi.WodId
					join p in db.GetTable<DbProfile>().Where(x => country == null || x.Country == country)
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
					Type = type,
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