using System.Collections.Immutable;
using Concept2Stats.Services;

namespace Concept2Stats.Tests.Services
{
	public class DateTimeHelperTests
	{
		[Test]
		public void GetDatesInAllTimeЯones_ForGivenUtcTime_ReturnsTwoOrThreeDates()
		{
			// arrange
			
			var twoDateTimesBefore = new[]
			{
				new DateTime(2025, 01, 30, 9, 55, 00, DateTimeKind.Utc),
				new DateTime(2025, 01, 30, 9, 59, 00, DateTimeKind.Utc)
			};
			
			var threeDateTimes = new[]
			{
				new DateTime(2025, 01, 30, 10, 0, 00, DateTimeKind.Utc),
				new DateTime(2025, 01, 30, 10, 30, 00, DateTimeKind.Utc),
				new DateTime(2025, 01, 30, 10, 59, 00, DateTimeKind.Utc)
			};
			
			var twoDateTimesAfter = new[]
			{
				new DateTime(2025, 01, 30, 11, 0, 00, DateTimeKind.Utc),
				new DateTime(2025, 01, 30, 11, 30, 00, DateTimeKind.Utc)
			};

			// act & assert

			foreach (var time in twoDateTimesBefore)
			{
				Assert.That(DateTimeHelper.GetDatesInAllTimeZones(time),
					Is.EquivalentTo([new DateOnly(2025, 01, 29), new DateOnly(2025, 01, 30)]));
			}
			
			foreach (var time in threeDateTimes)
			{
				Assert.That(DateTimeHelper.GetDatesInAllTimeZones(time),
					Is.EquivalentTo([new DateOnly(2025, 01, 29), new DateOnly(2025, 01, 30), new DateOnly(2025, 01, 31)]));
			}
			
			foreach (var time in twoDateTimesAfter)
			{
				Assert.That(DateTimeHelper.GetDatesInAllTimeZones(time),
					Is.EquivalentTo([new DateOnly(2025, 01, 30), new DateOnly(2025, 01, 31)]));
			}
		}

		[Test]
		public void GetDatesInAllTimeZones_Visualization()
		{
			const int minutesInDay = 24 * 60;

			var time = DateTime.UtcNow.Date;
			
			var intervals = 0;

			ICollection<DateOnly> lastResult = ImmutableList<DateOnly>.Empty;
			
			for (var minute = 0; minute <= minutesInDay; minute++)
			{
				var result = DateTimeHelper.GetDatesInAllTimeZones(time);

				if (result.SequenceEqual(lastResult) == false)
				{
					Console.WriteLine($"{time} - {string.Join(", ", result)}");
					
					lastResult = result;
					
					intervals++;
				}
				
				time = time.AddMinutes(1);
			}
			
			Assert.That(intervals, Is.EqualTo(3));
		}
	}
}