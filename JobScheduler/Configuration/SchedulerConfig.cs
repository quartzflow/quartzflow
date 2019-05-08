using System.IO;
using System.Configuration;

namespace JobScheduler
{
    public static class SchedulerConfig
    {
        public static string JobsFile => ConfigurationManager.AppSettings["JobsFile"];

        public static string CalendarsFile => ConfigurationManager.AppSettings["CalendarsFile"];

        public static string LogPath => ConfigurationManager.AppSettings["LogPath"];

        public static string ApiPortToUse => ConfigurationManager.AppSettings["ApiPortToUse"];
    }
}
