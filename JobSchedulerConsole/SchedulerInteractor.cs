using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JobScheduler;
using JobScheduler.QuartzExtensions;
using JobSchedulerHost.HttpApi;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;

namespace JobSchedulerHost
{
    public interface ISchedulerInteractor
    {
        //void Start();
        //void Stop();
        string GetStatus();
        List<string> GetJobNames();

        JobDetailsModel GetJobDetails(string jobName);
    }

    public class SchedulerInteractor : ISchedulerInteractor
    {
        private readonly IScheduler _scheduler;

        public SchedulerInteractor()
        {
            _scheduler = StdSchedulerFactory.GetDefaultScheduler();
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

        public JobDetailsModel GetJobDetails(string jobName)
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

            var job =_scheduler.GetJobDetail(key);

            var jobTriggers = _scheduler.GetTriggersOfJob(key);

            var result = new JobDetailsModel()
            {
                Name = job.Key.ToString(),
                Description = job.Description,
                NextRunAt = job.GetNextRunAtMessages(new Quartz.Collection.HashSet<ITrigger>(jobTriggers))
            };

            return result;
        }
    }
}
