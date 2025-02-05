namespace Concept2Stats.Services
{
	public interface ITimeZoneDateProvider
	{
		IEnumerable<DateOnly> GetDatesInAllTimeZones(DateTime utcTime);
	}

	public class SystemTimeZoneDateProvider : ITimeZoneDateProvider
	{
		/// <summary>
		/// Returns unique dates (without time) in all timezones for given UTC time.
		/// Note: from UTC 10:00 (including) to 11:00 (not including) there are 3 dates, at other time 2 dates.
		///
		/// Note:
		/// 
		/// Alpine and Ubuntu Chiseled .NET images are focused on size. By default, these images do not include icu
		/// or tzdata, meaning that these images only work with apps that are configured for globalization-invariant mode.
		/// Apps that require globalization support can use the extra image variant of the dotnet/runtime-deps images.
		///
		/// https://github.com/dotnet/dotnet-docker/blob/main/documentation/image-variants.md
		/// </summary>
		public IEnumerable<DateOnly> GetDatesInAllTimeZones(DateTime utcTime)
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
	
	public class SimpleTimeZoneDateProvider : ITimeZoneDateProvider
	{
		public IEnumerable<DateOnly> GetDatesInAllTimeZones(DateTime utcTime)
		{
			if (utcTime.Kind != DateTimeKind.Utc) throw new InvalidOperationException("Provided time is not UTC");
			
			if (utcTime.Hour < 11) yield return DateOnly.FromDateTime(utcTime.AddDays(-1));
			
			yield return DateOnly.FromDateTime(utcTime);
			
			if (utcTime.Hour >= 10) yield return DateOnly.FromDateTime(utcTime.AddDays(1));
		}
	}
}