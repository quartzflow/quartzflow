using System;

namespace QuartzFlow.Calendars
{
    public class CustomCalendarDefinition
    {
        public string CalendarName { get; set; }
        public string Action { get; set; }
        public DateTime[] Dates { get; set; }
    }
}
