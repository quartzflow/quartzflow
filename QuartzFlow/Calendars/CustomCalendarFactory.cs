using System.Collections.Generic;
using Quartz;
using Quartz.Impl.Calendar;

namespace QuartzFlow.Calendars
{
    public class CustomCalendarFactory
    {
        public static List<ICalendar> CreateAnnualCalendarsWithSpecifiedDatesExcluded(List<CustomCalendarDefinition> customCalendarDefinitions)
        {
            var actualCalendars = new List<ICalendar>();

            foreach (var customCalendar in customCalendarDefinitions)
            {
                var calendar = new AnnualCalendar
                {
                    Description = customCalendar.CalendarName
                };

                bool shouldExclude = customCalendar.Action.ToLower().Trim() == "exclude";
                foreach (var date in customCalendar.Dates)
                {
                    calendar.SetDayExcluded(date, shouldExclude);    
                }

                actualCalendars.Add(calendar);

            }
            return actualCalendars;
        }
    }
}
