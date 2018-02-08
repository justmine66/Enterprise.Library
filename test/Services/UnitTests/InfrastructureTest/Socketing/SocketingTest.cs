using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace InfrastructureTest.Socketing
{
    public class SocketingTest
    {
        public static void Test()
        {
            var _socket = new SocketManager(200, 1024);
            _socket.Init();
            _socket.Start(new IPEndPoint(IPAddress.Any, 13909));

            Console.Read();
        }
    }
}
