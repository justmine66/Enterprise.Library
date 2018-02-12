using Enterprise.Library.Common.Configurations;
using Enterprise.Library.Common.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Library.Common.Log4NetLogging
{
    public static class ConfigurationExtensions
    {
        /// <summary>Use log4net as the logger.
        /// </summary>
        public static Configuration UseLog4netLogging(this Configuration configuration)
        {
            return UseLog4netLogging(configuration, "log4net.config");
        }

        /// <summary>Use log4net as the logger.
        /// </summary>
        /// <param name="configFile">The full qualified name of the config file, or the relative file name.Do not end the path with the directory separator character.</param>
        public static Configuration UseLog4netLogging(this Configuration configuration, string configFile, string loggerRepository = "NetStandardRepository")
        {
            configuration.SetDefault<ILoggerFactory, Log4NetLoggerFactory>(new Log4NetLoggerFactory(configFile));
            return configuration;
        }
    }
}
