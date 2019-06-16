using System.Collections.Generic;
using System.Linq;
using QuartzFlow.QuartzExtensions;
using NUnit.Framework;
using Quartz;
using Quartz.Impl;

namespace QuartzFlow.Tests.QuartzExtensions
{
    [TestFixture]
    public class SchedulerExtensionsFixture
    {
        [Test]
        public void AddJobAndCreateTriggers_ForJobThatRequiresTrigger_WillCreateAndSchedule()
        {
            var job = TestHelper.CreateTestJob("test1", "01:07, 23:45", "mon, tue", null, null);

            var scheduler = StdSchedulerFactory.GetDefaultScheduler().Result;

            scheduler.AddJobAndCreateTriggers(job);

            Assert.AreEqual(scheduler.GetJobDetail(job.Key), job);

            var jobTriggers = scheduler.GetTriggersOfJob(job.Key).Result;
            Assert.That(jobTriggers.Count == 2);

            jobTriggers.GetEnumerator().Reset();
            Assert.AreEqual("1:07 AM", jobTriggers.ElementAt(0).GetNextFireTimeUtc().Value.ToLocalTime().ToString("t"));
            Assert.AreEqual("11:45 PM", jobTriggers.ElementAt(0).GetNextFireTimeUtc().Value.ToLocalTime().ToString("t"));
        }

        [Test]
        public void AddJobAndCreateTriggers_ForJobThatDoesNotRequireTrigger_WillSimplyAdd()
        {
            var job = TestHelper.CreateTestJob("test2", null, null, null, null);

            var scheduler = StdSchedulerFactory.GetDefaultScheduler().Result;

            scheduler.AddJobAndCreateTriggers(job);

            Assert.AreEqual(scheduler.GetJobDetail(job.Key), job);

            var jobTriggers = scheduler.GetTriggersOfJob(job.Key).Result;

            Assert.That(jobTriggers.Count == 0);
        }
    }
}
