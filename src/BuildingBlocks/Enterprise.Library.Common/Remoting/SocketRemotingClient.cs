using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Library.Common.Remoting
{
    /// <summary>
    /// represent a socket remote client
    /// </summary>
    public class SocketRemotingClient
    {
        private readonly byte[] TimeoutMessage = Encoding.UTF8.GetBytes("Remoting request timeout.");

    }
}
