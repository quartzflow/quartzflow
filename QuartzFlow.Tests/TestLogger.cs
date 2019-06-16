using System;
using System.Collections.Generic;
using NLog.Targets;
using NUnit.Framework;

namespace QuartzFlow.Tests
{
    public class TestLogger
    {
        private readonly MemoryTarget _loggingSink;

        public MemoryTarget LoggingSink => _loggingSink;

        public TestLogger()
        {
            _loggingSink = new MemoryTarget();
        }

        public IList<string> GetLoggedMessages()
        {
            return _loggingSink.Logs;
        }

        public void AssertInfoMessagesLogged(params string[] messages)
        {
            var loggedMessages = GetLoggedMessages();
            Assert.AreEqual(messages.Length, loggedMessages.Count);

            int i = 0;
            foreach (var loggedMessage in loggedMessages)
            {
                //Assert.AreEqual(LogLevel.Info, loggedMessage.Level);
                Assert.AreEqual(messages[i++], loggedMessage);
            }
        }

        public void AssertNoMessagesLogged()
        {
            var messages = GetLoggedMessages();
            Assert.AreEqual(0, messages.Count);
        }

        public void Clear()
        {
            _loggingSink.Logs.Clear();
        }
    }
}