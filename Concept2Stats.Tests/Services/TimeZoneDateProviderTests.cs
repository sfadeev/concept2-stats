using System.Collections.Immutable;
using System.Text;
using Concept2Stats.Services;

namespace Concept2Stats.Tests.Services
{
	public class TimeZoneDateProviderTests
	{
		[Test]
		public void GetDatesInAllTimeZones_ForGivenUtcTime_ReturnsTwoOrThreeDates()
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

			Asserts(time => new SystemTimeZoneDateProvider().GetDatesInAllTimeZones(time));
			
			Asserts(time => new SimpleTimeZoneDateProvider().GetDatesInAllTimeZones(time));
			
			return;

			void Asserts(Func<DateTime, IEnumerable<DateOnly>> testedFunc)
			{
				foreach (var time in twoDateTimesBefore)
				{
					var result = testedFunc(time);
					
					Assert.That(result, Is.EquivalentTo([new DateOnly(2025, 01, 29), new DateOnly(2025, 01, 30)]));
				}
			
				foreach (var time in threeDateTimes)
				{
					var result = testedFunc(time);
					
					Assert.That(result, Is.EquivalentTo([new DateOnly(2025, 01, 29), new DateOnly(2025, 01, 30), new DateOnly(2025, 01, 31)]));
				}
			
				foreach (var time in twoDateTimesAfter)
				{
					var result = testedFunc(time); 
					
					Assert.That(result, Is.EquivalentTo([new DateOnly(2025, 01, 30), new DateOnly(2025, 01, 31)]));
				}
			}
		}

		[Test]
		public void GetDatesInAllTimeZones_Visualization()
		{
			var print1 = Asserts(time => new SystemTimeZoneDateProvider().GetDatesInAllTimeZones(time));
			
			var print2 = Asserts(time => new SimpleTimeZoneDateProvider().GetDatesInAllTimeZones(time));
			
			Assert.That(print2, Is.EqualTo(print1));
			
			Console.WriteLine(print1);
			
			return;
			
			string Asserts(Func<DateTime, IEnumerable<DateOnly>> testedFunc)
			{
				var print = new StringBuilder();
				
				const int minutesInDay = 24 * 60;

				var time = DateTime.UtcNow.Date;
			
				var intervals = 0;

				ICollection<DateOnly> lastResult = ImmutableList<DateOnly>.Empty;
			
				for (var minute = 0; minute <= minutesInDay; minute++)
				{
					var result = testedFunc(time).ToList();

					if (result.SequenceEqual(lastResult) == false)
					{
						print.AppendLine($"{time} - {string.Join(", ", result)}");
					
						lastResult = result;
					
						intervals++;
					}
				
					time = time.AddMinutes(1);
				}
				
				Assert.That(intervals, Is.EqualTo(3));
				
				return print.ToString();
			}
		}
	}
}