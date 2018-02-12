using Enterprise.Library.Common.Configurations;
using Enterprise.Library.Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Enterprise.Library.Common.ConsoleLogging
{
    /// <summary>
    /// configuration class Autofac extensions.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>Use console as the logger.
        /// </summary>
        /// <returns></returns>
        public static Configuration UseConsoleLogging(this Configuration configuration)
        {
            configuration.SetDefault<ILoggerFactory, ConsoleLoggerFactory>(new ConsoleLoggerFactory());
            return configuration;
        }
    }
}
