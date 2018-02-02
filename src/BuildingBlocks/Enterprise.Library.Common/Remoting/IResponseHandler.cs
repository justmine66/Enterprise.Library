using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Library.Common.Remoting
{
    public interface IResponseHandler
    {
        void HandleResponse(RemotingResponse remotingResponse);
    }
}
