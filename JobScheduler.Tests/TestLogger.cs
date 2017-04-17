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

        public void AssertInfoMessageLogged(string message)
        {
            var messages = GetLoggedMessages();
            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual(LogLevel.Info, messages[0].Level);
            Assert.AreEqual(message, messages[0].RenderedMessage);
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