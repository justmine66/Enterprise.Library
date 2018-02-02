using Enterprise.Library.Common.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Enterprise.Library.Common.ConsoleLogging
{
    /// <summary>
    /// An gd-logger implementation of ILoggerFactory.
    /// </summary>
    public class ConsoleLoggerFactory : ILoggerFactory
    {
        private static readonly ConsoleLogger Logger = new ConsoleLogger();

        public ILogger Create(string name)
        {
            return Logger;
        }

        public ILogger Create(Type type)
        {
            return Logger;
        }
    }
}