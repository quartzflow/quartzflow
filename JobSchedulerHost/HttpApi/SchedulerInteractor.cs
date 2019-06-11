using System;
using System.Collections.Generic;
using System.Linq;
using QuartzFlow;
using QuartzFlow.QuartzExtensions;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;

namespace QuartzFlowHost.HttpApi
{
    public interface ISchedulerInteractor
    {
        //void Start();
        //void Stop();
        string GetStatus();
        List<ActiveJobDetailsModel> GetCurrentlyExecutingJobs();
        JobDetailsModel GetJobDetails(string jobName);
        List<JobDetailsModel> GetJobs();
        void PauseJob(string jobName);
        void PauseAllJobs();
        void ResumeJob(string jobName);
        void ResumeAllJobs();
        void StartJob(string jobName);
        bool KillJob(string jobName);
        bool JobExists(string jobName);
        string GetJobNameById(int jobId);
    }

    public class SchedulerInteractor : ISchedulerInteractor
    {
        private readonly IScheduler _scheduler;
        private readonly IProcessManager _processManager;

        public SchedulerInteractor()
        {
            _scheduler = StdSchedulerFactory.GetDefaultScheduler();
            _processManager = new ProcessManager();
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

        public List<JobDetailsModel> GetJobs()
        {
            var jobList = new List<JobDetailsModel>();
            var allJobs = _scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
            if (allJobs != null && allJobs.Count > 0)
            {
                foreach (var job in allJobs)
                {
                    jobList.Add(GetJobDetails($"{job.Group}.{job.Name}"));
                }
            }
            return jobList;
        }

        public List<ActiveJobDetailsModel> GetCurrentlyExecutingJobs()
        {
            var activeJobList = new List<ActiveJobDetailsModel>();
            var executingJobs = _scheduler.GetCurrentlyExecutingJobs();
            if (executingJobs != null && executingJobs.Count > 0)
            {
                foreach (var jobExecutionContext in executingJobs)
                {
                    TimeSpan jobRunTime = DateTime.UtcNow.Subtract(jobExecutionContext.FireTimeUtc.Value.DateTime);
                    //jobDetails.Add($"Job {jobExecutionContext.JobDetail.Key} was started at {jobExecutionContext.FireTimeUtc.Value.LocalDateTime} and has been running for {jobRunTime.TotalMinutes:0.00} minutes");
      
                    var activeJobDetails = new ActiveJobDetailsModel()
                    {
                        Id = jobExecutionContext.JobDetail.JobDataMap.GetInt(Constants.FieldNames.JobId),
                        Name = jobExecutionContext.JobDetail.Key.ToString(),
                        Description = jobExecutionContext.JobDetail.Description,
                        StartedAt = jobExecutionContext.FireTimeUtc.Value.LocalDateTime,
                        MinutesExecutingFor = jobRunTime.TotalMinutes,
                        RetryCount = jobExecutionContext.RefireCount
                    };

                    activeJobList.Add(activeJobDetails);
                }
                
            }

            return activeJobList;
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

            IList<ITrigger> jobTriggers = _scheduler.GetTriggersOfJob(job.Key);

            var result = new JobDetailsModel()
            {
                Id = job.JobDataMap.GetInt(Constants.FieldNames.JobId),
                Name = job.Key.ToString(),
                Description = job.Description,
                NextRunAt = job.GetNextRunAtMessages(new Quartz.Collection.HashSet<ITrigger>(jobTriggers)),
                Status = _scheduler.GetJobStatus(job),
                Properties = props
            };

            return result;
        }

        public string GetJobNameById(int jobId)
        {
            var keys = _scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());

            foreach(var key in keys)
            {
                var job = _scheduler.GetJobDetail(key);
                if (job.JobDataMap.GetInt(Constants.FieldNames.JobId) == jobId)
                    return key.ToString();
            }

            return null;
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
                targetJob.AppendToOutputBuffer($"About to terminate Job {jobName} manually via API at {DateTime.Now.ToLongTimeString()}");
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
