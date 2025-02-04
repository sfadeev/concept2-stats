namespace Concept2Stats.Services
{
	public static class DateTimeHelper
	{
		/// <summary>
		/// Returns unique dates (without time) in all timezones for given UTC time.
		/// Note: from UTC 10:00 to 11:00 there are 3 dates, at other time 2 dates.
		/// </summary>
		public static ICollection<DateOnly> GetDatesInAllTimeZones(DateTime utcTime)
		{
			var result = new List<DateOnly>();
			
			var tzs = TimeZoneInfo.GetSystemTimeZones();

			foreach (var tz in tzs)
			{
				var tzTime = TimeZoneInfo.ConvertTime(utcTime, tz);
				
				var tzDate = DateOnly.FromDateTime(tzTime);
				
				if (result.Contains(tzDate) == false)
				{
					result.Add(tzDate);
				}
			}
			
			return result;
		}
	}
}