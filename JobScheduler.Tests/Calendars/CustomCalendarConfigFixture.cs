using System;
using System.IO;
using QuartzFlow.Calendars;
using NUnit.Framework;

namespace QuartzFlow.Tests.Calendars
{
    [TestFixture()]
    public class CustomCalendarConfigFixture
    {
        [Test]
        public void CreateCalendarDefinitions_ForValidData_WillCreateCalendarDefintions()
        {
            var sr = TestHelper.GetFileContents("test-calendars.json");
            var result = CustomCalendarConfig.CreateCalendarDefinitions(sr);

            Assert.AreEqual(2, result.Count);

            Assert.AreEqual("AU_Without_Public_Holidays", result[0].CalendarName);
            Assert.AreEqual("Exclude", result[0].Action);
            Assert.AreEqual(new DateTime(2017, 1, 1), result[0].Dates[0]);
            Assert.AreEqual(new DateTime(2017, 1, 2), result[0].Dates[1]);
            Assert.AreEqual(new DateTime(2017, 1, 26), result[0].Dates[2]);
            Assert.AreEqual(new DateTime(2017, 4, 14), result[0].Dates[3]);
            Assert.AreEqual(new DateTime(2017, 4, 15), result[0].Dates[4]);
            Assert.AreEqual(new DateTime(2017, 4, 16), result[0].Dates[5]);
            Assert.AreEqual(new DateTime(2017, 4, 17), result[0].Dates[6]);
            Assert.AreEqual(new DateTime(2017, 4, 25), result[0].Dates[7]);
            Assert.AreEqual(new DateTime(2017, 6, 12), result[0].Dates[8]);
            Assert.AreEqual(new DateTime(2017, 12, 25), result[0].Dates[9]);
            Assert.AreEqual(new DateTime(2017, 12, 26), result[0].Dates[10]);

            Assert.AreEqual("NZ_Without_Public_Holidays", result[1].CalendarName);
            Assert.AreEqual("Exclude", result[1].Action);
            Assert.AreEqual(new DateTime(2017, 1, 1), result[1].Dates[0]);
            Assert.AreEqual(new DateTime(2017, 1, 2), result[1].Dates[1]);
            Assert.AreEqual(new DateTime(2017, 1, 26), result[1].Dates[2]);
            Assert.AreEqual(new DateTime(2017, 4, 14), result[1].Dates[3]);
            Assert.AreEqual(new DateTime(2017, 4, 15), result[1].Dates[4]);
            Assert.AreEqual(new DateTime(2017, 4, 16), result[1].Dates[5]);
            Assert.AreEqual(new DateTime(2017, 4, 17), result[1].Dates[6]);
            Assert.AreEqual(new DateTime(2017, 4, 25), result[1].Dates[7]);
            Assert.AreEqual(new DateTime(2017, 6, 12), result[1].Dates[8]);
            Assert.AreEqual(new DateTime(2017, 12, 25), result[1].Dates[9]);
            Assert.AreEqual(new DateTime(2017, 12, 26), result[1].Dates[10]);

        }

        public void CreateCalendarDefinitions_ForValidDataFromFile_WillCreateCalendarDefintions()
        {
            var result = CustomCalendarConfig.CreateCalendarDefinitions(Path.Combine(AppDomain.CurrentDomain.BaseDirectory + @"\TestData", "test-calendars.json"));

            Assert.AreEqual(2, result.Count);

            Assert.AreEqual("AU_Without_Public_Holidays", result[0].CalendarName);
            Assert.AreEqual("Exclude", result[0].Action);
            Assert.AreEqual(new DateTime(2017, 1, 1), result[0].Dates[0]);
            Assert.AreEqual(new DateTime(2017, 1, 2), result[0].Dates[1]);
            Assert.AreEqual(new DateTime(2017, 1, 26), result[0].Dates[2]);
            Assert.AreEqual(new DateTime(2017, 4, 14), result[0].Dates[3]);
            Assert.AreEqual(new DateTime(2017, 4, 15), result[0].Dates[4]);
            Assert.AreEqual(new DateTime(2017, 4, 16), result[0].Dates[5]);
            Assert.AreEqual(new DateTime(2017, 4, 17), result[0].Dates[6]);
            Assert.AreEqual(new DateTime(2017, 4, 25), result[0].Dates[7]);
            Assert.AreEqual(new DateTime(2017, 6, 12), result[0].Dates[8]);
            Assert.AreEqual(new DateTime(2017, 12, 25), result[0].Dates[9]);
            Assert.AreEqual(new DateTime(2017, 12, 26), result[0].Dates[10]);

            Assert.AreEqual("NZ_Without_Public_Holidays", result[1].CalendarName);
            Assert.AreEqual("Exclude", result[1].Action);
            Assert.AreEqual(new DateTime(2017, 1, 1), result[1].Dates[0]);
            Assert.AreEqual(new DateTime(2017, 1, 2), result[1].Dates[1]);
            Assert.AreEqual(new DateTime(2017, 1, 26), result[1].Dates[2]);
            Assert.AreEqual(new DateTime(2017, 4, 14), result[1].Dates[3]);
            Assert.AreEqual(new DateTime(2017, 4, 15), result[1].Dates[4]);
            Assert.AreEqual(new DateTime(2017, 4, 16), result[1].Dates[5]);
            Assert.AreEqual(new DateTime(2017, 4, 17), result[1].Dates[6]);
            Assert.AreEqual(new DateTime(2017, 4, 25), result[1].Dates[7]);
            Assert.AreEqual(new DateTime(2017, 6, 12), result[1].Dates[8]);
            Assert.AreEqual(new DateTime(2017, 12, 25), result[1].Dates[9]);
            Assert.AreEqual(new DateTime(2017, 12, 26), result[1].Dates[10]);

        }

        [Test]
        public void CreateCalendarDefinitions_ForMissingCalendarName_WillThrowException()
        {
            var sr = TestHelper.GetFileContents("test-calendars-missing-name.json");
            var ex = Assert.Throws<Exception>(() => CustomCalendarConfig.CreateCalendarDefinitions(sr));
            Assert.AreEqual("Failed to create calendars from config - one or more of the calendars has a blank or missing CalendarName value", ex.Message);
        }

        [Test]
        public void CreateCalendarDefinitions_ForMissingAction_WillThrowException()
        {
            var sr = TestHelper.GetFileContents("test-calendars-missing-action.json");
            var ex = Assert.Throws<Exception>(() => CustomCalendarConfig.CreateCalendarDefinitions(sr));
            Assert.AreEqual("Failed to create calendars from config - one or more of the calendars has a blank or missing Action value", ex.Message);
        }
    }
}