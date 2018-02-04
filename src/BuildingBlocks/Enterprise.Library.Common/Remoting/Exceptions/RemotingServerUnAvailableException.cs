using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Enterprise.Library.Common.Remoting.Exceptions
{
    public class RemotingServerUnAvailableException : Exception
    {
        public RemotingServerUnAvailableException(EndPoint serverEndPoint)
            : base(string.Format("Remoting server is unavailable, server address: {0}", serverEndPoint))
        {
        }
    }
}
