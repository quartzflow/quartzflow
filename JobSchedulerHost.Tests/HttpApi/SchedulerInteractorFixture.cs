using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JobScheduler;
using JobScheduler.QuartzExtensions;
using JobSchedulerHost.HttpApi;
using NUnit.Framework;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Quartz.Job;
using Rhino.Mocks;

namespace JobSchedulerHost.Tests.HttpApi
{
    [TestFixture()]
    public class SchedulerInteractorFixture
    {
        private IScheduler _mockScheduler;
        private SchedulerInteractor _interactor;
        private IProcessManager _mockProcessManager;

        [SetUp]
        public void Setup()
        {
            _mockScheduler = MockRepository.GenerateMock<IScheduler>();
            _mockProcessManager = MockRepository.GenerateMock<IProcessManager>();
            _interactor = new SchedulerInteractor(_mockScheduler, _mockProcessManager);

        }

        [TearDown]
        public void TearDown()
        {
            _mockScheduler.VerifyAllExpectations();
            _mockProcessManager.VerifyAllExpectations();
        }

        [Test]
        public void GetStatus_WhenNoSchedulerPresent_IsCorrect()
        {
            Assert.AreEqual("Not set", new SchedulerInteractor(null, null).GetStatus());
        }

        [Test]
        public void GetStatus_WhenShutdown_IsCorrect()
        {
            _mockScheduler.Expect(s => s.IsShutdown).Return(true);
            Assert.AreEqual("Shutdown", _interactor.GetStatus());
        }

        [Test]
        public void GetStatus_WhenInStandby_IsCorrect()
        {
            _mockScheduler.Expect(s => s.InStandbyMode).Return(true);
            Assert.AreEqual("InStandBy", _interactor.GetStatus());
        }

        [Test]
        public void GetStatus_WhenStarted_IsCorrect()
        {
            _mockScheduler.Expect(s => s.IsStarted).Return(true);
            Assert.AreEqual("Started", _interactor.GetStatus());
        }

        [Test]
        public void GetJobNames_ForNoJobs_ReturnsEmptyList()
        {
            _mockScheduler.Expect(s => s.GetJobKeys(GroupMatcher<JobKey>.AnyGroup()))
                .Return(new Quartz.Collection.HashSet<JobKey>());
            Assert.AreEqual(0, _interactor.GetJobs().Count);
        }

        [Test]
        public void GetJobNames_ForPresentJobs_ReturnsNames()
        {
            _mockScheduler.Expect(s => s.GetJobKeys(GroupMatcher<JobKey>.AnyGroup()))
                .Return(new Quartz.Collection.HashSet<JobKey>() {JobKey.Create("job1", "SOD"), JobKey.Create("job2", "SOD")});

            IJobDetail job1 = GetMockJob("job1", "SOD");
            IJobDetail job2 = GetMockJob("job2", "SOD");

            var trigger = MockRepository.GenerateMock<ITrigger>();
            trigger.Expect(t => t.GetNextFireTimeUtc()).Return(new DateTimeOffset(DateTime.UtcNow.AddMinutes(5))).Repeat.Twice();
            trigger.Expect(t => t.Key).Return(new TriggerKey("someKey")).Repeat.Twice();
            var triggers = new List<ITrigger>() { trigger };

            _mockScheduler.Expect(s => s.GetJobDetail(Arg<JobKey>.Is.TypeOf)).Return(job1).Repeat.Once();
            _mockScheduler.Expect(s => s.GetJobDetail(Arg<JobKey>.Is.TypeOf)).Return(job2).Repeat.Once();
            _mockScheduler.Expect(s => s.GetTriggersOfJob(Arg<JobKey>.Is.TypeOf)).Return(triggers).Repeat.Times(4);
            _mockScheduler.Expect(s => s.GetTriggerState(Arg<TriggerKey>.Is.TypeOf)).Return(TriggerState.Normal).Repeat.Twice();

            var result = _interactor.GetJobs();
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("SOD.job1", result[0].Name);
            Assert.AreEqual("SOD.job2", result[1].Name);

            job1.VerifyAllExpectations();
            job2.VerifyAllExpectations();
        }

        [Test]
        public void GetCurrentlyExecutingJobs_ForNoJobs_ReturnsEmptyList()
        {
            _mockScheduler.Expect(s => s.GetCurrentlyExecutingJobs())
                .Return(new List<IJobExecutionContext>());
            Assert.AreEqual(0, _interactor.GetCurrentlyExecutingJobs().Count);
        }

        [Test]
        public void GetCurrentlyExecutingJobs_ForPresentJobs_ReturnsNames()
        {
            var job1StartTime = DateTime.UtcNow.Subtract(new TimeSpan(0, 0, 0, 5));
            var context1 = MockRepository.GenerateMock<IJobExecutionContext>();
            context1.Expect(c => c.FireTimeUtc)
                .Return(new DateTimeOffset(job1StartTime)).Repeat.Twice();
            context1.Expect(c => c.JobDetail).Return(new JobDetailImpl("job1", typeof(NoOpJob)));

            var job2StartTime = DateTime.UtcNow.Subtract(new TimeSpan(0, 0, 0, 15));
            var context2 = MockRepository.GenerateMock<IJobExecutionContext>();
            context2.Expect(c => c.FireTimeUtc)
                .Return(new DateTimeOffset(job2StartTime)).Repeat.Twice();
            context2.Expect(c => c.JobDetail).Return(new JobDetailImpl("job2", typeof(NoOpJob)));

            _mockScheduler.Expect(s => s.GetCurrentlyExecutingJobs())
                .Return(new List<IJobExecutionContext>() {context1, context2 });

            var result = _interactor.GetCurrentlyExecutingJobs();
            Assert.AreEqual(2, result.Count);

            Assert.AreEqual(job1StartTime, result[0].StartedAt.ToUniversalTime());
            Assert.AreEqual("DEFAULT.job1", result[0].Name);
            Assert.AreEqual(job2StartTime, result[1].StartedAt.ToUniversalTime());
            Assert.AreEqual("DEFAULT.job2", result[1].Name);

            context1.VerifyAllExpectations();
            context2.VerifyAllExpectations();
        }

        [Test]
        public void GetJobDetails_ForExistingJob_ReturnsCorrectInfo()
        {
            IJobDetail job = GetMockJob("job1", "SOD");

            var trigger = MockRepository.GenerateMock<ITrigger>();
            DateTimeOffset triggerTime = new DateTimeOffset(DateTime.UtcNow.AddMinutes(5));
            trigger.Expect(t => t.GetNextFireTimeUtc()).Return(triggerTime).Repeat.Twice();
            var triggers = new List<ITrigger>() {trigger};
            
            _mockScheduler.Expect(s => s.GetJobDetail(Arg<JobKey>.Is.TypeOf)).Return(job);
            _mockScheduler.Expect(s => s.GetTriggersOfJob(Arg<JobKey>.Is.TypeOf)).Return(triggers);

            var result = _interactor.GetJobDetails("SOD.job1");

            Assert.AreEqual("SOD.job1", result.Name);
            Assert.AreEqual("this does something", result.Description);

            bool isLocalInDaylightSavingTime = TimeZoneInfo.Local.IsDaylightSavingTime(triggerTime.LocalDateTime);
            var localTimeZoneName = isLocalInDaylightSavingTime ? TimeZoneInfo.Local.DaylightName : TimeZoneInfo.Local.StandardName;
            Assert.IsTrue(result.NextRunAt.StartsWith(triggerTime.ToLocalTime().ToString("F") + " " + localTimeZoneName));
            Assert.AreEqual(2, result.Properties.Count);
            Assert.AreEqual("something", result.Properties.FirstOrDefault(p => p.Key == "prop1").Value);
            Assert.AreEqual("something else", result.Properties.FirstOrDefault(p => p.Key == "prop2").Value);

            job.VerifyAllExpectations();
            trigger.VerifyAllExpectations();
        }

        [Test]
        public void GetJobDetails_ForNotExistantJob_ReturnsNull()
        {
            _mockScheduler.Expect(s => s.GetJobDetail(Arg<JobKey>.Is.TypeOf)).Return(null);
            Assert.IsNull(_interactor.GetJobDetails("SOD.job1"));
        }

        [Test]
        public void JobExists_ForGivenJob_WillAttemptToFind()
        {
            _mockScheduler.Expect(s => s.CheckExists(Arg<JobKey>.Is.TypeOf)).Return(true);
            Assert.IsTrue(_interactor.JobExists("job1"));
        }

        [Test]
        public void PauseJob_ForGivenJob_WillAttemptToPause()
        {
            _mockScheduler.Expect(s => s.PauseJob(Arg<JobKey>.Is.TypeOf));
            _interactor.PauseJob("job1");
        }

        [Test]
        public void PauseAllJobs_WillAttemptToPauseAllJobs()
        {
            _mockScheduler.Expect(s => s.PauseJobs(GroupMatcher<JobKey>.AnyGroup()));
            _interactor.PauseAllJobs();
        }

        [Test]
        public void ResumeJob_ForGivenJob_WillAttemptToResume()
        {
            _mockScheduler.Expect(s => s.ResumeJob(Arg<JobKey>.Is.TypeOf));
            _interactor.ResumeJob("job1");
        }

        [Test]
        public void ResumeAllJobs_WillAttemptToResumeAllJobs()
        {
            _mockScheduler.Expect(s => s.ResumeJobs(GroupMatcher<JobKey>.AnyGroup()));
            _interactor.ResumeAllJobs();
        }

        [Test]
        public void StartJob_ForGivenJob_WillAttemptToStart()
        {
            _mockScheduler.Expect(s => s.TriggerJob(Arg<JobKey>.Is.TypeOf));
            _interactor.StartJob("job1");
        }

        [Test]
        public void KillJob_ForRunningJob_WillAttemptToKill()
        {
            var context1 = MockRepository.GenerateMock<IJobExecutionContext>();
            context1.Expect(c => c.JobDetail).Return(new JobDetailImpl("job1", typeof(NoOpJob)));

            var context2 = MockRepository.GenerateMock<IJobExecutionContext>();
            context2.Expect(c => c.JobDetail).Return(new JobDetailImpl("job2", typeof(NoOpJob)));
            context2.Expect(c => c.Get(Arg<object>.Is.Anything)).Return(1234);

            _mockScheduler.Expect(s => s.GetCurrentlyExecutingJobs())
                .Return(new List<IJobExecutionContext>() { context1, context2 });

            _mockProcessManager.Expect(pm => pm.KillProcess(1234));

            var result = _interactor.KillJob("DEFAULT.job2");

            Assert.IsTrue(result);
            context1.VerifyAllExpectations();
            context2.VerifyAllExpectations();
        }

        [Test]
        public void KillJob_ForNonRunningJob_WillReturnFalse()
        {
            var context1 = MockRepository.GenerateMock<IJobExecutionContext>();
            context1.Expect(c => c.JobDetail).Return(new JobDetailImpl("job1", typeof(NoOpJob)));

            var context2 = MockRepository.GenerateMock<IJobExecutionContext>();
            context2.Expect(c => c.JobDetail).Return(new JobDetailImpl("job2", typeof(NoOpJob)));

            _mockScheduler.Expect(s => s.GetCurrentlyExecutingJobs())
                .Return(new List<IJobExecutionContext>() { context1, context2 });

            var result = _interactor.KillJob("DEFAULT.job3");

            Assert.IsFalse(result);
            context1.VerifyAllExpectations();
            context2.VerifyAllExpectations();
        }

        private IJobDetail GetMockJob(string jobName, string groupName)
        {
            var job = MockRepository.GenerateMock<IJobDetail>();
            job.Expect(j => j.JobDataMap).Return(new JobDataMap() { new KeyValuePair<string, object>("prop1", "something"), new KeyValuePair<string, object>("prop2", "something else") });

            var key = JobKey.Create(jobName, groupName);
            job.Expect(j => j.Key).Return(key).Repeat.Any();
            job.Expect(j => j.Description).Return("this does something");

            return job;
        }
    }
}
