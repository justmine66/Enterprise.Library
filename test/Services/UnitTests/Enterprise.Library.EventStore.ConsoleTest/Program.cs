using Enterprise.Library.Common.Components;
using Enterprise.Library.Common.Performances;
using Enterprise.Library.Common.Utilities;
using Enterprise.Library.Common.Autofac;
using Enterprise.Library.Common.Log4NetLogging;
using System;
using System.IO;
using ECommonCofig = Enterprise.Library.Common.Configurations.Configuration;

namespace Enterprise.Library.EventStore.ConsoleTest
{
    class Program
    {
        static string _performanceKey = "ApendEvent";
        static IPerformanceService _performanceService;

        static void Main(string[] args)
        {
            ECommonCofig.Create()
                .UseAutofac()
                .RegisterCommonComponents()
                .UseLog4net()
                .RegisterUnhandledExceptionHandler()
                .BuildContainer();

            _performanceService = ObjectContainer.Resolve<IPerformanceService>();
            _performanceService.Initialize(_performanceKey);
            _performanceService.Start();

            AppendEventTest();

            Console.Read();
        }

        static void AppendEventTest()
        {
            var eventStore = new DefaultEventStore();
            eventStore.Load();
            eventStore.Start();

            var eventStream = new EventStream()
            {
                SourceId = ObjectId.GenerateNewStringId(),
                CommandId = ObjectId.GenerateNewStringId(),
                Name = "Note",
                Events = "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890",
                Items = string.Empty
            };
            var totalEventCount = 5000 * 10000;

            for (int i = 0; i < totalEventCount; i++)
            {
                eventStream.Version = i;
                eventStream.Timestamp = DateTime.Now;
                eventStore.AppendStream(eventStream);
                _performanceService.IncrementKeyCount(_performanceKey, DateTime.Now.Subtract(eventStream.Timestamp).TotalMilliseconds);
            }

            eventStore.Shutdown();
        }
    }
}
