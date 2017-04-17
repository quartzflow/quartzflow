using System;
using System.Collections.Generic;
using Quartz;
using Quartz.Listener;

namespace JobScheduler.Listeners
{
    public enum JobResultCriteria
    {
        OnSuccess,
        OnFailure,
        OnCompletion
    }

    public class DependentJobDetails
    {
        public JobResultCriteria PredecessorJobResult { get; private set; }
        public JobKey DependentJobKey { get; private set; }

        public DependentJobDetails(JobResultCriteria predecessorJobResult, JobKey dependentJobKey) 
        {
            PredecessorJobResult = predecessorJobResult;
            DependentJobKey = dependentJobKey;
        }
    }

    public class ConditionalJobChainingListener : JobListenerSupport
    {
        private readonly IDictionary<JobKey, DependentJobDetails> _chainLinks;

        public override string Name => "ConditionalJobChainingListener";

        public ConditionalJobChainingListener()
        {
            _chainLinks = new Dictionary<JobKey, DependentJobDetails>();
        }

        public IDictionary<JobKey, DependentJobDetails> GetChainLinks()
        {
            return _chainLinks;
        }

        /// <summary>
        /// Add a chain mapping - when the Job identified by the first key completes
        /// the job identified by the second key will be triggered.
        /// </summary>
        /// <param name="firstJob">a JobKey with the name and group of the first job</param>
        /// <param name="firstJobResult">what result of the first job triggers the second job</param>
        /// <param name="secondJob">a JobKey with the name and group of the follow-up job</param>
        public void AddJobChainLink(JobKey firstJob, JobResultCriteria firstJobResult, JobKey secondJob)
        {
            if (firstJob == null || secondJob == null)
            {
                throw new ArgumentException("Key cannot be null!");
            }
            if (String.IsNullOrEmpty(firstJob.Name) || String.IsNullOrEmpty(secondJob.Name))
            {
                throw new ArgumentException("Key cannot have a null name!");
            }

            _chainLinks.Add(firstJob, new DependentJobDetails(firstJobResult, secondJob));
        }

        public override void JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException)
        {
            DependentJobDetails sj;
            _chainLinks.TryGetValue(context.JobDetail.Key, out sj);

            if (sj == null)
            {
                return;
            }

            var predecessorJobStatus = (JobExecutionStatus)context.Result;

            switch (sj.PredecessorJobResult)
            {
                case JobResultCriteria.OnCompletion:
                    if (predecessorJobStatus != JobExecutionStatus.Retrying)
                    {
                        Log.Info($"Completion of Job '{context.JobDetail.Key}' will now trigger Job '{sj.DependentJobKey}'");
                        TriggerDependentJob(context, sj);
                    }
                    break;

                case JobResultCriteria.OnSuccess:
                    if (predecessorJobStatus == JobExecutionStatus.Succeeded)
                    {
                        Log.Info($"Success of Job '{context.JobDetail.Key}' will now trigger Job '{sj.DependentJobKey}'");
                        TriggerDependentJob(context, sj);
                    }
                    break;

                case JobResultCriteria.OnFailure:
                    if (predecessorJobStatus == JobExecutionStatus.Failed)
                    {
                        Log.Info($"Failure of Job '{context.JobDetail.Key}' will now trigger Job '{sj.DependentJobKey}'");
                        TriggerDependentJob(context, sj);
                    }
                    break;
            }
        }

        private void TriggerDependentJob(IJobExecutionContext context, DependentJobDetails sj)
        {
            try
            {
                context.Scheduler.TriggerJob(sj.DependentJobKey);
            }
            catch (SchedulerException se)
            {
                Log.Error($"Error encountered triggering Job '{sj.DependentJobKey}'", se);
            }
        }

    }
}
