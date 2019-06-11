using System;
using System.Collections.Generic;
using System.Linq;
using QuartzFlow.Calendars;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Quartz.Impl.Calendar;

namespace QuartzFlow.Tests.Calendars
{
    [TestFixture()]
    public class CustomCalendarFactoryFixture
    {
        [Test]
        public void CreateAnnualCalendarsWithSpecifiedDatesExcluded_WillCreateCorrectly()
        {
            var calendar1 = new CustomCalendarDefinition
            {
                CalendarName = "Holiday",
                Action = "exclude",
                Dates = new[] {new DateTime(2017, 1, 5), new DateTime(2017, 5, 4)}
            };

            var calendar2 = new CustomCalendarDefinition
            {
                CalendarName = "Release days",
                Action = "exclude",
                Dates = new[] { new DateTime(2017, 3, 7), new DateTime(2017, 9, 14) }
            };

            var calendars =
                CustomCalendarFactory.CreateAnnualCalendarsWithSpecifiedDatesExcluded(
                    new List<CustomCalendarDefinition>() {calendar1, calendar2});

            Assert.AreEqual(2, calendars.Count);

            Assert.AreEqual("Holiday", calendars[0].Description);
            Assert.IsInstanceOf<AnnualCalendar>(calendars[0]);
            var annualCalendar1 = (AnnualCalendar) calendars[0];
            Assert.AreEqual(2, annualCalendar1.DaysExcluded.Count);
            Assert.IsTrue(annualCalendar1.IsDayExcluded(new DateTimeOffset(calendar1.Dates[0])));
            Assert.IsTrue(annualCalendar1.IsDayExcluded(new DateTimeOffset(calendar1.Dates[1])));

            Assert.AreEqual("Release days", calendars[1].Description);
            Assert.IsInstanceOf<AnnualCalendar>(calendars[1]);
            var annualCalendar2 = (AnnualCalendar)calendars[1];
            Assert.AreEqual(2, annualCalendar2.DaysExcluded.Count);
            Assert.IsTrue(annualCalendar2.IsDayExcluded(new DateTimeOffset(calendar2.Dates[0])));
            Assert.IsTrue(annualCalendar2.IsDayExcluded(new DateTimeOffset(calendar2.Dates[1])));

        }
    }
}