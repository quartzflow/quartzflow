using System;
using System.Collections.Generic;
using System.IO;
using JobScheduler.Jobs;
using Quartz;

namespace JobScheduler
{
    public class JobFactory
    {
        public static List<IJobDetail> CreateFromJobDefinitions(List<JobDefinition> jobDefinitions)
        {
            var actualJobs = new List<IJobDetail>();
            int jobId = 1;
            foreach (var jobDefinition in jobDefinitions)
            {
                IJobDetail job = JobBuilder.Create<ConsoleJob>()
                                            .WithIdentity(jobDefinition.JobName, jobDefinition.Group)
                                            .WithDescription(jobDefinition.Description)
                                            .StoreDurably(true)
                                            .Build();

                job.JobDataMap[Constants.FieldNames.JobId] = jobId++;
                job.JobDataMap[Constants.FieldNames.ExecutableName] = jobDefinition.ExecutableName;
                job.JobDataMap[Constants.FieldNames.Parameters] = jobDefinition.Parameters;
                job.JobDataMap[Constants.FieldNames.OutputFile] = Path.Combine(SchedulerConfig.LogPath, $"{jobDefinition.Group}-{jobDefinition.JobName}.txt");
                job.JobDataMap[Constants.FieldNames.RunOnCompletionOf] = jobDefinition.RunOnCompletionOf?.QuartzId;
                job.JobDataMap[Constants.FieldNames.RunOnSuccessOf] = jobDefinition.RunOnSuccessOf?.QuartzId;
                job.JobDataMap[Constants.FieldNames.RunOnFailureOf] = jobDefinition.RunOnFailureOf?.QuartzId;
                job.JobDataMap[Constants.FieldNames.MaxRetries] = jobDefinition.Retries;
                job.JobDataMap[Constants.FieldNames.WarnAfter] = jobDefinition.WarnAfter;
                job.JobDataMap[Constants.FieldNames.TerminateAfter] = jobDefinition.TerminateAfter;

                if (jobDefinition.RunSchedule != null)
                {
                    job.JobDataMap[Constants.FieldNames.RunAt] = jobDefinition.RunSchedule.RunAt;
                    job.JobDataMap[Constants.FieldNames.ExclusionCalendar] = jobDefinition.RunSchedule.ExclusionCalendar;
                    job.JobDataMap[Constants.FieldNames.RunCalendar] = jobDefinition.RunSchedule.RunOn.Calendar;
                    job.JobDataMap[Constants.FieldNames.RunDays] = jobDefinition.RunSchedule.RunOn.Days;
                    job.JobDataMap[Constants.FieldNames.Timezone] = jobDefinition.RunSchedule.Timezone;
                }
                else
                {
                    job.JobDataMap[Constants.FieldNames.RunAt] = string.Empty;
                    job.JobDataMap[Constants.FieldNames.ExclusionCalendar] = string.Empty;
                    job.JobDataMap[Constants.FieldNames.RunCalendar] = string.Empty;
                    job.JobDataMap[Constants.FieldNames.RunDays] = string.Empty;
                    job.JobDataMap[Constants.FieldNames.Timezone] = string.Empty;
                }

                actualJobs.Add(job);
            }
            return actualJobs;
        }
    }
}
