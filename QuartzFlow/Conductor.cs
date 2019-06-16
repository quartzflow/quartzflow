using System;
using System.Collections.Generic;
using System.Timers;
using QuartzFlow.Listeners;
using QuartzFlow.QuartzExtensions;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Timer = System.Timers.Timer;

namespace QuartzFlow
{
    public class Conductor
    {
        private readonly IScheduler _scheduler;
        private readonly Timer _warningTimer;
        private readonly Timer _terminationTimer;
        private readonly IProcessManager _processManager;

        public event EventHandler<List<string>> JobsStillExecutingWarning;
        public event EventHandler<List<string>> JobsTerminated;

        public bool IsWarningTimerRunning => _warningTimer.Enabled;
   
        public bool IsTerminationTimerRunning => _terminationTimer.Enabled;

        public Conductor(List<JobDefinition> jobDefinitions, List<ICalendar> calendars, double intervalToCheckForLongRunningJobsInMs, double intervalToCheckForJobsToTerminateInMs) : 
            this(jobDefinitions, calendars, intervalToCheckForLongRunningJobsInMs, intervalToCheckForJobsToTerminateInMs, StdSchedulerFactory.GetDefaultScheduler().Result, new ProcessManager()) {}

        public Conductor(List<JobDefinition> jobDefinitions, List<ICalendar> calendars, double intervalToCheckForLongRunningJobsInMs, double intervalToCheckForJobsToTerminateInMs, 
                            IScheduler scheduler, IProcessManager processManager)
        {
            _scheduler = scheduler;

            _warningTimer = new Timer(intervalToCheckForLongRunningJobsInMs);
            _warningTimer.Elapsed += WarningTimerElapsed;
            _warningTimer.AutoReset = true;

            _terminationTimer = new Timer(intervalToCheckForJobsToTerminateInMs);
            _terminationTimer.Elapsed += TerminationTimerElapsed;
            _terminationTimer.AutoReset = true;

            _processManager = processManager;

            calendars.ForEach(c => _scheduler.AddCalendar(c.Description, c, true, true));

            AddJobsToScheduler(jobDefinitions);
        }

        public void StartScheduler()
        {
            if (!_scheduler.IsStarted)
                _scheduler.Start();

            _warningTimer.Start();
            _terminationTimer.Start();
        }

        public void StopScheduler()
        {
            _terminationTimer.Stop();
            _warningTimer.Stop();

            if (!_scheduler.IsShutdown)
                _scheduler.Shutdown();
        }

        void WarningTimerElapsed(object sender, ElapsedEventArgs e)
        {
            //If there are no subscribers then don't check
            if (JobsStillExecutingWarning == null)
                return;

            _warningTimer.Enabled = false;

            var currentTime = DateTime.UtcNow;
            var longRunningJobs = new List<string>();
            var currentRunningJobs = _scheduler.GetCurrentlyExecutingJobs().Result;
            foreach (var job in currentRunningJobs)
            {
                bool alreadyWarned = job.Trigger.JobDataMap.GetBooleanValue(Constants.FieldNames.HasIssuedLongRunningWarning);

                if (!alreadyWarned)
                {
                    int minutesToWarnAfter = job.MergedJobDataMap.GetIntValue(Constants.FieldNames.WarnAfter);

                    if (minutesToWarnAfter > 0)
                    {
                        var jobStartedAtUtc = job.FireTimeUtc;
                        double runTime = currentTime.Subtract(jobStartedAtUtc.DateTime).TotalMinutes;
                        if (runTime > minutesToWarnAfter)
                        {
                            longRunningJobs.Add(
                                $"Job {job.JobDetail.Key} was started at {jobStartedAtUtc.ToLocalTime():G} and is still running after {runTime:F} minutes");
                            job.Trigger.JobDataMap[Constants.FieldNames.HasIssuedLongRunningWarning] = true;
                        }
                    }
                }
            }

            _warningTimer.Enabled = true;

            if (longRunningJobs.Count > 0)
            {
                JobsStillExecutingWarning?.Invoke(this, longRunningJobs);
            }

        }

        void TerminationTimerElapsed(object sender, ElapsedEventArgs e)
        {
            //If there are no subscribers then don't check
            if (JobsTerminated == null)
                return;

            _terminationTimer.Enabled = false;

            var currentTime = DateTime.UtcNow;

            var terminatedJobs = new List<string>();
            var currentRunningJobs = _scheduler.GetCurrentlyExecutingJobs().Result;
            foreach (var job in currentRunningJobs)
            {
                int minutesToTerminateAfter = job.MergedJobDataMap.GetIntValue(Constants.FieldNames.TerminateAfter);

                if (minutesToTerminateAfter > 0)
                {
                    double runTime = currentTime.Subtract(job.FireTimeUtc.DateTime).TotalMinutes;
                    if (runTime > minutesToTerminateAfter)
                    {
                        _scheduler.KillJob(job, _processManager);
                        terminatedJobs.Add($"Job {job.JobDetail.Key} was started at {job.FireTimeUtc.ToLocalTime():G} and was killed after {runTime:F} minutes");
                    }
                }
            }

            _terminationTimer.Enabled = true;

            if (terminatedJobs.Count > 0)
            {
                JobsTerminated?.Invoke(this, terminatedJobs);
            }

        }

        private void AddJobsToScheduler(List<JobDefinition> jobDefinitions)
        {
            if (jobDefinitions == null || jobDefinitions.Count <= 0)
                return;

            var jobs = JobFactory.CreateFromJobDefinitions(jobDefinitions);
            jobs.ForEach(j => _scheduler.AddJobAndCreateTriggers(j));

            _scheduler.SetupJobDependencies(jobs);
            _scheduler.ListenerManager.AddJobListener(new ConsoleJobListener(), GroupMatcher<JobKey>.AnyGroup());
        }




    }
}
