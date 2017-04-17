using System;
using System.Linq;

namespace JobScheduler
{
    public class ScheduleStringHelper
    {
        public static Tuple<int, int> GetHoursAndMinutes(string runAtTime)
        {
            int hour = 0;
            int minutes = 0;

            if (runAtTime.ToLower().Contains("now"))
            {
                DateTime runTime = DateTime.Now;
                if (runAtTime.ToLower().Contains("+"))
                {
                    int minutesOffset = int.Parse(runAtTime.Split('+')[1]);
                    runTime = runTime.AddMinutes(minutesOffset);
                }

                hour = runTime.Hour;
                minutes = runTime.Minute;
            }
            else
            {
                hour = int.Parse(runAtTime.Split(':')[0]);
                minutes = int.Parse(runAtTime.Split(':')[1]);
            }

            return new Tuple<int, int>(hour, minutes);
        }

        public static DayOfWeek[] GetDaysOfWeekToRunOn(string runOnDays)
        {
            string[] days = runOnDays.Split(',');

            days = days.Distinct().ToArray();

            DayOfWeek[] daysOfWeek = new DayOfWeek[days.Length];
            int i = 0;
            foreach (var day in days)
            {
                switch (day.ToLower().Trim())
                {
                    case "mon":
                    case "mo":
                        daysOfWeek[i++] = DayOfWeek.Monday;
                        break;

                    case "tue":
                    case "tu":
                        daysOfWeek[i++] = DayOfWeek.Tuesday;
                        break;

                    case "wed":
                    case "we":
                        daysOfWeek[i++] = DayOfWeek.Wednesday;
                        break;

                    case "thu":
                    case "th":
                        daysOfWeek[i++] = DayOfWeek.Thursday;
                        break;

                    case "fri":
                    case "fr":
                        daysOfWeek[i++] = DayOfWeek.Friday;
                        break;

                    case "sat":
                    case "sa":
                        daysOfWeek[i++] = DayOfWeek.Saturday;
                        break;

                    case "sun":
                    case "su":
                        daysOfWeek[i++] = DayOfWeek.Sunday;
                        break;
                }
            }
            return daysOfWeek;
        }
    }
}
