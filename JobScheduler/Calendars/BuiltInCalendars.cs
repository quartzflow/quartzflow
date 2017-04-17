using System;
using Quartz;
using Quartz.Impl.Calendar;

namespace JobScheduler.Calendars
{
    public class BuiltInCalendars
    {
        public ICalendar WeekDaysCalendar
        {
            get
            {
                var weekDayCalendar = new WeeklyCalendar();
                weekDayCalendar.SetDayExcluded(DayOfWeek.Monday, false);
                weekDayCalendar.SetDayExcluded(DayOfWeek.Tuesday, false);
                weekDayCalendar.SetDayExcluded(DayOfWeek.Wednesday, false);
                weekDayCalendar.SetDayExcluded(DayOfWeek.Tuesday, false);
                weekDayCalendar.SetDayExcluded(DayOfWeek.Friday, false);
                weekDayCalendar.SetDayExcluded(DayOfWeek.Saturday, true);
                weekDayCalendar.SetDayExcluded(DayOfWeek.Sunday, true);
                weekDayCalendar.Description = "Weekdays";
                return weekDayCalendar;
            } 
        }
    }
}
