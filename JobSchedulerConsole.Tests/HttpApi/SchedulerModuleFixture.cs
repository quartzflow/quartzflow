using System.Collections.Generic;
using System.Linq;
using CsQuery.Utility;
using JobSchedulerHost.HttpApi;
using Nancy;
using Nancy.Testing;
using NUnit.Framework;
using Rhino.Mocks;
using Browser = Nancy.Testing.Browser;

namespace JobSchedulerHost.Tests.HttpApi
{
    [TestFixture]
    public class SchedulerModuleFixture
    {
        private ISchedulerInteractor _interactor;
        private Browser _browser;

        [SetUp]
        public void Setup()
        {
            _interactor = MockRepository.GenerateMock<ISchedulerInteractor>();
            _browser = new Browser(with =>
            {
                with.Module<SchedulerModule>();
                with.Dependency<ISchedulerInteractor>(_interactor);
            });
        }

        [TearDown]
        public void TearDown()
        {
            _interactor.VerifyAllExpectations();
        }

        [Test]
        public void Get_Status_WithNoParameter_ReturnsResultAndOK()
        {
            _interactor.Expect(i => i.GetStatus()).Return("started");

            var result = _browser.Get("/scheduler/status", with => {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual("started", result.Body.AsString());
        }

        [Test]
        public void Get_Jobs_WithNoCriteria_ReturnsResultAndOK()
        {
            _interactor.Expect(i => i.GetJobNames()).Return(new List<string> {"job1", "job2"});

            var result = _browser.Get("/scheduler/jobs", with => {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

            var resultList = JSON.ParseJSONValue<List<string>>(result.Body.AsString());
            Assert.AreEqual(2, resultList.Count);
            Assert.AreEqual("job1", resultList[0]);
            Assert.AreEqual("job2", resultList[1]);
        }

        [Test]
        public void Get_Jobs_ForCurrentlyExecutingJobsCriteria_ReturnsResultAndOK()
        {
            _interactor.Expect(i => i.GetCurrentlyExecutingJobs()).Return(new List<string> { "job1", "job2" });

            var result = _browser.Get("/scheduler/jobs", with => {
                with.HttpRequest();
                with.Query("criteria", "executing");
            });

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

            var resultList = JSON.ParseJSONValue<List<string>>(result.Body.AsString());
            Assert.AreEqual(2, resultList.Count);
            Assert.AreEqual("job1", resultList[0]);
            Assert.AreEqual("job2", resultList[1]);
        }

        [Test]
        public void Get_Jobs_ForInvalidCritieria_ReturnsNoResultAndNotFound()
        {
            _interactor.Expect(i => i.GetJobNames()).Return(null).Repeat.Never();
            _interactor.Expect(i => i.GetCurrentlyExecutingJobs()).Return(null).Repeat.Never();

            var result = _browser.Get("/scheduler/jobs", with => {
                with.HttpRequest();
                with.Query("criteria", "gibberish");
            });

            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);
            Assert.IsTrue(string.IsNullOrEmpty(result.Body.AsString()));
        }

        [Test]
        public void Get_Job_ForExistingJob_ReturnsResultAndOK()
        {
            _interactor.Expect(i => i.JobExists("job1")).Return(true);

            var props = new SortedList<string, string> {{"retries", "2"}};
            _interactor.Expect(i => i.GetJobDetails("job1"))
                        .Return(new JobDetailsModel() {Description = "something", Name = "job1", NextRunAt = "blah", Properties = props });

            var result = _browser.Get("/scheduler/jobs/job1", with => {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

            var responseModel = JSON.ParseJSON<JobDetailsModel>(result.Body.AsString());
            Assert.AreEqual("job1", responseModel.Name);
            Assert.AreEqual("something", responseModel.Description);
            Assert.AreEqual("blah", responseModel.NextRunAt);
            Assert.AreEqual(1, responseModel.Properties.Count);
            Assert.AreEqual("2", responseModel.Properties.FirstOrDefault(a => a.Key == "retries").Value);
        }

        [Test]
        public void Get_Job_ForNonExistingJob_ReturnsNoResultAndNotFound()
        {
            _interactor.Expect(i => i.JobExists("job1")).Return(false);
            _interactor.Expect(i => i.GetJobDetails("job1")).Return(null).Repeat.Never();

            var result = _browser.Get("/scheduler/jobs/job1", with => {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);
            Assert.IsTrue(string.IsNullOrEmpty(result.Body.AsString()));
        }

        [Test]
        public void Put_Job_PauseAllJobs_ReturnsNoResultAndOK()
        {
            _interactor.Expect(i => i.PauseAllJobs());

            var response = SendJobActionRequest("", HttpApiConstants.JobAction.Pause);

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
            Assert.IsTrue(string.IsNullOrEmpty(response.Body.AsString()));
        }

        [Test]
        public void Put_Job_ResumeAllJobs_ReturnsNoResultAndOK()
        {
            _interactor.Expect(i => i.ResumeAllJobs());

            var response = SendJobActionRequest("", HttpApiConstants.JobAction.Resume);

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
            Assert.IsTrue(string.IsNullOrEmpty(response.Body.AsString()));
        }

        [Test]
        public void Put_Job_PauseExistingJob_ReturnsNoResultAndOK()
        {
            _interactor.Expect(i => i.JobExists("job1")).Return(true);
            _interactor.Expect(i => i.PauseJob("job1"));

            var response = SendJobActionRequest("job1", HttpApiConstants.JobAction.Pause);

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
            Assert.IsTrue(string.IsNullOrEmpty(response.Body.AsString()));
        }

        [Test]
        public void Put_Job_PauseNonexistantJob_ReturnsNotFound()
        {
            _interactor.Expect(i => i.JobExists("job1")).Return(false);
            _interactor.Expect(i => i.PauseJob("job1")).Repeat.Never();

            var response = SendJobActionRequest("job1", HttpApiConstants.JobAction.Pause);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            Assert.IsTrue(string.IsNullOrEmpty(response.Body.AsString()));
        }

        [Test]
        public void Put_Job_ResumeExistingJob_ReturnsNoResultAndOK()
        {
            _interactor.Expect(i => i.JobExists("job1")).Return(true);
            _interactor.Expect(i => i.ResumeJob("job1"));

            var response = SendJobActionRequest("job1", HttpApiConstants.JobAction.Resume);

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
            Assert.IsTrue(string.IsNullOrEmpty(response.Body.AsString()));
        }

        [Test]
        public void Put_Job_ResumeNonexistantJob_ReturnsNotFound()
        {
            _interactor.Expect(i => i.JobExists("job1")).Return(false);
            _interactor.Expect(i => i.ResumeJob("job1")).Repeat.Never();

            var response = SendJobActionRequest("job1", HttpApiConstants.JobAction.Resume);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            Assert.IsTrue(string.IsNullOrEmpty(response.Body.AsString()));
        }

        [Test]
        public void Put_Job_StartExistingJob_ReturnsNoResultAndOK()
        {
            _interactor.Expect(i => i.JobExists("job1")).Return(true);
            _interactor.Expect(i => i.StartJob("job1"));

            var response = SendJobActionRequest("job1", HttpApiConstants.JobAction.Start);

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
            Assert.IsTrue(string.IsNullOrEmpty(response.Body.AsString()));
        }

        [Test]
        public void Put_Job_StartNonexistantJob_ReturnsNotFound()
        {
            _interactor.Expect(i => i.JobExists("job1")).Return(false);
            _interactor.Expect(i => i.StartJob("job1")).Repeat.Never();

            var response = SendJobActionRequest("job1", HttpApiConstants.JobAction.Start);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            Assert.IsTrue(string.IsNullOrEmpty(response.Body.AsString()));
        }

        [Test]
        public void Put_Job_KillingExistingRunningJob_ReturnsNoResultAndOK()
        {
            _interactor.Expect(i => i.JobExists("job1")).Return(true);
            _interactor.Expect(i => i.KillJob("job1")).Return(true);

            var response = SendJobActionRequest("job1", HttpApiConstants.JobAction.Kill);

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
            Assert.IsTrue(string.IsNullOrEmpty(response.Body.AsString()));
        }

        [Test]
        public void Put_Job_KillingExistingNonRunningJob_ReturnsErrorMessageAndBadRequest()
        {
            _interactor.Expect(i => i.JobExists("job1")).Return(true);
            _interactor.Expect(i => i.KillJob("job1")).Return(false);

            var response = SendJobActionRequest("job1", HttpApiConstants.JobAction.Kill);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("Job 'job1' is not currently executing.", response.Body.AsString());
        }

        [Test]
        public void Put_Job_KillNonexistantJob_ReturnsNotFound()
        {
            _interactor.Expect(i => i.JobExists("job1")).Return(false);
            _interactor.Expect(i => i.KillJob("job1")).Repeat.Never();

            var response = SendJobActionRequest("job1", HttpApiConstants.JobAction.Kill);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            Assert.IsTrue(string.IsNullOrEmpty(response.Body.AsString()));
        }

        private BrowserResponse SendJobActionRequest(string jobName, string action)
        {
            var result = _browser.Put($"/scheduler/jobs/{jobName}", with =>
            {
                with.HttpRequest();
                with.FormValue(HttpApiConstants.FormFieldNames.ActionToTake, action);
            });
            return result;
        }
    }
}
