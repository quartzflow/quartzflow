using System;
using System.Collections.Generic;
using JobScheduler;
using JobScheduler.Calendars;

namespace JobSchedulerHost
{
    public class SchedulerHost
    {
        private readonly Conductor _conductor;

        public SchedulerHost()
        {
            var jobDefinitions = JobConfig.CreateJobDefinitions(SchedulerConfig.JobsFile);
            var calendarDefinitions = CustomCalendarConfig.CreateCalendarDefinitions(SchedulerConfig.CalendarsFile);
            var customCalendars = CustomCalendarFactory.CreateAnnualCalendarsWithSpecifiedDatesExcluded(calendarDefinitions);

            _conductor = new Conductor(jobDefinitions, customCalendars, 60000, 90000);
        }

        public void Start()
        {
            _conductor.JobsStillExecutingWarning += Conductor_JobsStillExecutingWarning;
            _conductor.JobsTerminated += Conductor_JobsTerminated;
            _conductor.StartScheduler();
        }

        public void Stop()
        {
            _conductor.StopScheduler();
            _conductor.JobsStillExecutingWarning -= Conductor_JobsStillExecutingWarning;
            _conductor.JobsTerminated -= Conductor_JobsTerminated;
        }

        void Conductor_JobsTerminated(object sender, List<string> e)
        {
            foreach (var message in e)
            {
                Console.WriteLine("ERROR - " + message);
            }
        }

        void Conductor_JobsStillExecutingWarning(object sender, List<string> e)
        {
            foreach (var message in e)
            {
                Console.WriteLine("WARNING - " + message);
            }
        }
    }
}
