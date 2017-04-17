using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobScheduler
{
    public static class SchedulerConfig
    {
        public static string JobsFile => Path.Combine(Directory.GetCurrentDirectory(), "jobs.json");

        public static string CalendarsFile => Path.Combine(Directory.GetCurrentDirectory(), "Calendars.json");

        public static string LogPath => @"d:\temp";
    }
}
