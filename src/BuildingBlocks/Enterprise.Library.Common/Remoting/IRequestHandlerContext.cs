using Enterprise.Library.Common.Socketing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Library.Common.Remoting
{
    public class IRequestHandlerContext
    {
        ITcpConnection Connection { get; }
        Action<RemotingResponse> SendRemotingResponse { get; }
    }
}
