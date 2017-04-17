using System.Collections.Generic;
using JobScheduler.QuartzExtensions;
using NUnit.Framework;
using Quartz;
using Quartz.Impl;

namespace JobScheduler.Tests.QuartzExtensions
{
    [TestFixture]
    public class SchedulerExtensionsFixture
    {
        [Test]
        public void AddJobAndCreateTriggers_ForJobThatRequiresTrigger_WillCreateAndSchedule()
        {
            var job = TestHelper.CreateTestJob("test1", "01:07, 23:45", "mon, tue", null, null);

            var scheduler = StdSchedulerFactory.GetDefaultScheduler();

            scheduler.AddJobAndCreateTriggers(job);

            Assert.AreEqual(scheduler.GetJobDetail(job.Key), job);

            var jobTriggers = scheduler.GetTriggersOfJob(job.Key);
            Assert.That(jobTriggers.Count == 2);
            Assert.AreEqual("1:07 AM", jobTriggers[0].GetNextFireTimeUtc().Value.ToLocalTime().ToString("t"));
            Assert.AreEqual("11:45 PM", jobTriggers[1].GetNextFireTimeUtc().Value.ToLocalTime().ToString("t"));
        }

        [Test]
        public void AddJobAndCreateTriggers_ForJobThatDoesNotRequireTrigger_WillSimplyAdd()
        {
            var job = TestHelper.CreateTestJob("test2", null, null, null, null);

            var scheduler = StdSchedulerFactory.GetDefaultScheduler();

            scheduler.AddJobAndCreateTriggers(job);

            Assert.AreEqual(scheduler.GetJobDetail(job.Key), job);

            var jobTriggers = scheduler.GetTriggersOfJob(job.Key);

            Assert.That(jobTriggers.Count == 0);
        }
    }
}
