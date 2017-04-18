using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Quartz;
using Rhino.Mocks;
using System.Threading;
using JobScheduler.Calendars;
using JobScheduler.Listeners;

namespace JobScheduler.Tests
{
    [TestFixture]
    public class ConductorFixture
    {
        private Conductor _conductor;
        private IScheduler _mockScheduler;
        private IProcessManager _mockProcessManager;
        private List<ICalendar> _calendars;

        [SetUp]
        public void Setup()
        {
            _mockScheduler = MockRepository.GenerateMock<IScheduler>();
            _mockProcessManager = MockRepository.GenerateMock<IProcessManager>();
            _calendars = CustomCalendarFactory.CreateAnnualCalendarsWithSpecifiedDatesExcluded(
                new List<CustomCalendarDefinition>() {new CustomCalendarDefinition() {CalendarName = "AU_Without_Public_Holidays", Action = "exclude", Dates = new [] {new DateTime(2017, 1, 1), }} });
            _conductor = new Conductor(new List<JobDefinition>(), new List<ICalendar>(), 2000, 2000, _mockScheduler, _mockProcessManager);
        }

        [TearDown]
        public void TearDown()
        {
            _mockScheduler.VerifyAllExpectations();
            _mockProcessManager.VerifyAllExpectations();
        }

        [Test]
        public void CanCreateCorrectly()
        {
            StringReader sr = TestHelper.GetFileContents("test-jobs.json");
            var definitions = JobConfig.CreateJobDefinitions(sr);

            _mockScheduler.Expect(s => s.AddCalendar(Arg<string>.Is.Equal("AU_Without_Public_Holidays"), Arg<ICalendar>.Is.NotNull, Arg<bool>.Is.Equal(true), Arg<bool>.Is.Equal(true)));
            _mockScheduler.Expect(s => s.ScheduleJob(null, null, true)).IgnoreArguments();
            _mockScheduler.Expect(s => s.AddJob(null, true, true)).IgnoreArguments().Repeat.Twice();

            var mockListenerManager = MockRepository.GenerateMock<IListenerManager>();
            _mockScheduler.Expect(s => s.ListenerManager).Return(mockListenerManager).Repeat.Twice();
            mockListenerManager.Expect(l => l.AddJobListener(Arg<ConditionalJobChainingListener>.Is.TypeOf, Arg<IMatcher<JobKey>>.Is.Anything));
            mockListenerManager.Expect(l => l.AddJobListener(Arg<ConsoleJobListener>.Is.TypeOf, Arg<IMatcher<JobKey>>.Is.Anything));
            _conductor = new Conductor(definitions, _calendars, 2000, 2000, _mockScheduler, _mockProcessManager);

            mockListenerManager.VerifyAllExpectations();
        }

        [Test]
        public void Start_WillStartCorrectly()
        {
            _mockScheduler.Expect(s => s.Start());
            _conductor.StartScheduler();
            Assert.IsTrue(_conductor.IsWarningTimerRunning);
            Assert.IsTrue(_conductor.IsTerminationTimerRunning);
        }

        [Test]
        public void Stop_WillStopCorrectly()
        {
            _mockScheduler.Expect(s => s.Shutdown());
            _conductor.StartScheduler();
            _conductor.StopScheduler();
            Assert.IsFalse(_conductor.IsWarningTimerRunning);
            Assert.IsFalse(_conductor.IsTerminationTimerRunning);
        }

        [Test]
        public void WarningTimerElapsed_ForNormalRunningJob_WillNotRaiseEvent()
        {
            List<IJobExecutionContext> executingJobs = new List<IJobExecutionContext>();

            IJobExecutionContext jobContext1 = MockRepository.GenerateMock<IJobExecutionContext>();
            ITrigger trigger1 = MockRepository.GenerateMock<ITrigger>();
            ConfigureMocksForNormalRunningJob(jobContext1, trigger1);
            executingJobs.Add(jobContext1);

            _mockScheduler.Expect(s => s.GetCurrentlyExecutingJobs()).Return(executingJobs);
            _mockScheduler.Expect(s => s.Start());
            _mockScheduler.Expect(s => s.Shutdown());

            var returnValue = new List<string>();

            _conductor.JobsStillExecutingWarning += (sender, e) => { returnValue = e; };
            _conductor.StartScheduler();
            Thread.Sleep(7000);
            _conductor.StopScheduler();

            Assert.That(returnValue.Count, NUnit.Framework.Is.Zero);
            jobContext1.VerifyAllExpectations();
            trigger1.VerifyAllExpectations();
        }

        [Test]
        public void WarningTimerElapsed_ForLongRunningJob_WillRaiseEvent()
        {       
            List<IJobExecutionContext> executingJobs = new List<IJobExecutionContext>();

            IJobExecutionContext jobContext1 = MockRepository.GenerateMock<IJobExecutionContext>();
            ITrigger trigger1 = MockRepository.GenerateMock<ITrigger>();
            ConfigureMocksForNormalRunningJob(jobContext1, trigger1);
            executingJobs.Add(jobContext1);

            IJobExecutionContext jobContext2 = MockRepository.GenerateMock<IJobExecutionContext>();
            ITrigger trigger2 = MockRepository.GenerateMock<ITrigger>();
            ConfigureMocksForLateRunningJob(jobContext2, trigger2);
            executingJobs.Add(jobContext2);

            _mockScheduler.Expect(s => s.GetCurrentlyExecutingJobs()).Return(executingJobs);
            _mockScheduler.Expect(s => s.Start());
            _mockScheduler.Expect(s => s.Shutdown());

            var returnValue = new List<string>();
            _conductor.JobsStillExecutingWarning += (sender, e) => { returnValue = e; };
            _conductor.StartScheduler();
            Thread.Sleep(7000);
            _conductor.StopScheduler();

            var map = trigger2.JobDataMap;
            Assert.IsTrue((bool)map[Constants.FieldNames.HasIssuedLongRunningWarning]);

            Assert.That(returnValue.Count == 1);
            bool match = Regex.IsMatch(returnValue[0],
                @"Job DEFAULT.LateJob was started at \d\d\/\d\d\/\d{4} \d+:\d\d:\d\d [AP]M and is still running after \d+.\d\d minutes");
            Assert.IsTrue(match);

            jobContext1.VerifyAllExpectations();
            trigger1.VerifyAllExpectations();
        }

        [Test]
        public void WarningTimerElapsed_IfAlreadyWarnedForLongRunningJob_WillNotRaiseEventAgain()
        {
            List<IJobExecutionContext> executingJobs = new List<IJobExecutionContext>();

            IJobExecutionContext jobContext1 = MockRepository.GenerateMock<IJobExecutionContext>();
            ITrigger trigger1 = MockRepository.GenerateMock<ITrigger>();
            ConfigureMocksForNormalRunningJob(jobContext1, trigger1);
            executingJobs.Add(jobContext1);

            IJobExecutionContext jobContext2 = MockRepository.GenerateMock<IJobExecutionContext>();
            ITrigger trigger2 = MockRepository.GenerateMock<ITrigger>();
            ConfigureMocksForLateRunningJob(jobContext2, trigger2);
            executingJobs.Add(jobContext2);

            _mockScheduler.Expect(s => s.GetCurrentlyExecutingJobs()).Return(executingJobs);
            _mockScheduler.Expect(s => s.Start());
            _mockScheduler.Expect(s => s.Shutdown());

            int warnings = 0;
            _conductor.JobsStillExecutingWarning += (sender, e) => { warnings++; };
            _conductor.StartScheduler();
            Thread.Sleep(7000);
            _conductor.StopScheduler();

            var map = trigger2.JobDataMap;
            Assert.IsTrue((bool)map[Constants.FieldNames.HasIssuedLongRunningWarning]);
            Assert.That(warnings == 1);
            jobContext1.VerifyAllExpectations();
            trigger1.VerifyAllExpectations();
        }

        [Test]
        public void TerminationTimerElapsed_ForNormalRunningJob_WillNotDoAnything()
        {
            List<IJobExecutionContext> executingJobs = new List<IJobExecutionContext>();

            IJobExecutionContext jobContext1 = MockRepository.GenerateMock<IJobExecutionContext>();
            ConfigureMocksForNormalRunningJobForTerminationCheck(jobContext1);
            executingJobs.Add(jobContext1);

            _mockScheduler.Expect(s => s.GetCurrentlyExecutingJobs()).Return(executingJobs);
            _mockScheduler.Expect(s => s.Start());
            _mockScheduler.Expect(s => s.Shutdown());

            int terminationNotices = 0;
            _conductor.JobsTerminated += (sender, e) => { terminationNotices++; };
            _conductor.StartScheduler();
            Thread.Sleep(4000);
            _conductor.StopScheduler();

            Assert.That(terminationNotices == 0);
            jobContext1.VerifyAllExpectations();
        }

        [Test]
        public void TerminationTimerElapsed_ForLongRunningJob_WillKillProcessAndRaiseEvent()
        {
            List<IJobExecutionContext> executingJobs = new List<IJobExecutionContext>();

            IJobExecutionContext jobContext1 = MockRepository.GenerateMock<IJobExecutionContext>();
            ConfigureMocksForLongRunningJobForTerminationCheck(jobContext1);
            executingJobs.Add(jobContext1);

            _mockScheduler.Expect(s => s.GetCurrentlyExecutingJobs()).Return(executingJobs);
            _mockScheduler.Expect(s => s.Start());
            _mockScheduler.Expect(s => s.Shutdown());
            _mockProcessManager.Expect(p => p.KillProcess(1234));

            var returnValue = new List<string>();
            _conductor.JobsTerminated += (sender, e) => { returnValue = e; };
            _conductor.StartScheduler();
            Thread.Sleep(7000);
            _conductor.StopScheduler();

            Assert.That(returnValue.Count == 1);
            bool match = Regex.IsMatch(returnValue[0],
                @"Job DEFAULT.LateJob was started at \d\d\/\d\d\/\d{4} \d+:\d\d:\d\d [AP]M and was killed after \d+.\d\d minutes");
            Assert.IsTrue(match);
            jobContext1.VerifyAllExpectations();
        }

        private void ConfigureMocksForNormalRunningJob(IJobExecutionContext jobContext, ITrigger trigger)
        {
            trigger.Expect(t => t.JobDataMap).Return(new JobDataMap());
            jobContext.Expect(j => j.Trigger).Return(trigger);
            JobDataMap mergedMap1 = new JobDataMap
            {
                {Constants.FieldNames.WarnAfter, 10},
            };
            jobContext.Expect(j => j.MergedJobDataMap).Return(mergedMap1);
            jobContext.Expect(j => j.FireTimeUtc).Return(DateTime.UtcNow);
        }

        private void ConfigureMocksForNormalRunningJobForTerminationCheck(IJobExecutionContext jobContext)
        {
            JobDataMap mergedMap1 = new JobDataMap
            {
                {Constants.FieldNames.TerminateAfter, 10}
            };
            jobContext.Expect(j => j.MergedJobDataMap).Return(mergedMap1);
            jobContext.Expect(j => j.FireTimeUtc).Return(DateTime.UtcNow);
        }

        private void ConfigureMocksForLateRunningJob(IJobExecutionContext jobContext, ITrigger trigger)
        {
            var map = new JobDataMap
            {
                {Constants.FieldNames.HasIssuedLongRunningWarning, false}
            };
            trigger.Expect(t => t.JobDataMap).Return(map);

            jobContext.Expect(j => j.Trigger).Return(trigger);

            JobDataMap mergedMap2 = new JobDataMap
            {
                {Constants.FieldNames.WarnAfter, 1}
            };
            jobContext.Expect(j => j.MergedJobDataMap).Return(mergedMap2);
            jobContext.Expect(j => j.FireTimeUtc).Return(DateTime.UtcNow.Subtract(new TimeSpan(0, 2, 0)));

            IJobDetail jobDetail = MockRepository.GenerateMock<IJobDetail>();
            jobDetail.Expect(j => j.Key).Return(new JobKey("LateJob"));

            jobContext.Expect(j => j.JobDetail).Return(jobDetail);

            trigger.Expect(t => t.JobDataMap).Return(map);
        }

        private void ConfigureMocksForLongRunningJobForTerminationCheck(IJobExecutionContext jobContext)
        {
            JobDataMap mergedMap1 = new JobDataMap
            {
                {Constants.FieldNames.TerminateAfter, 1}
            };
            jobContext.Expect(j => j.MergedJobDataMap).Return(mergedMap1);
            jobContext.Expect(j => j.FireTimeUtc).Return(DateTime.UtcNow.Subtract(new TimeSpan(0, 2, 0)));

            jobContext.Expect(j => j.Get(Constants.FieldNames.ProcessId)).Return(1234);

            IJobDetail jobDetail = MockRepository.GenerateMock<IJobDetail>();
            jobDetail.Expect(j => j.Key).Return(new JobKey("LateJob"));

            jobContext.Expect(j => j.JobDetail).Return(jobDetail);
        }

    }
}
