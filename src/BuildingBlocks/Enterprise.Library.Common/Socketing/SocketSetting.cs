using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Library.Common.Socketing
{
    /// <summary>
    /// socket options
    /// </summary>
    public class SocketSetting
    {
        /// <summary>
        /// the send buffer size to use for each socket I/O operation. default 64kb.
        /// </summary>
        public int SendBufferSize = 1024 * 64;
        /// <summary>
        /// the receive buffer size to use for each socket I/O operation. default 64kb.
        /// </summary>
        public int ReceiveBufferSize = 1024 * 64;

        /// <summary>
        /// the max size of send packet of server-side socket. default 64kb.
        /// </summary>
        public int MaxSendPacketSize = 1024 * 64;
        /// <summary>
        /// the threshold of send message flow control of client-side socket. default 1000.
        /// </summary>
        public int SendMessageFlowControlThreshold = 1000;

        /// <summary>
        /// the interval reconnecting the server of client-side socket.default 1 second.
        /// </summary>
        public int ReconnectToServerInterval = 1000;
        /// <summary>
        /// the interval scanning request timeout of client-side socket.default 1 second.
        /// </summary>
        public int ScanTimeoutRequestInterval = 1000;

        /// <summary>
        /// the size of buffer of receive data. default 64kb.
        /// </summary>
        public int ReceiveDataBufferSize = 1024 * 64;
        /// <summary>
        /// the size of buffer pool of receive data. default 50 count.
        /// </summary>
        public int ReceiveDataBufferPoolSize = 50;
    }
}
