using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Enterprise.Library.Common.Logging;

namespace Enterprise.Library.Common.ConsoleLogging
{
    /// <summary>
    /// GdLogger based logger implementation.
    /// </summary>
    public class ConsoleLogger : ILogger
    {
        public bool IsDebugEnabled => true;

        public void Debug(object message)
        {
            ConsoleOutput.DebugFormat(message + string.Empty);
        }

        public void Debug(object message, Exception exception)
        {
            ConsoleOutput.DebugFormat("{0}, exception: {1}", message, exception.ToString());
        }

        public void DebugFormat(string format, params object[] args)
        {
            ConsoleOutput.DebugFormat(format, args);
        }

        public void Info(object message)
        {
            ConsoleOutput.InfoFormat(message + string.Empty);
        }

        public void Info(object message, Exception exception)
        {
            ConsoleOutput.InfoFormat("{0}, exception: {1}", message, exception.ToString());
        }

        public void InfoFormat(string format, params object[] args)
        {
            ConsoleOutput.InfoFormat(format, args);
        }

        public void Warn(object message)
        {
            ConsoleOutput.WarnFormat(message + string.Empty);
        }

        public void Warn(object message, Exception exception)
        {
            ConsoleOutput.WarnFormat("{0}, exception: {1}", message, exception.ToString());
        }

        public void WarnFormat(string format, params object[] args)
        {
            ConsoleOutput.WarnFormat(format, args);
        }

        public void Error(object message)
        {
            ConsoleOutput.ErrorFormat(message + string.Empty);
        }

        public void Error(object message, Exception exception)
        {
            ConsoleOutput.ErrorFormat("{0}, exception: {1}", message, exception.ToString());
        }

        public void ErrorFormat(string format, params object[] args)
        {
            ConsoleOutput.ErrorFormat(format, args);
        }

        public void Fatal(object message)
        {
            ConsoleOutput.FatalFormat(message + string.Empty);
        }

        public void Fatal(object message, Exception exception)
        {
            ConsoleOutput.FatalFormat("{0}, exception: {1}", message, exception.ToString());
        }

        public void FatalFormat(string format, params object[] args)
        {
            ConsoleOutput.FatalFormat(format, args);
        }
    }
}
