using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Enterprise.Library.Common.Remoting.Exceptions
{
    public class RemotingTimeoutException : Exception
    {
        public RemotingTimeoutException(EndPoint serverEndPoint, RemotingRequest request, int timeoutMillis)
            : base(string.Format("Wait response for server[{0}] timeout, request:{1}, timeountMillis:{2}ms", serverEndPoint, request, timeoutMillis))
        {

        }
    }
}
