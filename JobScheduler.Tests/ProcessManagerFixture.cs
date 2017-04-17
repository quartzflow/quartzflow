using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using NUnit.Framework;

namespace JobScheduler.Tests
{
    [TestFixture()]
    public class ProcessManagerFixture
    {
        [Test]
        public void KillProcess_ForExistingProcess_WillTerminate()
        {
            var process = Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestApp\TestApp.exe"), "success 5000");
            Assert.That(process.Id > 0);

            var processManager = new ProcessManager();
            processManager.KillProcess(process.Id);

            Thread.Sleep(1000);
            Assert.IsTrue(process.HasExited);
        }
    }
}
