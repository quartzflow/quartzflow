using System;
using System.IO;
using NUnit.Framework;

namespace JobScheduler.Tests
{
    [TestFixture()]
    public class JobConfigFixture
    {
        [Test]
        public void CreateJobDefinitions_ForValidConfig_WillProvideDefinitions()
        {
            StringReader sr = TestHelper.GetFileContents("test-jobs.json");
            var definitions = JobConfig.CreateJobDefinitions(sr);
            Assert.AreEqual(3, definitions.Count);
        }

        [Test]
        public void CreateJobDefinitions_ForJobWithMissingGroup_WillThrowException()
        {
            StringReader sr = TestHelper.GetFileContents("test-jobs-missing-group.json");
            var ex = Assert.Throws<Exception>(() => JobConfig.CreateJobDefinitions(sr));
            Assert.AreEqual("Failed to create job defintions from config - one or more of the jobs has a blank or missing Group value", ex.Message);
        }

        [Test]
        public void CreateJobDefinitions_ForJobWithMissingName_WillThrowException()
        {
            StringReader sr = TestHelper.GetFileContents("test-jobs-missing-name.json");
            var ex = Assert.Throws<Exception>(() => JobConfig.CreateJobDefinitions(sr));
            Assert.AreEqual("Failed to create job defintions from config - one or more of the jobs has a blank or missing JobName value", ex.Message);
        }

        [Test]
        public void CreateJobDefinitions_ForJobsWithMissingOnCompletionDependencies_WillThrowException()
        {
            StringReader sr = TestHelper.GetFileContents("test-jobs-missing-oncompletion-job.json");
            var ex = Assert.Throws<Exception>(() => JobConfig.CreateJobDefinitions(sr));
            Assert.AreEqual("Failed to create job definitions from config - job 'Job2' has a dependency 'Job1' that does not exist", ex.Message);
        }

        [Test]
        public void CreateJobDefinitions_ForJobsWithMissingOnSuccessDependencies_WillThrowException()
        {
            StringReader sr = TestHelper.GetFileContents("test-jobs-missing-onsuccess-job.json");
            var ex = Assert.Throws<Exception>(() => JobConfig.CreateJobDefinitions(sr));
            Assert.AreEqual("Failed to create job definitions from config - job 'Job3' has a dependency 'Job2' that does not exist", ex.Message);
        }

        [Test]
        public void CreateJobDefinitions_ForJobsWithMissingOnFailureDependencies_WillThrowException()
        {
            StringReader sr = TestHelper.GetFileContents("test-jobs-missing-onfailure-job.json");
            var ex = Assert.Throws<Exception>(() => JobConfig.CreateJobDefinitions(sr));
            Assert.AreEqual("Failed to create job definitions from config - job 'Job3' has a dependency 'Job2' that does not exist", ex.Message);
        }        
    }
}
