using Enterprise.Library.Common.Components;
using Enterprise.Library.Common.Logging;
using Enterprise.Library.Common.Socketing;
using Enterprise.Library.Common.Socketing.Buffering;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Enterprise.Library.Common.Remoting
{
    /// <summary>
    /// Represents a remote server-side socket.
    /// </summary>
    public class SocketRemotingServer
    {
        #region [ private fields and constructors ]

        private readonly ServerSocket _serverSocket;
        private readonly Dictionary<int, IRequestHandler> _requestHandlerDict;
        private readonly IBufferPool _receiveDataBufferPool;
        private readonly ILogger _logger;
        private readonly SocketSetting _setting;
        private bool _isShutteddown = false;

        public SocketRemotingServer()
            : this("serverSide", new IPEndPoint(IPAddress.Loopback, 5000))
        { }
        public SocketRemotingServer(SocketSetting setting)
            : this("serverSide", new IPEndPoint(IPAddress.Loopback, 5000), setting)
        { }
        public SocketRemotingServer(
            string name,
            IPEndPoint listeningEndPoint,
            SocketSetting setting = null)
        {
            _setting = setting ?? new SocketSetting();
            _receiveDataBufferPool = new BufferPool(_setting.ReceiveDataBufferSize, _setting.ReceiveDataBufferPoolSize);
            _serverSocket = new ServerSocket(listeningEndPoint, _setting, _receiveDataBufferPool, this.HandleRemotingRequest);
            _requestHandlerDict = new Dictionary<int, IRequestHandler>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(name ?? this.GetType().Name);
        }

        #endregion

        #region [ public properties and methods ]

        /// <summary>
        /// Represents the received data buffer pool of server-side socket.
        /// </summary>
        public IBufferPool BufferPool
        {
            get { return _receiveDataBufferPool; }
        }
        /// <summary>
        /// Represents the server-side socket instance.
        /// </summary>
        public ServerSocket ServerSocket
        {
            get { return _serverSocket; }
        }
        /// <summary>
        /// Adds an the instance of IConnectionEventListener to the server-side socket.
        /// </summary>
        /// <param name="listener"></param>
        /// <returns>the instance of server-side socket.</returns>
        public SocketRemotingServer RegisterConnectionEventListener(IConnectionEventListener listener)
        {
            _serverSocket.RegisterConnectionEventListener(listener);
            return this;
        }
        /// <summary>
        /// Starts a server-side socket.
        /// </summary>
        /// <returns>the instance of server-side socket.</returns>
        public SocketRemotingServer Start()
        {
            _isShutteddown = false;
            _serverSocket.Start();
            return this;
        }
        /// <summary>
        /// Shutdown a server-side socket and releases all resources.
        /// </summary>
        /// <returns>the instance of server-side socket.</returns>
        public SocketRemotingServer Shutdown()
        {
            _isShutteddown = true;
            _serverSocket.Shutdown();
            return this;
        }
        /// <summary>
        /// Adds an the instance of IRequestHandler to the server-side socket.
        /// </summary>
        /// <param name="requestCode">the indentifier of request.</param>
        /// <param name="requestHandler">the instance of IRequestHandler.</param>
        /// <returns>the instance of server-side socket.</returns>
        public SocketRemotingServer RegisterRequestHandler(int requestCode, IRequestHandler requestHandler)
        {
            _requestHandlerDict[requestCode] = requestHandler;
            return this;
        }
        /// <summary>
        /// Pushes remoting message to all connected client-side tcp connections.
        /// </summary>
        /// <param name="message">the remoting message.</param>
        public void PushMessageToAllConnections(RemotingServerMessage message)
        {
            byte[] data = RemotingUtils.BuildRemotingServerMessage(message);
            _serverSocket.PushMessageToAllConnections(data);
        }
        /// <summary>
        /// Pushes remoting message to a specified client-side tcp connection.
        /// </summary>
        /// <param name="connectionId">the identifier of connection.</param>
        /// <param name="message">the remoting message.</param>
        public void PushMessageToConnection(Guid connectionId, RemotingServerMessage message)
        {
            byte[] data = RemotingUtils.BuildRemotingServerMessage(message);
            _serverSocket.PushMessageToConnection(connectionId, data);
        }
        /// <summary>
        /// Returns all client-side tcp connections which are pended the server-side socket.
        /// </summary>
        /// <returns></returns>
        public IList<ITcpConnection> GetAllConnections()
        {
            return _serverSocket.GetAllConnections();
        }

        #endregion

        #region [ internal methods ]

        private void HandleRemotingRequest(
            ITcpConnection connection,
            byte[] message,
            Action<byte[]> sendReplyAction)
        {
            if (_isShutteddown) return;

            var remotingRequest = RemotingUtils.ParseRequest(message);
            var requestHandlerContext = new SocketRequestHandlerContext(connection, sendReplyAction);

            IRequestHandler requestHandler;
            if (!_requestHandlerDict.TryGetValue(remotingRequest.Code, out requestHandler))
            {
                var errorMessage = string.Format("No request handler found for remoting request:{0}", remotingRequest);
                this.HandleExeption(requestHandlerContext, remotingRequest, errorMessage);
                return;
            }

            try
            {
                RemotingResponse response = requestHandler.HandleRequest(requestHandlerContext, remotingRequest);
                if (remotingRequest.Type != RemotingRequestType.Oneway && response != null)
                {
                    requestHandlerContext.SendRemotingResponse(response);
                }
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format("Unknown exception raised when handling remoting request: {0}", remotingRequest);
                this.HandleExeption(requestHandlerContext, remotingRequest, errorMessage, ex);
            }
        }

        private void HandleExeption(
            IRequestHandlerContext context,
            RemotingRequest request,
            string errorMessage,
            Exception exception = null)
        {
            if (exception == null)
            { _logger.Error(errorMessage); }
            else
            { _logger.Error(errorMessage, exception); }

            if (request.Type != RemotingRequestType.Oneway)
            {
                context.SendRemotingResponse(new RemotingResponse(
                    request.Type,
                    request.Code,
                    request.Sequence,
                    request.CreatedTime,
                    -1,
                    Encoding.UTF8.GetBytes(errorMessage),
                    DateTime.Now,
                    request.Header,
                    null));
            }
        }

        #endregion
    }
}
