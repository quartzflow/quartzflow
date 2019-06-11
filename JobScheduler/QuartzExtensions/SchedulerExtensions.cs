using System;
using System.Collections.Generic;
using System.Linq;
using Common.Logging;
using QuartzFlow.Listeners;
using Quartz;
using Quartz.Impl.Matchers;

namespace QuartzFlow.QuartzExtensions 
{
    public static class SchedulerExtensions
    {
        public static void AddJobAndCreateTriggers(this IScheduler scheduler, IJobDetail job)
        {
            if (job == null)
                return;

            if (job.RequiresTrigger())
            {
                var triggers = job.CreateTriggers();
                scheduler.ScheduleJob(job, triggers, true);
                LogManager.GetLogger<IScheduler>().Info(job.GetNextRunAtMessages(triggers));
            }
            else
            {
                scheduler.AddJob(job, true, true);
            }
        }

        public static void SetupJobDependencies(this IScheduler scheduler, List<IJobDetail> jobs)
        {
            var myJobListener = CreateJobChainingJobListener(jobs);
            scheduler.ListenerManager.AddJobListener(myJobListener, GroupMatcher<JobKey>.AnyGroup());
        }

        public static void KillJob(this IScheduler scheduler, IJobExecutionContext job, IProcessManager processManager)
        {
            int processId = (int)job.Get(Constants.FieldNames.ProcessId);
            processManager.KillProcess(processId);
        }

        private static IJobListener CreateJobChainingJobListener(List<IJobDetail> jobs)
        {
            var myJobListener = new ConditionalJobChainingListener();

            foreach (var job in jobs)
            {
                var predecessorJobKey = job.JobDataMap[Constants.FieldNames.RunOnSuccessOf];
                if (predecessorJobKey != null)
                {
                    var predecessorJob = jobs.FirstOrDefault(j => j.Key.ToString() == predecessorJobKey.ToString());
                    if (predecessorJob == null)
                        throw new Exception($"Unable to find predecessor job '{predecessorJobKey}' for job '{job.Key}'");
                    myJobListener.AddJobChainLink(predecessorJob.Key, JobResultCriteria.OnSuccess, job.Key);
                }

                predecessorJobKey = job.JobDataMap[Constants.FieldNames.RunOnFailureOf];
                if (predecessorJobKey != null)
                {
                    var predecessorJob = jobs.FirstOrDefault(j => j.Key.ToString() == predecessorJobKey.ToString());
                    if (predecessorJob == null)
                        throw new Exception($"Unable to find predecessor job '{predecessorJobKey}' for job '{job.Key}'");
                    myJobListener.AddJobChainLink(predecessorJob.Key, JobResultCriteria.OnFailure, job.Key);
                }

                predecessorJobKey = job.JobDataMap[Constants.FieldNames.RunOnCompletionOf];
                if (predecessorJobKey != null)
                {
                    var predecessorJob = jobs.FirstOrDefault(j => j.Key.ToString() == predecessorJobKey.ToString());
                    if (predecessorJob == null)
                        throw new Exception($"Unable to find predecessor job '{predecessorJobKey}' for job '{job.Key}'");
                    myJobListener.AddJobChainLink(predecessorJob.Key, JobResultCriteria.OnCompletion, job.Key);
                }
            }

            return myJobListener;
        }

        public static string GetJobStatus(this IScheduler scheduler, IJobDetail job)
        {
            IList<ITrigger> jobTriggers = scheduler.GetTriggersOfJob(job.Key);
            var triggerStates = new List<TriggerState>();

            foreach (ITrigger trigger in jobTriggers)
            {
                TriggerState state = scheduler.GetTriggerState(trigger.Key);
                triggerStates.Add(state);
            }

            var distinctTriggerState = triggerStates.Distinct().Count();
            var status = "Indeterminate";
            switch (distinctTriggerState)
            {
                case 0:
                    status = "Unscheduled";
                    break;
                case 1:
                    status = triggerStates.First().ToString();
                    break;
                default:
                    break;

            }

            return status;
        }
    }
}
