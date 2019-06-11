using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Quartz.Util;

namespace QuartzFlow.Calendars
{
    public class CustomCalendarConfig
    {
        public static List<CustomCalendarDefinition> CreateCalendarDefinitions(string filename)
        {
            var reader = new StringReader(File.ReadAllText(filename));
            return CreateCalendarDefinitions(reader);
        }

        public static List<CustomCalendarDefinition> CreateCalendarDefinitions(StringReader configReader)
        {
            string jsonConfig = configReader.ReadToEnd();   
            var calendars = JsonConvert.DeserializeObject<List<CustomCalendarDefinition>>(jsonConfig);

            ValidateConfig(calendars);

            return calendars;
        }

        private static void ValidateConfig(List<CustomCalendarDefinition> calendars)
        {
            if (calendars.Any(j => j.CalendarName.IsNullOrWhiteSpace()))
                throw new Exception(
                    "Failed to create calendars from config - one or more of the calendars has a blank or missing CalendarName value");

            if (calendars.Any(j => j.Action.IsNullOrWhiteSpace()))
                throw new Exception(
                    "Failed to create calendars from config - one or more of the calendars has a blank or missing Action value");
        }
    }
}
