using System;
using NUnit.Framework;
using JobScheduler.Listeners;
using Quartz;
using Rhino.Mocks;
using Common.Logging;

namespace JobScheduler.Tests.Listeners
{
    [TestFixture()]
    public class ConsoleJobListenerFixture
    {
        private ConsoleJobListener _listener;
        private IJobExecutionContext _context;
        private IScheduler _scheduler;
        private IJobDetail _jobDetail;
        private TestLogger _testLogger;

        [SetUp]
        public void Setup()
        {
            _testLogger = new TestLogger();
            LogManager.Adapter = _testLogger.LoggingAdapter;
            _listener = new ConsoleJobListener();
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
            Assert.AreEqual("ConsoleJobListener", _listener.Name);
        }

       [Test]
        public void JobBeExecuted_ForValidJob_WillLogCorrectly()
        {
            JobKey jobA = new JobKey("JobA");

            _jobDetail.Expect(j => j.Key).Return(jobA);
            _context.Expect(c => c.JobDetail).Return(_jobDetail);

            _listener.JobToBeExecuted(_context);

            _testLogger.AssertInfoMessagesLogged($"-----About to run '{_context.JobDetail.Key}'");
        }

        [Test]
        public void JobExecutionVetoed_ForValidJob_WillLogCorrectly()
        {
            JobKey jobA = new JobKey("JobA");

            _jobDetail.Expect(j => j.Key).Return(jobA);
            _context.Expect(c => c.JobDetail).Return(_jobDetail);

            _listener.JobExecutionVetoed(_context);

            _testLogger.AssertInfoMessagesLogged($"-----Run of '{_context.JobDetail.Key}' was vetoed!");
        }

        [Test]
        public void JobWasExecuted_ForValidJob_WillLogCorrectly()
        {
            var programOutput = "Some program output";

            _context.Expect(c => c.Get(Constants.FieldNames.StandardOutput)).Return(programOutput);

            _listener.JobWasExecuted(_context, null);

            var messages = _testLogger.GetLoggedMessages();
            Assert.AreEqual($"-----Execution log:\r\n{programOutput}", messages[0].RenderedMessage);
            Assert.AreEqual("---------------------", messages[1].RenderedMessage);
        }



       
    }
}
