using System.IO;
using System.Configuration;

namespace JobScheduler
{
    public static class SchedulerConfig
    {
        public static string JobsFile => Path.Combine(Directory.GetCurrentDirectory(), "jobs.json");

        public static string CalendarsFile => Path.Combine(Directory.GetCurrentDirectory(), "Calendars.json");

        public static string LogPath => ConfigurationManager.AppSettings["LogPath"];
    }
}
