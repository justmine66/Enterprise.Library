using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Enterprise.Library.Common.Socketing
{
    /// <summary>
    /// represents a connection event listener
    /// </summary>
    public interface IConnectionEventListener
    {
        void OnConnectionAccepted(ITcpConnection connection);
        void OnConnectionEstablished(ITcpConnection connection);
        void OnConnectionFailed(EndPoint remotingEndPoint, SocketError socketError);
        void OnConnectionClosed(ITcpConnection connection, SocketError socketError);
    }
}
