using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Enterprise.Library.Common.Socketing
{
    /// <summary>
    /// Represents a tcp connection interface.
    /// </summary>
    public interface ITcpConnection
    {
        /// <summary>
        /// The indentifier.
        /// </summary>
        Guid Id { get; }
        bool IsConnected { get; }
        EndPoint LocalEndPoint { get; }
        EndPoint RemotingEndPoint { get; }
        void QueueMessage(byte[] message);
        void Close();
    }
}
