using System;
using System.Collections.Generic;
using System.IO;
using Quartz;

namespace QuartzFlow.Tests
{
    public class TestHelper
    {
        public static StringReader GetFileContents(string filename) => new StringReader(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory + @"\TestData", filename)));

        public static IJobDetail CreateTestJob(string jobName, string runAt, string runOn, string exclusionCalendar, string timeZoneId)
        {
            var jobs = JobFactory.CreateFromJobDefinitions(new List<JobDefinition>()
            {
                new JobDefinition()
                {
                    JobName = jobName,
                    RunSchedule = new RunSchedule()
                    {
                        ExclusionCalendar = exclusionCalendar,
                        RunAt = runAt,
                        RunOn = new RunOn() {Calendar = null, Days = runOn},
                        Timezone = timeZoneId
                    }
                }
            });
            return jobs[0];
        }
    }   
}