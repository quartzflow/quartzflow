using System;
using NUnit.Framework;
using JobScheduler.Listeners;
using Quartz;
using Rhino.Mocks;
using Common.Logging;

namespace JobScheduler.Tests.Listeners
{
    [TestFixture()]
    public class ConditionalJobChainingListenerFixture
    {
        private ConditionalJobChainingListener _listener;
        private IJobExecutionContext _context;
        private IScheduler _scheduler;
        private IJobDetail _jobDetail;
        private TestLogger _testLogger;

        [SetUp]
        public void Setup()
        {
            _testLogger = new TestLogger();
            LogManager.Adapter = _testLogger.LoggingAdapter;
            _listener = new ConditionalJobChainingListener();
            _context = MockRepository.GenerateMock<IJobExecutionContext>();
            _scheduler = MockRepository.GenerateMock<IScheduler>();
            _context.Expect(c => c.Scheduler).Return(_scheduler).Repeat.Times(0, 100);
            _jobDetail = MockRepository.GenerateMock<IJobDetail>();   
        }

        [TearDown]
        public void TearDown()
        {
            _context.VerifyAllExpectations();
            _scheduler.VerifyAllExpectations();
            _jobDetail.VerifyAllExpectations();
            _testLogger.Clear();
        }

        [Test]
        public void Name_IsCorrect()
        {
            Assert.AreEqual("ConditionalJobChainingListener", _listener.Name);
        }

        [Test]
        public void AddJobChainLink_ForValidParameters_WillAddLink()
        {
            JobKey jobA = new JobKey("JobA");
            JobKey jobB = new JobKey("JobB");
            JobResultCriteria resultCriteria = JobResultCriteria.OnCompletion;

            _listener.AddJobChainLink(jobA, resultCriteria, jobB);

            var links = _listener.GetChainLinks();

            Assert.AreEqual(jobB, links[jobA].DependentJobKey);
            Assert.AreEqual(resultCriteria, links[jobA].PredecessorJobResult);
        }

        [Test]
        public void AddJobChainLink_ForInvalidFirstJobKey_WillThrowException()
        {
            ArgumentException ex = Assert.Throws<ArgumentException>(() => _listener.AddJobChainLink(null, JobResultCriteria.OnCompletion, new JobKey("JobB")));
            Assert.AreEqual("Key cannot be null!", ex.Message);
        }

        [Test]
        public void AddJobChainLink_ForInvalidFirstJobKeyName_WillThrowException()
        {
            ArgumentException ex = Assert.Throws<ArgumentException>(() => _listener.AddJobChainLink(new JobKey(""), JobResultCriteria.OnCompletion, new JobKey("JobB")));
            Assert.AreEqual("Key cannot have a null name!", ex.Message);
        }

        [Test]
        public void AddJobChainLink_ForInvalidSecondJobKey_WillThrowException()
        {
            ArgumentException ex = Assert.Throws<ArgumentException>(() => _listener.AddJobChainLink(new JobKey("JobA"), JobResultCriteria.OnCompletion, null));
            Assert.AreEqual("Key cannot be null!", ex.Message);
        }

        [Test]
        public void AddJobChainLink_ForInvalidSecondJobKeyName_WillThrowException()
        {
            ArgumentException ex = Assert.Throws<ArgumentException>(() => _listener.AddJobChainLink(new JobKey("A"), JobResultCriteria.OnCompletion, new JobKey("")));
            Assert.AreEqual("Key cannot have a null name!", ex.Message);
        }

        [Test]
        public void JobWasExecuted_NoDependencySpecified_WillNotAttemptToDoAnything()
        {
            JobKey jobA = new JobKey("JobA");

            _jobDetail.Expect(j => j.Key).Return(jobA);
            _context.Expect(c => c.JobDetail).Return(_jobDetail);

            _listener.JobWasExecuted(_context, null);

            _testLogger.AssertNoMessagesLogged();
        }

        [Test]
        [TestCase(JobExecutionStatus.Succeeded)]
        [TestCase(JobExecutionStatus.Failed)]
        public void JobWasExecuted_ForOnCompletionTriggerAndCompletedState_WillTriggerNextJob(JobExecutionStatus status)
        {
            JobKey jobA = new JobKey("JobA");
            JobKey jobB = new JobKey("JobB");

            _jobDetail.Expect(j => j.Key).Return(jobA);
            _context.Expect(c => c.JobDetail).Return(_jobDetail);
            _context.Expect(c => c.Result).Return(status);
            _context.Scheduler.Expect(s => s.TriggerJob(jobB));

            _listener.AddJobChainLink(jobA, JobResultCriteria.OnCompletion, jobB);
            _listener.JobWasExecuted(_context, null);

            _testLogger.AssertInfoMessageLogged($"Completion of Job 'DEFAULT.{jobA.Name}' will now trigger Job 'DEFAULT.{jobB.Name}'");
        }

        [Test]
        public void JobWasExecuted_ForOnCompletionTriggerAndRetryingState_WillNotTriggerNextJob()
        {
            JobKey jobA = new JobKey("JobA");
            JobKey jobB = new JobKey("JobB");

            _jobDetail.Expect(j => j.Key).Return(jobA);
            _context.Expect(c => c.JobDetail).Return(_jobDetail);
            _context.Expect(c => c.Result).Return(JobExecutionStatus.Retrying);

            _listener.AddJobChainLink(jobA, JobResultCriteria.OnCompletion, jobB);
            _listener.JobWasExecuted(_context, null);

            _testLogger.AssertNoMessagesLogged();
        }

        [Test]
        public void JobWasExecuted_ForOnSuccessTriggerAndSucceededState_WillTriggerNextJob()
        {
            JobKey jobA = new JobKey("JobA");
            JobKey jobB = new JobKey("JobB");

            _jobDetail.Expect(j => j.Key).Return(jobA);
            _context.Expect(c => c.JobDetail).Return(_jobDetail);
            _context.Expect(c => c.Result).Return(JobExecutionStatus.Succeeded);
            _context.Scheduler.Expect(s => s.TriggerJob(jobB));

            _listener.AddJobChainLink(jobA, JobResultCriteria.OnSuccess, jobB);
            _listener.JobWasExecuted(_context, null);

            _testLogger.AssertInfoMessageLogged($"Success of Job 'DEFAULT.{jobA.Name}' will now trigger Job 'DEFAULT.{jobB.Name}'");
        }

        [Test]
        public void JobWasExecuted_ForFailedAttemptToTriggerJob_WillLogError()
        {
            JobKey jobA = new JobKey("JobA");
            JobKey jobB = new JobKey("JobB");

            _jobDetail.Expect(j => j.Key).Return(jobA);
            _context.Expect(c => c.JobDetail).Return(_jobDetail);
            _context.Expect(c => c.Result).Return(JobExecutionStatus.Succeeded);
            var ex = new SchedulerException();
            _context.Scheduler.Expect(s => s.TriggerJob(jobB)).Throw(ex);

            _listener.AddJobChainLink(jobA, JobResultCriteria.OnSuccess, jobB);
            _listener.JobWasExecuted(_context, null);

            var messages = _testLogger.GetLoggedMessages();
            Assert.AreEqual(2, messages.Count);
            Assert.AreEqual($"Success of Job 'DEFAULT.{jobA.Name}' will now trigger Job 'DEFAULT.{jobB.Name}'", messages[0].RenderedMessage);
            Assert.AreEqual(LogLevel.Info, messages[0].Level);
            Assert.AreEqual($"Error encountered triggering Job 'DEFAULT.{jobB.Name}'", messages[1].RenderedMessage);
            Assert.AreEqual(LogLevel.Error, messages[1].Level);
            Assert.AreEqual(ex, messages[1].Exception);
        }

        [Test]
        [TestCase(JobExecutionStatus.Retrying)]
        [TestCase(JobExecutionStatus.Failed)]
        public void JobWasExecuted_ForOnSuccessTriggerAndUnsuccessfulState_WillNotTriggerNextJob(JobExecutionStatus status)
        {
            JobKey jobA = new JobKey("JobA");
            JobKey jobB = new JobKey("JobB");

            _jobDetail.Expect(j => j.Key).Return(jobA);
            _context.Expect(c => c.JobDetail).Return(_jobDetail);
            _context.Expect(c => c.Result).Return(status);

            _listener.AddJobChainLink(jobA, JobResultCriteria.OnSuccess, jobB);
            _listener.JobWasExecuted(_context, null);

            _testLogger.AssertNoMessagesLogged();
        }

        [Test]
        public void JobWasExecuted_ForOnFailureTriggerAndFailedState_WillTriggerNextJob()
        {
            JobKey jobA = new JobKey("JobA");
            JobKey jobB = new JobKey("JobB");

            _jobDetail.Expect(j => j.Key).Return(jobA);
            _context.Expect(c => c.JobDetail).Return(_jobDetail);
            _context.Expect(c => c.Result).Return(JobExecutionStatus.Failed);
            _context.Scheduler.Expect(s => s.TriggerJob(jobB));

            _listener.AddJobChainLink(jobA, JobResultCriteria.OnFailure, jobB);
            _listener.JobWasExecuted(_context, null);

            _testLogger.AssertInfoMessageLogged($"Failure of Job 'DEFAULT.{jobA.Name}' will now trigger Job 'DEFAULT.{jobB.Name}'");
        }

        [Test]
        [TestCase(JobExecutionStatus.Retrying)]
        [TestCase(JobExecutionStatus.Succeeded)]
        public void JobWasExecuted_ForOnFailureTriggerAndNonFailedState_WillNotTriggerNextJob(JobExecutionStatus status)
        {
            JobKey jobA = new JobKey("JobA");
            JobKey jobB = new JobKey("JobB");

            _jobDetail.Expect(j => j.Key).Return(jobA);
            _context.Expect(c => c.JobDetail).Return(_jobDetail);
            _context.Expect(c => c.Result).Return(status);

            _listener.AddJobChainLink(jobA, JobResultCriteria.OnFailure, jobB);
            _listener.JobWasExecuted(_context, null);

            _testLogger.AssertNoMessagesLogged();
        }
    }
}
