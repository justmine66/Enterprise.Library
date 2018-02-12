using Enterprise.Library.Common.Autofac;
using Enterprise.Library.Common.Components;
using Enterprise.Library.Common.Log4NetLogging;
using Enterprise.Library.Common.Logging;
using Enterprise.Library.Common.Performances;
using Enterprise.Library.Common.Remoting;
using System;
using EConfigration = Enterprise.Library.Common.Configurations.Configuration;

namespace RemotingPerformanceTest.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            EConfigration.Create()
                .UseAutofac()
                .RegisterCommonComponents()
                .RegisterUnhandledExceptionHandler()
                .UseLog4netLogging()
                .BuildContainer();

            new SocketRemotingServer()
                .RegisterRequestHandler(100, new RequestHanlder())
                .Start();

            Console.Read();
        }

        class RequestHanlder : IRequestHandler
        {
            readonly ILogger _logger;
            readonly string _performanceKey = "ServerSideReceiveMessage";
            readonly IPerformanceService _performanceService;
            readonly byte[] EmptyResponse = new byte[0];

            public RequestHanlder()
            {
                _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(this.GetType().FullName);
                _performanceService = ObjectContainer.Resolve<IPerformanceService>();
                var setting = new PerformanceServiceSetting()
                {
                    AutoLogging = false,
                    StatIntervalSeconds = 1,
                    PerformanceInfoHandler = x =>
                    {
                        _logger.InfoFormat("{0}, totalCount: {1}, throughput: {2}, averageThrughput: {3}, rt: {4:F3}ms, averageRT: {5:F3}ms", _performanceService.Name, x.TotalCount, x.Throughput, x.AverageThroughput, x.RT, x.AverageRT);
                    }
                };
                _performanceService.Initialize(_performanceKey, setting);
                _performanceService.Start();
            }

            public RemotingResponse HandleRequest(IRequestHandlerContext context, RemotingRequest request)
            {
                var currentDt = DateTime.Now;
                _performanceService.IncrementKeyCount(_performanceKey, currentDt.Subtract(request.CreatedTime).TotalMilliseconds);
                return new RemotingResponse()
                {
                    RequestCode = request.Code,
                    RequestHeader = request.Header,
                    RequestSequence = request.Sequence,
                    RequestTime = request.CreatedTime,
                    RequestType = request.Type,
                    ResponseTime = DateTime.Now,
                    ResponseCode = 10,
                    ResponseBody = EmptyResponse,
                    ResponseHeader = null
                };
            }
        }
    }
}
