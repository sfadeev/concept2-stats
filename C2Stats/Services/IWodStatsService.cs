using C2Stats.Entities;
using LinqToDB;
using LinqToDB.Data;

namespace C2Stats.Services
{
	public interface IWodStatsService
	{
		Task<IList<CountryDatum>> GetCountries(string type, int year, CancellationToken cancellationToken);
		
		Task<CalendarData> GetYear(string type, int year, string? country, CancellationToken cancellationToken);
		
		Task<DayData> GetDay(string type, DateOnly day, string? country, CancellationToken cancellationToken);
	}
	
	public class CountryDatum
	{
		public string? Code { get; set; }
		
		public string? Name { get; set; }
		
		public int? Value { get; set; }
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
		public async Task<IList<CountryDatum>> GetCountries(string type, int year, CancellationToken cancellationToken)
		{
			using (var db = new DataConnection())
			{
				var beginDate = new DateOnly(year, 1, 1);
				var endDate = new DateOnly(year, 12, 31);

				var query = from g in
						from w in db.GetTable<DbWod>()
						join wi in db.GetTable<DbWodItem>() on w.Id equals wi.WodId
						join p in db.GetTable<DbProfile>() on wi.ProfileId equals p.Id
						where w.Date >= beginDate && w.Date <= endDate && w.Type == type
						group w by p.Country
						into g
						select new { Code = g.Key, Count = g.Count() }
					join c in db.GetTable<DbCountry>() on g.Code equals c.Code
					orderby g.Count descending
					select new CountryDatum { Code = c.Code, Name = c.Name, Value = g.Count };
				
				var data = await query.ToArrayAsync(cancellationToken);
				
				return data;
			}
		}

		public async Task<CalendarData> GetYear(string type, int year, string? country, CancellationToken cancellationToken)
		{
			using (var db = new DataConnection())
			{
				var beginDate = new DateOnly(year, 1, 1);
				var endDate = new DateOnly(year, 12, 31);

				var query = from g in
						from w in db.GetTable<DbWod>()
						join wi in db.GetTable<DbWodItem>() on w.Id equals wi.WodId
						join p in db.GetTable<DbProfile>() on wi.ProfileId equals p.Id
						where w.Date >= beginDate && w.Date <= endDate && w.Type == type
						      && (country == null || p.Country == country)
						group w by w.Id
						into g
						select new { Id = g.Key, Count = g.Count() }
					join w in db.GetTable<DbWod>() on g.Id equals w.Id
					select new CalendarDatum { Day = w.Date, Value = g.Count };
				
				var data = await query.ToArrayAsync(cancellationToken);
				
				var result = new CalendarData
				{
					Type = type,
					From = data.Length > 0 ? data.Min(x => x.Day) : null,
					To = data.Length > 0 ? data.Max(x => x.Day) : null,
					Data = data
				};
				
				return result;
			}
		}

		public async Task<DayData> GetDay(string type, DateOnly day, string? country, CancellationToken cancellationToken)
		{
			using (var db = new DataConnection())
			{
				var query = from w in db.GetTable<DbWod>()
					join wi in db.GetTable<DbWodItem>() on w.Id equals wi.WodId
					join p in db.GetTable<DbProfile>() on wi.ProfileId equals p.Id
					where w.Date == day && w.Type == type &&
					      wi.Pace.HasValue && (country == null || p.Country == country)
					select new { Pace = wi.Pace!.Value, p.Sex };
				
				var data = await query.ToListAsync(cancellationToken);
				
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
				
				var result = new DayData
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
				
				return result;
			}
		}
	}
}