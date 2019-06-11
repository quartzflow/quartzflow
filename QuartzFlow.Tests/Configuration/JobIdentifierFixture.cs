using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace QuartzFlow.Tests
{
    [TestFixture()]
    public class JobIdentifierFixture
    {
        [Test]
        public void QuartzId_IsConstructedCorrectly()
        {
            var jobId1 = new JobIdentifier();
            jobId1.Group = "SOD";
            jobId1.JobName = "VIPJob";

            Assert.AreEqual("SOD.VIPJob", jobId1.QuartzId);
        }

        [Test]
        public void CanSerializeAndDeserializeCorrectly()
        {
            var jobId1 = new JobIdentifier();
            jobId1.Group = "SOD";
            jobId1.JobName = "VIPJob";

            var serializedJobId = JsonConvert.SerializeObject(jobId1);

            var deserializedJobId = JsonConvert.DeserializeObject<JobIdentifier>(serializedJobId);

            Assert.AreEqual(jobId1, deserializedJobId, "JobIdentifers are not equivalent");
        }
    }
}
