using Enterprise.Library.Common.Socketing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Library.Common.Remoting
{
    public class SocketRequestHandlerContext : IRequestHandlerContext
    {
        public ITcpConnection Connection { get; private set; }
        public Action<RemotingResponse> SendRemotingResponse { get; private set; }

        public SocketRequestHandlerContext(ITcpConnection connection, Action<byte[]> sendReplyAction)
        {
            this.Connection = connection;
            this.SendRemotingResponse = remotingResponse =>
            {
                sendReplyAction(this.BuildRemotingServerMessage(remotingResponse));
            };
        }

        private byte[] BuildRemotingServerMessage(RemotingResponse remotingResponse)
        {
            byte[] remotingResponseMessage = RemotingUtils.BuildResponseMessage(remotingResponse);
            var remotingServerMessage = new RemotingServerMessage(
                RemotingServerMessageType.RemotingResponse,
                 100,
                 remotingResponseMessage);
            return RemotingUtils.BuildRemotingServerMessage(remotingServerMessage);
        }
    }
}
