using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NLog.Fluent;
using Quartz;
using Quartz.Listener;

namespace QuartzFlow.Listeners
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
        private readonly List<Tuple<JobKey, DependentJobDetails>> _chainLinks;

        public override string Name => "ConditionalJobChainingListener";

        public ConditionalJobChainingListener()
        {
            _chainLinks = new List<Tuple<JobKey, DependentJobDetails>>();
        }

        public IList<Tuple<JobKey, DependentJobDetails>> GetChainLinks()
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

            _chainLinks.Add(new Tuple<JobKey, DependentJobDetails>(firstJob, new DependentJobDetails(firstJobResult, secondJob)));
        }

        public override Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException, CancellationToken cancellationToken = new CancellationToken())
        {
            var jobDependencies = _chainLinks.FindAll(l => Equals(l.Item1, context.JobDetail.Key));

            if (jobDependencies == null || jobDependencies.Count == 0)
            {
                return Task.CompletedTask;
            }

            var predecessorJobStatus = (JobExecutionStatus)context.Result;

            foreach (var jobDependency in jobDependencies)
            {
                var jobDetails = jobDependency.Item2;

                switch (jobDetails.PredecessorJobResult)
                {
                    case JobResultCriteria.OnCompletion:
                        if (predecessorJobStatus != JobExecutionStatus.Retrying)
                        {
                            Log.Info($"Completion of Job '{context.JobDetail.Key}' will now trigger Job '{jobDetails.DependentJobKey}'");
                            TriggerDependentJob(context, jobDetails);
                        }
                        break;

                    case JobResultCriteria.OnSuccess:
                        if (predecessorJobStatus == JobExecutionStatus.Succeeded)
                        {
                            Log.Info($"Success of Job '{context.JobDetail.Key}' will now trigger Job '{jobDetails.DependentJobKey}'");
                            TriggerDependentJob(context, jobDetails);
                        }
                        break;

                    case JobResultCriteria.OnFailure:
                        if (predecessorJobStatus == JobExecutionStatus.Failed)
                        {
                            Log.Info($"Failure of Job '{context.JobDetail.Key}' will now trigger Job '{jobDetails.DependentJobKey}'");
                            TriggerDependentJob(context, jobDetails);
                        }
                        break;
                }
            }

            return Task.CompletedTask;

        }

        private void TriggerDependentJob(IJobExecutionContext context, DependentJobDetails sj)
        {
            try
            {
                context.Scheduler.TriggerJob(sj.DependentJobKey);
            }
            catch (SchedulerException se)
            {
                Log.Error().Message($"Error encountered triggering Job '{sj.DependentJobKey}'").Exception(se);
            }
        }

    }
}
