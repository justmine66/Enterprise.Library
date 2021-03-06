﻿using Enterprise.Library.Common.Components;
using Enterprise.Library.Common.Logging;
using Enterprise.Library.Common.Socketing.Buffering;
using Enterprise.Library.Common.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Enterprise.Library.Common.Socketing
{
    /// <summary>
    /// Represents a server-side socket.
    /// </summary>
    public class ServerSocket
    {
        #region [ private fields and constructors ]

        private readonly Socket _socket;
        private readonly SocketSetting _setting;
        private readonly IPEndPoint _listeningEndPoint;
        private readonly SocketAsyncEventArgs _acceptSocketArgs;
        private readonly IList<IConnectionEventListener> _connectionEventListeners;
        private readonly Action<ITcpConnection, byte[], Action<byte[]>> _messageArrivedHandler;
        private readonly IBufferPool _receiveDataBufferPool;
        private readonly ConcurrentDictionary<Guid, ITcpConnection> _connectionDict;
        private readonly ILogger _logger;

        public ServerSocket(
            IPEndPoint listeningEndPoint,
            SocketSetting setting,
            IBufferPool receiveDataBufferPool,
            Action<ITcpConnection, byte[], Action<byte[]>> messageArrivedHandler)
        {
            Ensure.NotNull(listeningEndPoint, "listeningEndPoint");
            Ensure.NotNull(setting, "setting");
            Ensure.NotNull(receiveDataBufferPool, "receiveDataBufferPool");
            Ensure.NotNull(messageArrivedHandler, "messageArrivedHandler");

            _listeningEndPoint = listeningEndPoint;
            _setting = setting;
            _receiveDataBufferPool = receiveDataBufferPool;
            _connectionEventListeners = new List<IConnectionEventListener>();
            _messageArrivedHandler = messageArrivedHandler;
            _connectionDict = new ConcurrentDictionary<Guid, ITcpConnection>();
            _socket = SocketUtils.CreateSocket(_setting.SendBufferSize, _setting.ReceiveBufferSize);
            _acceptSocketArgs = new SocketAsyncEventArgs();
            _acceptSocketArgs.Completed += AcceptCompleted;
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
        }

        #endregion

        #region [ public methods ]

        /// <summary>
        /// Registers a connection event listener.
        /// </summary>
        /// <param name="listener"></param>
        public void RegisterConnectionEventListener(IConnectionEventListener listener)
        {
            _connectionEventListeners.Add(listener);
        }
        /// <summary>
        /// Starts a server-side socket such that it is listening for incoming connection requests.
        /// </summary>
        public void Start()
        {
            _logger.InfoFormat("Socket server is starting, listening on TCP endpoint: {0}.", _listeningEndPoint);

            try
            {
                _socket.Bind(_listeningEndPoint);
                _socket.Listen(5000);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Failed to listen on TCP endpoint: {0}.", _listeningEndPoint), ex);
                SocketUtils.ShutdownSocket(_socket);
                throw;
            }

            StartAccepting();
        }
        /// <summary>
        /// Shutdown a server-side socket and releases all resources.
        /// </summary>
        public void Shutdown()
        {
            SocketUtils.ShutdownSocket(_socket);
            _logger.InfoFormat("Socket server shutdown, listening TCP endpoint: {0}.", _listeningEndPoint);
        }
        /// <summary>
        /// Pushes remoting message to all client-side tcp connections.
        /// </summary>
        /// <param name="message">the remoting message as the array of byte.</param>
        public void PushMessageToAllConnections(byte[] message)
        {
            foreach (var connection in _connectionDict.Values)
            {
                connection.QueueMessage(message);
            }
        }
        /// <summary>
        /// Pushes remoting message to a specified client-side tcp connection.
        /// </summary>
        /// <param name="connectionId">the identifier of connection.</param>
        /// <param name="message">the remoting message as the array of byte.</param>
        public void PushMessageToConnection(Guid connectionId, byte[] message)
        {
            ITcpConnection connection;
            if (_connectionDict.TryGetValue(connectionId, out connection))
            {
                connection.QueueMessage(message);
            }
        }
        /// <summary>
        /// Returns all client-side tcp connections which are pended the server-side socket.
        /// </summary>
        /// <returns></returns>
        public IList<ITcpConnection> GetAllConnections()
        {
            return _connectionDict.Values.ToList();
        }

        #endregion

        #region [ internal methods ]

        private void StartAccepting()
        {
            try
            {
                var firedAsync = _socket.AcceptAsync(_acceptSocketArgs);
                if (!firedAsync)
                {
                    ProcessAccept(_acceptSocketArgs);
                }
            }
            catch (Exception ex)
            {
                if (!(ex is ObjectDisposedException))
                {
                    _logger.Info("Socket accept has exception.", ex);
                }
                Task.Factory.StartNew(() => StartAccepting());
            }
        }
        // This method is the callback method associated with Socket.AcceptAsync 
        // operations and is invoked when an accept operation is complete
        private void AcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }
        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            try
            {
                if (e.SocketError == SocketError.Success)
                {
                    var acceptSocket = e.AcceptSocket;
                    e.AcceptSocket = null;
                    OnSocketAccepted(acceptSocket);
                }
                else
                {
                    SocketUtils.ShutdownSocket(e.AcceptSocket);
                    e.AcceptSocket = null;
                }
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                _logger.Error("Process socket accept has exception.", ex);
            }
            finally
            {
                // Accept the next connection request.
                StartAccepting();
            }
        }
        private void OnSocketAccepted(Socket socket)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var connection = new TcpConnection(
                        socket,
                        _setting,
                        _receiveDataBufferPool,
                        OnMessageArrived,
                        OnConnectionClosed);

                    if (_connectionDict.TryAdd(connection.Id, connection))
                    {
                        _logger.InfoFormat("Socket accepted, remote endpoint:{0}", socket.RemoteEndPoint);

                        foreach (var listener in _connectionEventListeners)
                        {
                            try
                            {
                                listener.OnConnectionAccepted(connection);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(string.Format("Notify connection accepted failed, listener type:{0}", listener.GetType().Name), ex);
                            }
                        }
                    }
                    else
                    {
                        _logger.InfoFormat("Duplicated tcp connection, remote endpoint:{0}", socket.RemoteEndPoint);
                    }
                }
                catch (ObjectDisposedException) { }
                catch (Exception ex)
                {
                    _logger.Info("Accept socket client has unknown exception.", ex);
                }
            });
        }
        private void OnMessageArrived(ITcpConnection connection, byte[] message)
        {
            try
            {
                _messageArrivedHandler(connection, message, reply =>
                {
                    Task.Factory.StartNew(() => connection.QueueMessage(reply));
                });
            }
            catch (Exception ex)
            {
                _logger.Error("Handle message error.", ex);
            }
        }
        private void OnConnectionClosed(ITcpConnection connection, SocketError socketError)
        {
            _connectionDict.TryRemove(connection.Id, out connection);
            foreach (var listener in _connectionEventListeners)
            {
                try
                {
                    listener.OnConnectionClosed(connection, socketError);
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Notify connection closed failed, listener type:{0}", listener.GetType().Name), ex);
                }
            }
        }

        #endregion
    }
}
