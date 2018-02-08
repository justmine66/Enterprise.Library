using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace InfrastructureTest.Socketing
{
    public class AsyncUserToken
    {
        public IPAddress ClientIPAddress { get; set; }
        public EndPoint RemoteIPAddress { get; set; }
        public Socket Socket { get; set; }
        public DateTime ConnectTime { get; set; }
        public UserInfoModel UserInfo { get; set; }
        public List<byte> Buffer { get; set; }

        public AsyncUserToken()
        {
            this.Buffer = new List<byte>();
        }
    }
}
