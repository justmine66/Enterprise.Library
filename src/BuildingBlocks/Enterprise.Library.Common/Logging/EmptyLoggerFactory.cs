using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Library.Common.Logging
{
    /// <summary>
    /// An empty implement of ILoggerFactory
    /// </summary>
    public class EmptyLoggerFactory : ILoggerFactory
    {
        static readonly EmptyLogger logger = new EmptyLogger();

        /// <summary>
        /// Create an empty logger instance by name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ILogger Create(string name)
        {
            return logger;
        }

        /// <summary>
        /// Create an empty logger instance by type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ILogger Create(Type type)
        {
            return logger;
        }
    }
}
