using Enterprise.Library.Common.Autofac;
using Enterprise.Library.Common.Components;
using Enterprise.Library.Common.Log4NetLogging;
using Enterprise.Library.Common.Logging;
using Enterprise.Library.Common.Performances;
using Enterprise.Library.Common.Remoting;
using System;
using System.Configuration;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ECommonConfig = Enterprise.Library.Common.Configurations.Configuration;

namespace RemotingPerformanceTest.Client
{
    class Program
    {
        static string _performanceKey = "ClientSideSendMessage";
        static string _mode;
        static int _messageCount;
        static byte[] _message;
        static ILogger _logger;
        static IPerformanceService _performanceService;
        static SocketRemotingClient _client;

        static void Main(string[] args)
        {
            InitializeECommon();
            StartSendMessageTest();

            Console.Read();
        }

        static void InitializeECommon()
        {
            _message = new byte[int.Parse(ConfigurationManager.AppSettings["MessageSize"])];
            _mode = ConfigurationManager.AppSettings["Mode"] ?? "Oneway";
            _messageCount = int.Parse(ConfigurationManager.AppSettings["MessageCount"]);

            var logContextText = "mode: " + _mode;

            ECommonConfig
                .Create()
                .UseAutofac()
                .RegisterCommonComponents()
                .RegisterUnhandledExceptionHandler()
                .UseLog4netLogging()
                .BuildContainer();

            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(typeof(Program).Name);
            _performanceService = ObjectContainer.Resolve<IPerformanceService>();
            var setting = new PerformanceServiceSetting
            {
                AutoLogging = false,
                StatIntervalSeconds = 1,
                PerformanceInfoHandler = x =>
                {
                    _logger.InfoFormat("{0}, {1}, totalCount: {2}, throughput: {3}, averageThrughput: {4}, rt: {5:F3}ms, averageRT: {6:F3}ms", _performanceService.Name, logContextText, x.TotalCount, x.Throughput, x.AverageThroughput, x.RT, x.AverageRT);
                }
            };
            _performanceService.Initialize(_performanceKey, setting);
            _performanceService.Start();
        }

        static void StartSendMessageTest()
        {
            var serverIP = ConfigurationManager.AppSettings["ServerAddress"];
            IPAddress serverAddress = string.IsNullOrEmpty(serverIP) ? IPAddress.Loopback : IPAddress.Parse(serverIP);
            var sendAction = default(Action);

            _client = new SocketRemotingClient(serverAddress, 5000).Start();

            switch (_mode)
            {
                case "Oneway"://QPS
                    sendAction = () =>
                    {
                        _client.InvokeOneway(new RemotingRequest(100, _message));
                    };
                    break;
                case "Sync"://TPS
                    sendAction = () =>
                    {
                        var request = new RemotingRequest(100, _message);
                        RemotingResponse response = _client.InvokeSync(request, 5000);
                        if (response.ResponseCode != 10)
                        {
                            _logger.Error(Encoding.UTF8.GetString(response.ResponseBody));
                            return;
                        }
                        _performanceService.IncrementKeyCount(_mode, DateTime.Now.Subtract(response.RequestTime).TotalMilliseconds);
                    };
                    break;
                case "Async"://TPS
                    sendAction = () =>
                    {
                        var request = new RemotingRequest(100, _message);
                        _client.InvokeAsync(request, 5000).ContinueWith(task =>
                        {
                            if (task.Exception != null)
                            {
                                _logger.Error(task.Exception);
                            }

                            RemotingResponse response = task.Result;
                            if (response.ResponseCode != 10)
                            {
                                _logger.Error(Encoding.UTF8.GetString(response.ResponseBody));
                                return;
                            }
                            _performanceService.IncrementKeyCount(_mode, DateTime.Now.Subtract(response.RequestTime).TotalMilliseconds);
                        });
                    };
                    break;
                case "Callback"://TPS
                    _client.RegisterResponseHandler(100, new ResponseHandler(_performanceService, _mode));
                    sendAction = () => _client.InvokeWaitCallbask(new RemotingRequest(103, _message));
                    break;
            }

            Task.Factory.StartNew(() =>
            {
                for (var i = 0; i < 1; i++)
                {
                    try
                    {
                        sendAction();
                    }
                    catch (Exception ex)
                    {
                        _logger.ErrorFormat("Send remotingRequest failed, errorMsg:{0}", ex.Message);
                        Thread.Sleep(3000);
                    }
                }
            });
        }

        class ResponseHandler : IResponseHandler
        {
            IPerformanceService _performanceService;
            string _performanceKey;

            public ResponseHandler(IPerformanceService performanceService, string performanceKey)
            {
                _performanceService = performanceService;
                _performanceKey = performanceKey;
            }

            public void HandleResponse(RemotingResponse remotingResponse)
            {
                if (remotingResponse.ResponseCode != 10)
                {
                    _logger.Error(Encoding.UTF8.GetString(remotingResponse.ResponseBody));
                    return;
                }
                _performanceService.IncrementKeyCount(_performanceKey, (DateTime.Now - remotingResponse.RequestTime).TotalMilliseconds);
            }
        }
    }
}
