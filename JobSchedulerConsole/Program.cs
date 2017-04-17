using System;
using System.IO;
using System.Threading;
using JobScheduler;

namespace JobSchedulerConsole
{
    public class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var jobsReader = new StringReader(File.ReadAllText(SchedulerConfig.JobsFile));
                var jobDefinitions = JobConfig.CreateJobDefinitions(jobsReader);

                var conductor = new Conductor(jobDefinitions, 60000, 90000);

                conductor.JobsStillExecutingWarning += Conductor_JobsStillExecutingWarning;
                conductor.JobsTerminated += Conductor_JobsTerminated;

                var sr = new StringReader(File.ReadAllText(SchedulerConfig.CalendarsFile));
                conductor.AddCalendarsToExcludeFromFullYear(sr);

                conductor.StartScheduler();

                // sleep to show what's happening
                Thread.Sleep(TimeSpan.FromSeconds(600));

                conductor.StopScheduler();
            }
            catch (Exception se)
            {
                Console.WriteLine(se);
            }

            Console.WriteLine("Press any key to close the application");
            Console.ReadKey();
        }

        static void Conductor_JobsTerminated(object sender, System.Collections.Generic.List<string> e)
        {
            foreach (var message in e)
            {
                Console.WriteLine("ERROR - " + message);
            }
        }

        static void Conductor_JobsStillExecutingWarning(object sender, System.Collections.Generic.List<string> e)
        {
            foreach (var message in e)
            {
                Console.WriteLine("WARNING - " + message);    
            }
        }



       
    }


}
