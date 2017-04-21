using System;
using System.Collections.Generic;
using System.Linq;
using JobScheduler;
using JobScheduler.QuartzExtensions;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;

namespace JobSchedulerHost.HttpApi
{
    public interface ISchedulerInteractor
    {
        //void Start();
        //void Stop();
        string GetStatus();
        List<string> GetJobNames();
        List<string> GetCurrentlyExecutingJobs();
        JobDetailsModel GetJobDetails(string jobName);
        void PauseJob(string jobName);
        void PauseAllJobs();
        void ResumeJob(string jobName);
        void ResumeAllJobs();
        void StartJob(string jobName);
        bool KillJob(string jobName);
        bool JobExists(string jobName);
    }

    public class SchedulerInteractor : ISchedulerInteractor
    {
        private readonly IScheduler _scheduler;
        private readonly IProcessManager _processManager;

        public SchedulerInteractor()
        {
            _scheduler = StdSchedulerFactory.GetDefaultScheduler();
        }

        public SchedulerInteractor(IScheduler scheduler, IProcessManager processManager)
        {
            _scheduler = scheduler;
            _processManager = processManager;
        }

        public string GetStatus()
        {
            if (_scheduler == null)
                return "Not set";

            if (_scheduler.IsStarted && !_scheduler.IsShutdown && !_scheduler.InStandbyMode)
                return "Started";

            if (_scheduler.InStandbyMode)
                return "InStandBy";

            if (_scheduler.IsShutdown)
                return "Shutdown";

            return "Undetermined";
        }

        public List<string> GetJobNames()
        {
            var jobNames = new List<string>();
            var allJobs = _scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
            if (allJobs != null && allJobs.Count > 0)
            {
                jobNames.AddRange(allJobs.Select(j => j.ToString()));
            }
            return jobNames;
        }

        public List<string> GetCurrentlyExecutingJobs()
        {
            var jobDetails = new List<string>();
            var executingJobs = _scheduler.GetCurrentlyExecutingJobs();
            if (executingJobs != null && executingJobs.Count > 0)
            {
                foreach (var job in executingJobs)
                {
                    TimeSpan jobRunTime = DateTime.UtcNow.Subtract(job.FireTimeUtc.Value.DateTime);
                    jobDetails.Add($"Job {job.JobDetail.Key} was started at {job.FireTimeUtc.Value.LocalDateTime} and has been running for {jobRunTime.TotalMinutes:0.00} minutes");
                }
                
            }

            return jobDetails;
        }

        public JobDetailsModel GetJobDetails(string jobName)
        {
            var key = GetJobKey(jobName);

            var job =_scheduler.GetJobDetail(key);

            if (job == null)
                return null;

            var props = new SortedList<string, string>();
            foreach (var keyValuePair in job.JobDataMap)
            {
                props.Add(keyValuePair.Key, keyValuePair.Value?.ToString() ?? string.Empty);
            }

            var jobTriggers = _scheduler.GetTriggersOfJob(key);

            var result = new JobDetailsModel()
            {
                Name = job.Key.ToString(),
                Description = job.Description,
                NextRunAt = job.GetNextRunAtMessages(new Quartz.Collection.HashSet<ITrigger>(jobTriggers)),
                Properties = props
            };

            return result;
        }

        public bool JobExists(string jobName)
        {
            return _scheduler.CheckExists(GetJobKey(jobName));
        }

        public void PauseJob(string jobName)
        {
            _scheduler.PauseJob(GetJobKey(jobName));
        }

        public void PauseAllJobs()
        {
            _scheduler.PauseJobs(GroupMatcher<JobKey>.AnyGroup());
        }

        public void ResumeJob(string jobName)
        {
            _scheduler.ResumeJob(GetJobKey(jobName));
        }

        public void ResumeAllJobs()
        {
            _scheduler.ResumeJobs(GroupMatcher<JobKey>.AnyGroup());
        }

        public bool KillJob(string jobName)
        {
            var currentlyExecutingJobs = _scheduler.GetCurrentlyExecutingJobs();
            var targetJob = currentlyExecutingJobs.FirstOrDefault(j => j.JobDetail.Key.ToString() == jobName);

            if (targetJob != null)
            {
                _scheduler.KillJob(targetJob, _processManager);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void StartJob(string jobName)
        {
            _scheduler.TriggerJob(GetJobKey(jobName));
        }

        private static JobKey GetJobKey(string jobName)
        {
            JobKey key;

            if (jobName.Contains('.'))
            {
                var group = jobName.Split('.')[0];
                var name = jobName.Split('.')[1];
                key = JobKey.Create(name, group);
            }
            else
            {
                key = JobKey.Create(jobName);
            }
            return key;
        }
    }
}
