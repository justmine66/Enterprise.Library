using Enterprise.Library.Common.Autofac;
using Enterprise.Library.Common.Components;
using Enterprise.Library.Common.ConsoleLogging;
using Enterprise.Library.Common.Logging;
using Enterprise.Library.Common.Remoting;
using System;
using System.Configuration;
using System.Net;
using System.Text;
using EConfiguration = Enterprise.Library.Common.Configurations.Configuration;

namespace ClientSideSocketTest
{
    class Program
    {
        static ILogger _logger;
        static SocketRemotingClient _client;

        static void Main(string[] args)
        {
            InitializeECommon();
            Console.Read();
        }

        static void InitializeECommon()
        {
            EConfiguration
                .Create()
                .UseAutofac()
                .RegisterCommonComponents()
                .UseConsoleLogging()
                .RegisterUnhandledExceptionHandler()
                .BuildContainer();

            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(typeof(Program).Name);
            var serverIP = ConfigurationManager.AppSettings["ServerAddress"];
            var serverAddress = string.IsNullOrEmpty(serverIP)
                                ? IPAddress.Loopback
                                : IPAddress.Parse(serverIP);
            _client = new SocketRemotingClient(new IPEndPoint(serverAddress, 5000)).Start();
            _client.RegisterRemotingServerMessageHandler(100, new RemotingServerMessageHandler());
        }

        class RemotingServerMessageHandler : IRemotingServerMessageHandler
        {
            public void HandleMessage(RemotingServerMessage message)
            {
                if (message.Code != 100)
                {
                    _logger.ErrorFormat("Invalid remoting server message: {0}", message);
                    return;
                }
                _logger.InfoFormat("Received server message: {0}", Encoding.UTF8.GetString(message.Body));
            }
        }
    }
}
