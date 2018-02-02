using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Library.Common.Remoting
{
    public interface IRemotingServerMessageHandler
    {
        void HandleMessage(RemotingServerMessage message);
    }
}
