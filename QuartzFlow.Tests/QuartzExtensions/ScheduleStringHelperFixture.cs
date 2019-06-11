using System;
using NUnit.Framework;

namespace QuartzFlow.Tests
{
    [TestFixture]
    public class ScheduleStringHelperFixture
    {
        [Test]
        public void GetHoursAndMinutes_ForNowKeyword_ReturnsCurrentTimeInHoursAndMinutesComponents()
        {
            var result = ScheduleStringHelper.GetHoursAndMinutes("now");

            Assert.That(DateTime.Now.Hour == result.Item1);
            Assert.That(DateTime.Now.Minute == result.Item2);
        }

        [Test]
        public void GetHoursAndMinutes_ForNowPlusOffsetKeyword_ReturnsCurrentTimePlusOffsetInHoursAndMinutesComponents()
        {
            var result = ScheduleStringHelper.GetHoursAndMinutes("now+2");

            var nowPlusOffset = DateTime.Now.AddMinutes(2);
            Assert.That(nowPlusOffset.Hour == result.Item1);
            Assert.That(nowPlusOffset.Minute == result.Item2);
        }

        [Test]
        public void GetHoursAndMinutes_ForTime_ReturnsTimeInHoursAndMinutesComponents()
        {
            var result = ScheduleStringHelper.GetHoursAndMinutes("13:42");

            Assert.That(result.Item1 == 13);
            Assert.That(result.Item2 == 42);
        }

        [Test]
        public void GetDaysOfWeekToRunOn_ForOneDay_ReturnsCorrectResult()
        {
            var result = ScheduleStringHelper.GetDaysOfWeekToRunOn("mon");
            Assert.That(result.Length == 1);
            Assert.That(result[0] == DayOfWeek.Monday);
        }

        [Test]
        public void GetDaysOfWeekToRunOn_ForMultipleDays_ReturnsCorrectResult()
        {
            var result = ScheduleStringHelper.GetDaysOfWeekToRunOn("mon,wed,tue,thu,sat,sun,fri");
            Assert.That(result.Length == 7);
            Assert.That(result[0] == DayOfWeek.Monday);
            Assert.That(result[1] == DayOfWeek.Wednesday);
            Assert.That(result[2] == DayOfWeek.Tuesday);
            Assert.That(result[3] == DayOfWeek.Thursday);
            Assert.That(result[4] == DayOfWeek.Saturday);
            Assert.That(result[5] == DayOfWeek.Sunday);
            Assert.That(result[6] == DayOfWeek.Friday);
        }

        [Test]
        public void GetDaysOfWeekToRunOn_ForMultipleDaysWithDuplicates_ReturnsCorrectUniqueResult()
        {
            var result = ScheduleStringHelper.GetDaysOfWeekToRunOn("mon,wed,tue,thu,sat,sun,sat");
            Assert.That(result.Length == 6);
            Assert.That(result[0] == DayOfWeek.Monday);
            Assert.That(result[1] == DayOfWeek.Wednesday);
            Assert.That(result[2] == DayOfWeek.Tuesday);
            Assert.That(result[3] == DayOfWeek.Thursday);
            Assert.That(result[4] == DayOfWeek.Saturday);
            Assert.That(result[5] == DayOfWeek.Sunday);
        }
    }
}
