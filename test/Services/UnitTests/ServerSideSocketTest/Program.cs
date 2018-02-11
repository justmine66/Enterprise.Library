using Enterprise.Library.Common.Autofac;
using Enterprise.Library.Common.Components;
using Enterprise.Library.Common.ConsoleLogging;
using Enterprise.Library.Common.Logging;
using Enterprise.Library.Common.Remoting;
using Enterprise.Library.Common.Socketing;
using System;
using System.Configuration;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EConfiguration = Enterprise.Library.Common.Configurations.Configuration;

namespace ServerSideSocketTest
{
    class Program
    {
        static ILogger _logger;
        static SocketRemotingServer _remotingServer;

        static void Main(string[] args)
        {
            EConfiguration
                .Create()
                .UseAutofac()
                .RegisterCommonComponents()
                .UseConsoleLogging()
                .RegisterUnhandledExceptionHandler()
                .BuildContainer();

            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(typeof(Program).Name);
            _remotingServer = new SocketRemotingServer().Start();
            PushTestMessageToAllClients();
            Console.Read();
        }

        static void PushTestMessageToAllClients()
        {
            var messageCount = int.Parse(ConfigurationManager.AppSettings["MessageCount"]);

            Task.Factory.StartNew(() =>
            {
                for (var i = 1; i <= messageCount; i++)
                {
                    try
                    {
                        var remotingServerMessage = new RemotingServerMessage(RemotingServerMessageType.ServerMessage, 100, Encoding.UTF8.GetBytes("message:" + i));
                        _remotingServer.PushMessageToAllConnections(remotingServerMessage);
                        _logger.InfoFormat("Pushed server message: {0}", "message:" + i);
                        Thread.Sleep(1000);
                    }
                    catch (Exception ex)
                    {
                        _logger.ErrorFormat("PushMessageToAllConnections failed, errorMsg: {0}", ex.Message);
                        Thread.Sleep(1000);
                    }
                }
            });
        }
    }
}
