using System;
using System.Linq;
using QuartzFlow.QuartzExtensions;
using NUnit.Framework;
using Quartz.Impl;
using Quartz.Impl.Triggers;

namespace QuartzFlow.Tests.QuartzExtensions
{
    [TestFixture]
    public class JobExtensionsFixture
    {
        [Test]
        public void RequiresTrigger_ForJobWithNullRunAtValue_ReturnsFalse()
        {
            var job = TestHelper.CreateTestJob("Test", null, "mon, tue", null, null);
            Assert.IsFalse(job.RequiresTrigger());
        }

        [Test]
        public void RequiresTrigger_ForJobWithRunAtValue_ReturnsTrue()
        {
            var job = TestHelper.CreateTestJob("Test", "01:07", "mon, tue", null, null);
            Assert.IsTrue(job.RequiresTrigger());
        }

        [Test]
        public void CreateTriggers_ForSingleRunTimeWithNoExclusionCalendarOrTimeZone_ReturnCorrectTrigger()
        {
            var job = TestHelper.CreateTestJob("Test", "01:07", "mon, tue", null, null);

            var triggers = job.CreateTriggers();

            Assert.AreEqual(1, triggers.Count);

            var triggerArray = triggers.ToArray();
            var cronTrigger1 = (CronTriggerImpl)triggerArray[0];

            Assert.That(cronTrigger1.Description == "Run at: 01:07, run days: mon, tue, exclusion calendar: ");
            Assert.That(cronTrigger1.CronExpressionString == "0 7 1 ? * 2,3");
            Assert.That(cronTrigger1.Name == "Trigger_0_for_DEFAULT.Test");
            Assert.That(cronTrigger1.TimeZone == TimeZoneInfo.Local);
            Assert.That(cronTrigger1.CalendarName == null);
        }

        [Test]
        public void CreateTriggers_ForSingleRunTimeWithExclusionCalendarAndNoTimeZone_ReturnCorrectTrigger()
        {
            var job = TestHelper.CreateTestJob("Test", "01:07", "mon, tue", "public_holidays", null);

            var triggers = job.CreateTriggers();

            Assert.AreEqual(1, triggers.Count);

            var triggerArray = triggers.ToArray();
            var cronTrigger1 = (CronTriggerImpl)triggerArray[0];

            Assert.That(cronTrigger1.Description == "Run at: 01:07, run days: mon, tue, exclusion calendar: public_holidays");
            Assert.That(cronTrigger1.CronExpressionString == "0 7 1 ? * 2,3");
            Assert.That(cronTrigger1.Name == "Trigger_0_for_DEFAULT.Test");
            Assert.That(cronTrigger1.TimeZone == TimeZoneInfo.Local);
            Assert.That(cronTrigger1.CalendarName == "public_holidays");
        }

        [Test]
        public void CreateTriggers_ForSingleRunTimeWithExclusionCalendarAndTimeZone_ReturnCorrectTrigger()
        {
            var job = TestHelper.CreateTestJob("Test", "01:07", "mon, tue", "public_holidays", "New Zealand Standard Time");

            var triggers = job.CreateTriggers();

            Assert.AreEqual(1, triggers.Count);

            var triggerArray = triggers.ToArray();
            var cronTrigger1 = (CronTriggerImpl)triggerArray[0];

            Assert.That(cronTrigger1.Description == "Run at: 01:07, run days: mon, tue, exclusion calendar: public_holidays");
            Assert.That(cronTrigger1.CronExpressionString == "0 7 1 ? * 2,3");
            Assert.That(cronTrigger1.Name == "Trigger_0_for_DEFAULT.Test");
            Assert.That(cronTrigger1.TimeZone.Id == "New Zealand Standard Time");
            Assert.That(cronTrigger1.CalendarName == "public_holidays");
        }

        [Test]
        public void CreateTriggers_ForMultipleRunTimesWithExclusionCalendarAndTimeZone_ReturnCorrectTriggers()
        {
            var job = TestHelper.CreateTestJob("Test", "01:07,21:23", "mon, tue", "public_holidays", "New Zealand Standard Time");

            var triggers = job.CreateTriggers();

            Assert.AreEqual(2, triggers.Count);

            var triggerArray = triggers.ToArray();
            var cronTrigger1 = (CronTriggerImpl)triggerArray[0];
            var cronTrigger2 = (CronTriggerImpl)triggerArray[1];

            Assert.That(cronTrigger1.Description == "Run at: 01:07, run days: mon, tue, exclusion calendar: public_holidays");
            Assert.That(cronTrigger1.CronExpressionString == "0 7 1 ? * 2,3");
            Assert.That(cronTrigger1.Name == "Trigger_0_for_DEFAULT.Test");
            Assert.That(cronTrigger1.TimeZone.Id == "New Zealand Standard Time");
            Assert.That(cronTrigger1.CalendarName == "public_holidays");

            Assert.That(cronTrigger2.Description == "Run at: 21:23, run days: mon, tue, exclusion calendar: public_holidays");
            Assert.That(cronTrigger2.CronExpressionString == "0 23 21 ? * 2,3");
            Assert.That(cronTrigger2.Name == "Trigger_1_for_DEFAULT.Test");
            Assert.That(cronTrigger2.TimeZone.Id == "New Zealand Standard Time");
            Assert.That(cronTrigger2.CalendarName == "public_holidays");
        }

        [Test]
        public void GetNextRunAtMessages_ForGivenTriggerOnCurrentDayBeforeCurrentTime_ReturnsCorrectMessage()
        {
            //Test is constructed to assume current time is 12pm (midday)

            var job = TestHelper.CreateTestJob("Test", "01:07", "mon", null, "New Zealand Standard Time");
            var triggers = job.CreateTriggers();

            var scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.ScheduleJob(job, triggers, true);

            var runMessage = job.GetNextRunAtMessages(triggers);

            var seedDate = DateTime.Now.Date;
            if (DateTime.Now.DayOfWeek == DayOfWeek.Monday)
                seedDate = seedDate.AddDays(1);

            var dateOfNextMonday = GetDateForNextDayOfWeek(seedDate, DayOfWeek.Monday);

            var nzTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time");
            var timeSpanDifference = nzTimeZoneInfo.BaseUtcOffset - TimeZoneInfo.Local.BaseUtcOffset;
            var expectedLocalTime = dateOfNextMonday.AddHours(1).AddMinutes(7).Subtract(timeSpanDifference);

            bool isInDaylightSavingTime = nzTimeZoneInfo.IsDaylightSavingTime(dateOfNextMonday);
            var nzTimeZoneName = isInDaylightSavingTime ? nzTimeZoneInfo.DaylightName : nzTimeZoneInfo.StandardName;

            bool isLocalInDaylightSavingTime = TimeZoneInfo.Local.IsDaylightSavingTime(dateOfNextMonday);
            var localTimeZoneName = isLocalInDaylightSavingTime ? TimeZoneInfo.Local.DaylightName : TimeZoneInfo.Local.StandardName;
            var expectedMessage =
                $"{dateOfNextMonday:D} 1:07:00 AM {nzTimeZoneName} ({expectedLocalTime:F} {localTimeZoneName})\r\n";
            Assert.AreEqual(expectedMessage, runMessage);
        }

        [Test]
        public void GetNextRunAtMessages_ForGivenTriggerOnCurrentDayAfterCurrentTime_ReturnsCorrectMessage()
        {
            //Test is constructed to assume current time is 12pm (midday)

            var job = TestHelper.CreateTestJob("Test", "21:23", "mon", null, "New Zealand Standard Time");
            var triggers = job.CreateTriggers();

            var scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.ScheduleJob(job, triggers, true);

            var runMessage = job.GetNextRunAtMessages(triggers);

            var seedDate = DateTime.Now.Date;

            if (DateTime.Now.DayOfWeek == DayOfWeek.Monday && (DateTime.UtcNow.TimeOfDay.CompareTo(new TimeSpan(9, 23, 00)) > 0))
                seedDate = seedDate.AddDays(1);

            var dateOfNextMonday = GetDateForNextDayOfWeek(seedDate, DayOfWeek.Monday);

            var nzTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time");
            var timeSpanDifference = nzTimeZoneInfo.BaseUtcOffset - TimeZoneInfo.Local.BaseUtcOffset;
            var expectedLocalTime = dateOfNextMonday.AddHours(21).AddMinutes(23).Subtract(timeSpanDifference);

            bool isInDaylightSavingTime = nzTimeZoneInfo.IsDaylightSavingTime(dateOfNextMonday);
            var nzTimeZoneName = isInDaylightSavingTime ? nzTimeZoneInfo.DaylightName : nzTimeZoneInfo.StandardName;

            bool isLocalInDaylightSavingTime = TimeZoneInfo.Local.IsDaylightSavingTime(dateOfNextMonday);
            var localTimeZoneName = isLocalInDaylightSavingTime ? TimeZoneInfo.Local.DaylightName : TimeZoneInfo.Local.StandardName;
            var expectedMessage =
                $"{dateOfNextMonday:D} 9:23:00 PM {nzTimeZoneName} ({expectedLocalTime:F} {localTimeZoneName})\r\n";
            Assert.AreEqual(expectedMessage, runMessage);
        }

        private static DateTime GetDateForNextDayOfWeek(DateTime start, DayOfWeek day)
        {
            // The (... + 7) % 7 ensures we end up with a value in the range [0, 6]
            int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
            return start.AddDays(daysToAdd);
        }
    }
}
