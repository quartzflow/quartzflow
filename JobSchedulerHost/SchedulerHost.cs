using System;
using System.Collections.Generic;
using System.IO;
using Common.Logging;
using QuartzFlow;
using QuartzFlow.Calendars;
using QuartzFlowHost.HttpApi;
using Microsoft.Owin.Hosting;
using LogManager = Common.Logging.LogManager;

namespace QuartzFlowHost
{
    public class SchedulerHost
    {
        private readonly Conductor _conductor;
        private readonly ILog _logger;   

        public SchedulerHost()
        {
            try
            {
                _logger = LogManager.GetLogger<SchedulerHost>();
                var jobDefinitions = JobConfig.CreateJobDefinitions(SchedulerConfig.JobsFile);
                var calendarDefinitions = CustomCalendarConfig.CreateCalendarDefinitions(SchedulerConfig.CalendarsFile);
                var customCalendars = CustomCalendarFactory.CreateAnnualCalendarsWithSpecifiedDatesExcluded(calendarDefinitions);

                _conductor = new Conductor(jobDefinitions, customCalendars, 60000, 90000);
            }
            catch (Exception e)
            {
                _logger.Error($"In constructor: {e.Message}\n\r{e.StackTrace}");
                throw;
            }
        }

        public void Start()
        {
            try
            {
                _conductor.JobsStillExecutingWarning += Conductor_JobsStillExecutingWarning;
                _conductor.JobsTerminated += Conductor_JobsTerminated;
                _conductor.StartScheduler();

                WebApp.Start<NancyStartup>("http://+:" + SchedulerConfig.ApiPortToUse);
            }
            catch (Exception e)
            {
                _logger.Error($"Start: {e.Message}\n\r{e.StackTrace}");
                throw;
            }

        }

        public void Stop()
        {
            try
            {
                _conductor.StopScheduler();
                _conductor.JobsStillExecutingWarning -= Conductor_JobsStillExecutingWarning;
                _conductor.JobsTerminated -= Conductor_JobsTerminated;
            }
            catch (Exception e)
            {
                _logger.Error($"Stop: {e.Message}\n\r{e.StackTrace}");
                throw;
            }

        }

        void Conductor_JobsTerminated(object sender, List<string> e)
        {
            foreach (var message in e)
            {
                Console.WriteLine("ERROR - " + message);
                _logger.Error(message);
            }
        }

        void Conductor_JobsStillExecutingWarning(object sender, List<string> e)
        {
            foreach (var message in e)
            {
                Console.WriteLine("WARNING - " + message);
                _logger.Warn(message);
            }
        }
    }
}
