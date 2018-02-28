using Enterprise.Library.Common.Components;
using Enterprise.Library.Common.IO;
using Enterprise.Library.Common.Logging;
using Enterprise.Library.Common.Performances;
using Enterprise.Library.Common.Scheduling;
using Enterprise.Library.Common.Serializing;
using Enterprise.Library.Common.Socketing.Framing;
using System;

namespace Enterprise.Library.Common.Configurations
{
    public class Configuration
    {
        private Configuration() { }
        public static Configuration Instance { get; private set; }
        public static Configuration Create()
        {
            Instance = new Configuration();
            return Instance;
        }

        public Configuration SetDefault<TService, TImplementer>(string serviceName = null, LifeStyle life = LifeStyle.Singleton)
            where TService : class
            where TImplementer : class, TService
        {
            ObjectContainer.Register<TService, TImplementer>(serviceName, life);
            return this;
        }

        public Configuration SetDefault<TService, TImplementer>(TImplementer instance, string serviceName = null)
            where TService : class
            where TImplementer : class, TService
        {
            ObjectContainer.RegisterInstance<TService, TImplementer>(instance, serviceName);
            return this;
        }

        public Configuration RegisterCommonComponents()
        {
            this.SetDefault<ILoggerFactory, EmptyLoggerFactory>();
            this.SetDefault<IBinarySerializer, DefaultBinarySerializer>();
            this.SetDefault<IJsonSerializer, NotImplementedJsonSerializer>();
            this.SetDefault<IScheduleService, ScheduleService>();
            this.SetDefault<IMessageFramer, LengthPrefixMessageFramer>();
            this.SetDefault<IOHelper, IOHelper>();
            this.SetDefault<IPerformanceService, DefaultPerformanceService>();

            return this;
        }

        public Configuration RegisterUnhandledExceptionHandler()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
                logger.ErrorFormat("Unhandled exception: {0}", args.ExceptionObject);
            };

            return this;
        }

        public Configuration BuildContainer()
        {
            ObjectContainer.Build();
            return this;
        }
    }
}
