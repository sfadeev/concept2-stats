using C2Stats.Entities;
using C2Stats.Models;
using LinqToDB;
using LinqToDB.Data;

namespace C2Stats.Services
{
	public interface IWodStatsService
	{
		Task<IEnumerable<Profile>> GetProfiles(string? search, CancellationToken cancellationToken);
		
		Task<IEnumerable<CountryDatum>> GetCountries(string type, int year, CancellationToken cancellationToken);
		
		Task<CalendarData> GetYear(string type, int year, string? country, CancellationToken cancellationToken);
		
		Task<DayData?> GetDay(string type, DateOnly day, string? country, CancellationToken cancellationToken);
		
		Task<IEnumerable<WodItem>> GetWodItems(string type, DateOnly day, string? country, CancellationToken cancellationToken);
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
		public Wod? Wod { get; set; }
		
		public DayDatum[]? Data { get; set; }
	}

	public class DayDatum
	{
		public TimeSpan Pace { get; set; }
		
		public int Male { get; set; }
		
		public int Female { get; set; }
	}
	
	public class WodItem
	{
		public int? No { get; set; }
		
		public int? ProfileId { get; set; }
		
		public int? Position { get; set; }

		public string? Name { get; set; }

		public string? Sex { get; set; }
		
		public int? Age { get; set; }

		public string? Location { get; set; }

		public string? Country { get; set; }
		
		public string? Affiliation { get; set; }

		public TimeSpan? ResultTime { get; set; }
		
		public string? ResultTimeFmt { get; set; }

		public int? ResultMeters { get; set; }

		public TimeSpan? Pace { get; set; }
		
		public string? PaceFmt { get; set; }
	}
	
	public class WodStatsService : IWodStatsService
	{
		public async Task<IEnumerable<Profile>> GetProfiles(string? search, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(search)) return [];
			
			using (var db = new DataConnection())
			{
				var result = from p in db.GetTable<DbProfile>()
					where p.Name != null && p.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
					orderby p.Name
					select new Profile { Id = p.Id, Name = p.Name };
				
				return await result.Take(100).ToListAsync(cancellationToken);
			}
		}

		public async Task<IEnumerable<CountryDatum>> GetCountries(string type, int year, CancellationToken cancellationToken)
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

		public async Task<DayData?> GetDay(string type, DateOnly day, string? country, CancellationToken cancellationToken)
		{
			using (var db = new DataConnection())
			{
				var wod = await (from w in db.GetTable<DbWod>()
					where w.Date == day && w.Type == type
					select new Wod
					{
						Id = w.Id,
						Date = w.Date,
						Type = w.Type,
						Name = w.Name,
						Description = w.Description,
						TotalCount = w.TotalCount
					}).SingleOrDefaultAsync(cancellationToken);

				if (wod != null)
				{
					var query = from wi in db.GetTable<DbWodItem>()
						join p in db.GetTable<DbProfile>() on wi.ProfileId equals p.Id
						where wi.WodId == wod.Id && wi.Pace.HasValue && (country == null || p.Country == country)
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

					return new DayData
					{
						Wod = wod,
						Data = grouped.Select(x => new DayDatum
						{
							Pace = TimeSpan.FromSeconds(x.Pace),
							Male = x.MaleCount,
							Female = x.FemaleCount
						}).OrderBy(x => x.Pace).ToArray()
					};
				}
				
				return new DayData();
			}
		}

		public async Task<IEnumerable<WodItem>> GetWodItems(string type, DateOnly day, string? country,
			CancellationToken cancellationToken)
		{
			using (var db = new DataConnection())
			{
				var wod = await (from w in db.GetTable<DbWod>()
					where w.Date == day && w.Type == type
					select new Wod
					{
						Id = w.Id,
						Date = w.Date,
						Type = w.Type,
						Name = w.Name,
						Description = w.Description,
						TotalCount = w.TotalCount
					}).SingleOrDefaultAsync(cancellationToken);

				if (wod != null)
				{
					var query = from wi in db.GetTable<DbWodItem>()
						join p in db.GetTable<DbProfile>() on wi.ProfileId equals p.Id
						where wi.WodId == wod.Id && (country == null || p.Country == country)
						orderby wi.No
						select new WodItem
						{
							ProfileId = wi.ProfileId,
							Position = wi.Position,
							Name = p.Name,
							Sex = p.Sex,
							Age = wi.Age,
							Location = p.Location,
							Country = p.Country,
							ResultTime = wi.ResultTime,
							ResultMeters = wi.ResultMeters,
							Pace = wi.Pace
						};

					var data = await query.ToListAsync(cancellationToken);

					Func<TimeSpan?, string?> formatTimeSpan = time =>
					{
						if (time.HasValue)
						{
							var t = time.Value;

							return $"{(int)t.TotalMinutes}:{t.Seconds:D2}.{t.Milliseconds / 100}";
						}
						
						return null;
					};
					
					for (var index = 0; index < data.Count; index++)
					{
						var item = data[index];
						
						item.No = index + 1;
						item.ResultTimeFmt = formatTimeSpan(item.ResultTime);
						item.PaceFmt = formatTimeSpan(item.Pace);
					}

					return data;
				}
				
				return [];
			}
		}
	}
}