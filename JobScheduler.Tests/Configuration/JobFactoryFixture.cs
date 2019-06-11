using System.Collections.Generic;
using System.IO;
using QuartzFlow.Jobs;
using NUnit.Framework;

namespace QuartzFlow.Tests
{
    [TestFixture()]
    public class JobFactoryFixture
    {
        [Test]
        public void CreateFromJobDefinitions_ForValidJobDefinitionsAndRunSchedule_WillCreateCorrectly()
        {
            var runSchedule1 = new RunSchedule()
            {
                ExclusionCalendar = "None",
                RunAt = "11",
                RunOn = new RunOn() {Calendar = "Everyday", Days = "M"},
                Timezone = "AEST"
            };

            var runSchedule2 = new RunSchedule()
            {
                ExclusionCalendar = "Holidays",
                RunAt = "9",
                RunOn = new RunOn() { Calendar = "Weekdays", Days = "T,W" },
                Timezone = "NZST"
            };
            var jobDefinitions = new List<JobDefinition>
            {
                GetJobDefinition("testJob1", "SOD", "it does stuff", runSchedule1),
                GetJobDefinition("testJob2", "EOD", "it does even more stuff", runSchedule2)
            };
            var jobDetails = JobFactory.CreateFromJobDefinitions(jobDefinitions);

            var jobA = jobDetails[0];
            Assert.IsNotNull(jobA);
            Assert.IsTrue(jobA.JobType == typeof(ConsoleJob));
            Assert.AreEqual("SOD.testJob1", jobA.Key.ToString());
            Assert.AreEqual("it does stuff", jobA.Description);
            Assert.IsTrue(jobA.Durable);
            Assert.AreEqual("Hello", jobA.JobDataMap[Constants.FieldNames.ExecutableName]);
            Assert.AreEqual("None", jobA.JobDataMap[Constants.FieldNames.Parameters]);
            Assert.AreEqual(Path.Combine(SchedulerConfig.LogPath, "SOD-testJob1.txt"), jobA.JobDataMap[Constants.FieldNames.OutputFile]);
            Assert.AreEqual("SOD.a", jobA.JobDataMap[Constants.FieldNames.RunOnCompletionOf]);
            Assert.AreEqual("SOD.b", jobA.JobDataMap[Constants.FieldNames.RunOnFailureOf]);
            Assert.AreEqual("SOD.c", jobA.JobDataMap[Constants.FieldNames.RunOnSuccessOf]);
            Assert.AreEqual(3, jobA.JobDataMap[Constants.FieldNames.MaxRetries]);
            Assert.AreEqual(4, jobA.JobDataMap[Constants.FieldNames.WarnAfter]);
            Assert.AreEqual(2, jobA.JobDataMap[Constants.FieldNames.TerminateAfter]);
            Assert.AreEqual("11", jobA.JobDataMap[Constants.FieldNames.RunAt]);
            Assert.AreEqual("None", jobA.JobDataMap[Constants.FieldNames.ExclusionCalendar]);
            Assert.AreEqual("Everyday", jobA.JobDataMap[Constants.FieldNames.RunCalendar]);
            Assert.AreEqual("M", jobA.JobDataMap[Constants.FieldNames.RunDays]);
            Assert.AreEqual("AEST", jobA.JobDataMap[Constants.FieldNames.Timezone]);

            var jobB = jobDetails[1];
            Assert.IsNotNull(jobB);
            Assert.IsTrue(jobB.JobType == typeof(ConsoleJob));
            Assert.AreEqual("EOD.testJob2", jobB.Key.ToString());
            Assert.AreEqual("it does even more stuff", jobB.Description);
            Assert.IsTrue(jobB.Durable);
            Assert.AreEqual("Hello", jobB.JobDataMap[Constants.FieldNames.ExecutableName]);
            Assert.AreEqual("None", jobB.JobDataMap[Constants.FieldNames.Parameters]);
            Assert.AreEqual(Path.Combine(SchedulerConfig.LogPath, "EOD-testJob2.txt"), jobB.JobDataMap[Constants.FieldNames.OutputFile]);
            Assert.AreEqual("EOD.a", jobB.JobDataMap[Constants.FieldNames.RunOnCompletionOf]);
            Assert.AreEqual("EOD.b", jobB.JobDataMap[Constants.FieldNames.RunOnFailureOf]);
            Assert.AreEqual("EOD.c", jobB.JobDataMap[Constants.FieldNames.RunOnSuccessOf]);
            Assert.AreEqual(3, jobB.JobDataMap[Constants.FieldNames.MaxRetries]);
            Assert.AreEqual(4, jobB.JobDataMap[Constants.FieldNames.WarnAfter]);
            Assert.AreEqual(2, jobB.JobDataMap[Constants.FieldNames.TerminateAfter]);
            Assert.AreEqual("9", jobB.JobDataMap[Constants.FieldNames.RunAt]);
            Assert.AreEqual("Holidays", jobB.JobDataMap[Constants.FieldNames.ExclusionCalendar]);
            Assert.AreEqual("Weekdays", jobB.JobDataMap[Constants.FieldNames.RunCalendar]);
            Assert.AreEqual("T,W", jobB.JobDataMap[Constants.FieldNames.RunDays]);
            Assert.AreEqual("NZST", jobB.JobDataMap[Constants.FieldNames.Timezone]);
        }

        [Test]
        public void CreateFromJobDefinitions_ForSingleValidJobDefinitionAndNoSchedule_WillCreateCorrectly()
        {
            var jobDefinitions = new List<JobDefinition> { GetJobDefinition("testJob", "SOD", "it does stuff") };
            var jobDetails = JobFactory.CreateFromJobDefinitions(jobDefinitions);

            var jobA = jobDetails[0];
            Assert.IsNotNull(jobA);
            Assert.IsTrue(jobA.JobType == typeof(ConsoleJob));
            Assert.AreEqual("SOD.testJob", jobA.Key.ToString());
            Assert.AreEqual("it does stuff", jobA.Description);
            Assert.IsTrue(jobA.Durable);
            Assert.AreEqual("Hello", jobA.JobDataMap[Constants.FieldNames.ExecutableName]);
            Assert.AreEqual("None", jobA.JobDataMap[Constants.FieldNames.Parameters]);
            Assert.AreEqual(Path.Combine(SchedulerConfig.LogPath, "SOD-testJob.txt"), jobA.JobDataMap[Constants.FieldNames.OutputFile]);
            Assert.AreEqual("SOD.a", jobA.JobDataMap[Constants.FieldNames.RunOnCompletionOf]);
            Assert.AreEqual("SOD.b", jobA.JobDataMap[Constants.FieldNames.RunOnFailureOf]);
            Assert.AreEqual("SOD.c", jobA.JobDataMap[Constants.FieldNames.RunOnSuccessOf]);
            Assert.AreEqual(3, jobA.JobDataMap[Constants.FieldNames.MaxRetries]);
            Assert.AreEqual(4, jobA.JobDataMap[Constants.FieldNames.WarnAfter]);
            Assert.AreEqual(2, jobA.JobDataMap[Constants.FieldNames.TerminateAfter]);
            Assert.AreEqual(string.Empty, jobA.JobDataMap[Constants.FieldNames.RunAt]);
            Assert.AreEqual(string.Empty, jobA.JobDataMap[Constants.FieldNames.ExclusionCalendar]);
            Assert.AreEqual(string.Empty, jobA.JobDataMap[Constants.FieldNames.RunCalendar]);
            Assert.AreEqual(string.Empty, jobA.JobDataMap[Constants.FieldNames.RunDays]);
            Assert.AreEqual(string.Empty, jobA.JobDataMap[Constants.FieldNames.Timezone]);
        }

        private JobDefinition GetJobDefinition(string jobName, string group, string description, RunSchedule runSchedule = null)
        {
            JobDefinition jobDefinition = new JobDefinition()
            {
                JobName = jobName,
                Description = description,
                Group = group,
                RunSchedule =  runSchedule,
                ExecutableName = "Hello",
                Parameters = "None",
                Retries = 3,
                RunOnCompletionOf = new JobIdentifier() { Group = group, JobName = "a" },
                RunOnFailureOf = new JobIdentifier() { Group = group, JobName = "b" },
                RunOnSuccessOf = new JobIdentifier() { Group = group, JobName = "c" },
                TerminateAfter = 2,
                WarnAfter = 4
            };

            return jobDefinition;
        }
    }
}
