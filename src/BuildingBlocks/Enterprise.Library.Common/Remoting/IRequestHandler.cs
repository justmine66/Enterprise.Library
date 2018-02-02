using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Library.Common.Remoting
{
    public interface IRequestHandler
    {
        RemotingResponse HandleRequest(IRequestHandlerContext context, RemotingRequest remotingRequest);
    }
}
