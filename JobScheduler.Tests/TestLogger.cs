using System.Collections.Generic;
using Common.Logging;
using Common.Logging.Simple;
using NUnit.Framework;

namespace JobScheduler.Tests
{
    public class TestLogger
    {
        private readonly CapturingLoggerFactoryAdapter _loggingAdapter;

        public CapturingLoggerFactoryAdapter LoggingAdapter => _loggingAdapter;

        public TestLogger()
        {
            _loggingAdapter = new CapturingLoggerFactoryAdapter();
            _loggingAdapter.Clear();
        }

        public IList<CapturingLoggerEvent> GetLoggedMessages()
        {
            return _loggingAdapter.LoggerEvents;
        }

        public void AssertInfoMessagesLogged(params string[] messages)
        {
            var loggedMessages = GetLoggedMessages();
            Assert.AreEqual(messages.Length, loggedMessages.Count);

            int i = 0;
            foreach (var loggedMessage in loggedMessages)
            {
                Assert.AreEqual(LogLevel.Info, loggedMessage.Level);
                Assert.AreEqual(messages[i++], loggedMessage.RenderedMessage);
            }
        }

        public void AssertNoMessagesLogged()
        {
            var messages = GetLoggedMessages();
            Assert.AreEqual(0, messages.Count);
        }

        public void Clear()
        {
            _loggingAdapter.Clear();
        }
    }
}