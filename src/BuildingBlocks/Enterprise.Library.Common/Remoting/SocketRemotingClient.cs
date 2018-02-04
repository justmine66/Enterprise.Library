using Enterprise.Library.Common.Components;
using Enterprise.Library.Common.Extensions;
using Enterprise.Library.Common.Logging;
using Enterprise.Library.Common.Remoting.Exceptions;
using Enterprise.Library.Common.Scheduling;
using Enterprise.Library.Common.Socketing;
using Enterprise.Library.Common.Socketing.Buffering;
using Enterprise.Library.Common.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Enterprise.Library.Common.Remoting
{
    /// <summary>
    /// Represents a remote client-side socket.
    /// </summary>
    public class SocketRemotingClient
    {
        #region [ private fields and constructors ]

        readonly byte[] TimeoutMessage = Encoding.UTF8.GetBytes("Remoting request timeout.");
        readonly Dictionary<int, IResponseHandler> _responseHandlerDict;
        readonly Dictionary<int, IRemotingServerMessageHandler> _remotingServerMessageHandlerDict;
        readonly IList<IConnectionEventListener> _connectionEventListeners;
        readonly ConcurrentDictionary<long, ResponseFuture> _responseFutureDict;
        readonly BlockingCollection<byte[]> _replyMessageQueue;
        readonly IScheduleService _scheduleService;
        readonly IBufferPool _receivedDataBufferPool;
        readonly ILogger _logger;
        readonly SocketSetting _setting;

        EndPoint _serverEndPoint;
        EndPoint _localEndPoint;
        ClientSocket _clientSocket;
        int _reconnecting = 0;
        bool _shutteddown = false;
        bool _started = false;

        public SocketRemotingClient(EndPoint serverEndPoint, SocketSetting setting = null, EndPoint localEndPoint = null)
        {
            Ensure.NotNull(serverEndPoint, "serverEndPoint");

            _serverEndPoint = serverEndPoint;
            _localEndPoint = localEndPoint;
            _setting = setting ?? new SocketSetting();
            _receivedDataBufferPool = new BufferPool(_setting.ReceiveDataBufferSize, _setting.ReceiveDataBufferPoolSize);
            _clientSocket = new ClientSocket(_serverEndPoint, _localEndPoint, _setting, _receivedDataBufferPool, HandleServerMessage);
            _responseFutureDict = new ConcurrentDictionary<long, ResponseFuture>();
            _replyMessageQueue = new BlockingCollection<byte[]>(new ConcurrentQueue<byte[]>());
            _responseHandlerDict = new Dictionary<int, IResponseHandler>();
            _remotingServerMessageHandlerDict = new Dictionary<int, IRemotingServerMessageHandler>();
            _connectionEventListeners = new List<IConnectionEventListener>();
            _scheduleService = ObjectContainer.Resolve<IScheduleService>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);

            RegisterConnectionEventListener(new ConnectionEventListener(this));
        }

        #endregion

        #region [ public properties and methods ]

        /// <summary>
        /// Get a value indicates whether a client-side socket is connected.
        /// </summary>
        public bool IsConnected
        {
            get { return _clientSocket != null && _clientSocket.IsConnected; }
        }
        /// <summary>
        /// Gets a local end point.
        /// </summary>
        public EndPoint LocalEndPoint
        {
            get { return _localEndPoint; }
        }
        /// <summary>
        /// Gets a server end point.
        /// </summary>
        public EndPoint ServerEndPoint
        {
            get { return _serverEndPoint; }
        }
        /// <summary>
        /// Gets a client-side socket.
        /// </summary>
        public ClientSocket ClientSocket
        {
            get { return _clientSocket; }
        }
        /// <summary>
        /// Gets a received data buffer pool.
        /// </summary>
        public IBufferPool BufferPool
        {
            get { return _receivedDataBufferPool; }
        }
        /// <summary>
        /// Add an the implementer of IConnectionEventListener to the client-side socket.
        /// </summary>
        /// <param name="listener">the implementer of IConnectionEventListener.</param>
        /// <returns>the client-side socket.</returns>
        public SocketRemotingClient RegisterConnectionEventListener(IConnectionEventListener listener)
        {
            _connectionEventListeners.Add(listener);
            _clientSocket.RegisterConnectionEventListener(listener);
            return this;
        }
        /// <summary>
        /// Add an the implementer of IResponseHandler to the client-side socket.
        /// </summary>
        /// <param name="listener">the implementer of IResponseHandler.</param>
        /// <returns>the client-side socket.</returns>
        public SocketRemotingClient RegisterResponseHandler(int requestCode, IResponseHandler responseHandler)
        {
            _responseHandlerDict[requestCode] = responseHandler;
            return this;
        }
        /// <summary>
        /// Add an the implementer of IRemotingServerMessageHandler to the client-side socket.
        /// </summary>
        /// <param name="listener">the implementer of IRemotingServerMessageHandler.</param>
        /// <returns>the client-side socket.</returns>
        public SocketRemotingClient RegisterRemotingServerMessageHandler(int messageCode, IRemotingServerMessageHandler messageHandler)
        {
            _remotingServerMessageHandlerDict[messageCode] = messageHandler;
            return this;
        }
        /// <summary>
        /// Starts a client-side socket.
        /// </summary>
        /// <returns></returns>
        public SocketRemotingClient Start()
        {
            if (_started) return this;

            this.StartClientSocket();
            this.StartScanTimeoutRequestTask();
            _shutteddown = false;
            _started = false;
            return this;
        }
        /// <summary>
        /// shutdown a client-side socket.
        /// </summary>
        /// <returns></returns>
        public void Shutdown()
        {
            _shutteddown = true;
            this.StopReconnectServerTask();
            this.StopScanTimeoutRequestTask();
            this.ShutdownClientSocket();
        }
        /// <summary>
        /// Synchronous sends request to server.
        /// </summary>
        /// <param name="request">the instance of RemotingRequest..</param>
        /// <param name="timeoutMillis">the timeout in milliseconds.</param>
        /// <returns>a instance of RemotingResponse.</returns>
        public RemotingResponse InvokeSync(RemotingRequest request, int timeoutMillis)
        {
            var task = this.InvokeAsync(request, timeoutMillis);
            RemotingResponse response = task.WaitResult(timeoutMillis);

            if (response == null)
            {
                if (!task.IsCompleted)
                {
                    throw new RemotingTimeoutException(_serverEndPoint, request, timeoutMillis);
                }
                else if (task.IsFaulted)
                {
                    throw new RemotingRequestException(_serverEndPoint, request, task.Exception);
                }
                else
                {
                    throw new RemotingRequestException(_serverEndPoint, request, "Remoting response is null due to unkown exception.");
                }
            }

            return response;
        }
        /// <summary>
        /// Asynchronous sends request to server.
        /// </summary>
        /// <param name="request">the instance of RemotingRequest.</param>
        /// <param name="timeoutMillis">the timeout in milliseconds.</param>
        /// <returns>a task represents instance of RemotingResponse.</returns>
        public Task<RemotingResponse> InvokeAsync(RemotingRequest request, int timeoutMillis)
        {
            this.EnsureClientStatus();

            request.Type = RemotingRequestType.Async;
            var taskCompletionSource = new TaskCompletionSource<RemotingResponse>();
            var responseFuture = new ResponseFuture(request, timeoutMillis, taskCompletionSource);

            if (!_responseFutureDict.TryAdd(request.Sequence, responseFuture))
            {
                throw new ResponseFutureAddFailedException(request.Sequence);
            }

            _clientSocket.QueueMessage(RemotingUtils.BuildRequestMessage(request));

            return taskCompletionSource.Task;
        }
        /// <summary>
        /// Sends request to server and wait callback.
        /// </summary>
        /// <param name="request">the instance of RemotingRequest.</param>
        public void InvokeWaitCallbask(RemotingRequest request)
        {
            this.EnsureClientStatus();

            request.Type = RemotingRequestType.Callback;
            _clientSocket.QueueMessage(RemotingUtils.BuildRequestMessage(request));
        }
        /// <summary>
        /// One-wayly sends request to server.
        /// </summary>
        /// <param name="request">the instance of RemotingRequest.</param>
        public void InvokeOneway(RemotingRequest request)
        {
            EnsureClientStatus();

            request.Type = RemotingRequestType.Oneway;
            _clientSocket.QueueMessage(RemotingUtils.BuildRequestMessage(request));
        }
        #endregion

        #region [ internal methods ]

        private void HandleServerMessage(ITcpConnection connection, byte[] message)
        {
            if (message == null) return;

            RemotingServerMessage remotingServerMessage = RemotingUtils.ParseRemotingServerMessage(message);
            switch (remotingServerMessage.Type)
            {
                case RemotingServerMessageType.RemotingResponse:
                    this.HandleResponseMessage(connection, message);
                    break;
                case RemotingServerMessageType.ServerMessage:
                    this.HandleServerPushMessage(connection, remotingServerMessage);
                    break;
            }
        }
        private void HandleResponseMessage(ITcpConnection connection, byte[] message)
        {
            if (message == null) return;

            RemotingResponse remotingResponse = RemotingUtils.ParseResponse(message);
            switch (remotingResponse.RequestType)
            {
                case RemotingRequestType.Callback:
                    IResponseHandler responseHandler;
                    if (_responseHandlerDict.TryGetValue(remotingResponse.RequestCode, out responseHandler))
                    {
                        responseHandler.HandleResponse(remotingResponse);
                    }
                    else
                    {
                        _logger.ErrorFormat("No response handler found for remoting response:{0}", remotingResponse);
                    }
                    break;
                case RemotingRequestType.Async:
                    ResponseFuture responseFuture;
                    if (_responseFutureDict.TryGetValue(remotingResponse.RequestSequence, out responseFuture))
                    {
                        if (responseFuture.SetResponse(remotingResponse))
                        {
                            if (_logger.IsDebugEnabled)
                            {
                                _logger.DebugFormat("Remoting response back, request code:{0}, requect sequence:{1}, time spent:{2}", responseFuture.Request.Code, responseFuture.Request.Sequence, (DateTime.Now - responseFuture.BeginTime).TotalMilliseconds);
                            }
                        }
                        else
                        {
                            _logger.ErrorFormat("Set remoting response failed, response:" + remotingResponse);
                        }
                    }
                    break;
            }
        }
        private void HandleServerPushMessage(ITcpConnection connection, RemotingServerMessage message)
        {
            IRemotingServerMessageHandler messageHandler;
            if (_remotingServerMessageHandlerDict.TryGetValue(message.Code, out messageHandler))
            {
                messageHandler.HandleMessage(message);
            }
            else
            {
                _logger.ErrorFormat("No handler found for remoting server message: {0}", message);
            }
        }
        private bool TryEnterReconnecting()
        {
            return Interlocked.CompareExchange(ref _reconnecting, 1, 0) == 0;
        }
        private void ExitReconnecting()
        {
            Interlocked.Exchange(ref _reconnecting, 0);
        }
        private void StartClientSocket()
        {
            _clientSocket.Start();
        }
        private void ShutdownClientSocket()
        {
            _clientSocket.Shutdown();
        }
        private void ScanTimeoutRequest()
        {
            var timeoutKeyList = new List<long>();
            foreach (KeyValuePair<long, ResponseFuture> entry in _responseFutureDict)
            {
                if (entry.Value.IsTimeout())
                {
                    timeoutKeyList.Add(entry.Key);
                }
            }
            foreach (long key in timeoutKeyList)
            {
                ResponseFuture responseFuture;
                if (_responseFutureDict.TryRemove(key, out responseFuture))
                {
                    RemotingRequest request = responseFuture.Request;
                    responseFuture.SetResponse(new RemotingResponse(
                        request.Type,
                        request.Code,
                        request.Sequence,
                        request.CreatedTime,
                        0,
                        TimeoutMessage,
                        DateTime.Now,
                        request.Header,
                        null));
                    if (_logger.IsDebugEnabled)
                    {
                        _logger.DebugFormat("Removed timeout request: {0}", responseFuture.Request);
                    }
                }
            }
        }
        private void StartScanTimeoutRequestTask()
        {
            _scheduleService.StartTask(string.Format("{0}.ScanTimeoutRequest", this.GetType().Name), ScanTimeoutRequest, 1000, _setting.ScanTimeoutRequestInterval);
        }
        private void StopScanTimeoutRequestTask()
        {
            _scheduleService.StopTask(string.Format("{0}.ScanTimeoutRequest", this.GetType().Name));
        }
        private void ReconnectServer()
        {
            _logger.InfoFormat("Try to reconnect server, address: {0}", _serverEndPoint);

            if (_clientSocket.IsConnected) return;
            if (!TryEnterReconnecting()) return;

            try
            {
                _clientSocket.Shutdown();
                _clientSocket = new ClientSocket(_serverEndPoint, _localEndPoint, _setting, _receivedDataBufferPool, this.HandleServerMessage);
                foreach (IConnectionEventListener listener in _connectionEventListeners)
                {
                    _clientSocket.RegisterConnectionEventListener(listener);
                }
                _clientSocket.Start();
            }
            catch (Exception exc)
            {
                _logger.Error("Reconnect to server error.", exc);
                this.ExitReconnecting();
            }
        }
        private void StartReconnectServerTask()
        {
            _scheduleService.StartTask(string.Format("{0}.ReconnectServer", this.GetType().Name), ReconnectServer, 1000, _setting.ReconnectToServerInterval);
        }
        private void StopReconnectServerTask()
        {
            _scheduleService.StopTask(string.Format("{0}.ReconnectServer", this.GetType().FullName));
        }
        private void EnsureClientStatus()
        {
            if (_clientSocket == null || !_clientSocket.IsConnected)
            {
                throw new RemotingServerUnAvailableException(_serverEndPoint);
            }
        }
        private void SetLocalEndPoint(EndPoint localEndPoint)
        {
            _localEndPoint = localEndPoint;
        }

        class ConnectionEventListener : IConnectionEventListener
        {
            readonly SocketRemotingClient _remotingClient;
            public ConnectionEventListener(SocketRemotingClient remotingClient)
            {
                _remotingClient = remotingClient;
            }

            public void OnConnectionAccepted(ITcpConnection connection) { }
            public void OnConnectionEstablished(ITcpConnection connection)
            {
                throw new NotImplementedException();
            }
            public void OnConnectionClosed(ITcpConnection connection, SocketError socketError)
            {
                throw new NotImplementedException();
            }
            public void OnConnectionFailed(EndPoint remotingEndPoint, SocketError socketError)
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
