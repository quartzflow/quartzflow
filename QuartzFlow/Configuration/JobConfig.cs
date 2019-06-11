using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using Newtonsoft.Json;
using Quartz.Util;

namespace QuartzFlow
{
    public class JobConfig
    {
        public static List<JobDefinition> CreateJobDefinitions(string filename)
        {
            var jobsReader = new StringReader(File.ReadAllText(filename));
            return CreateJobDefinitions(jobsReader);
        }

        public static List<JobDefinition> CreateJobDefinitions(StringReader configReader)
        {
            string jsonConfig = configReader.ReadToEnd();   
            var jobs = JsonConvert.DeserializeObject<List<JobDefinition>>(jsonConfig);

            ValidateConfig(jobs);

            return jobs;
        }

        private static void ValidateConfig(List<JobDefinition> jobs)
        {
            if (jobs.Any(j => j.Group.IsNullOrWhiteSpace()))
                throw new Exception(
                    "Failed to create job defintions from config - one or more of the jobs has a blank or missing Group value");

            if (jobs.Any(j => j.JobName.IsNullOrWhiteSpace()))
                throw new Exception(
                    "Failed to create job defintions from config - one or more of the jobs has a blank or missing JobName value");

            foreach (var job in jobs)
            {
                ValidateDependency(job, jobs, job.RunOnCompletionOf);
                ValidateDependency(job, jobs, job.RunOnFailureOf);
                ValidateDependency(job, jobs, job.RunOnSuccessOf);
            }
        }

        private static void ValidateDependency(JobDefinition job, List<JobDefinition> jobs, JobIdentifier dependentJob)
        {
            if (dependentJob != null)
            {
                if (jobs.FirstOrDefault(
                        j => (j.Group == dependentJob.Group && j.JobName == dependentJob.JobName)) == null)
                {
                    throw new Exception(
                        $"Failed to create job definitions from config - job '{job.JobName}' has a dependency '{dependentJob.JobName}' that does not exist");
                }
            }
        }
    }
}
