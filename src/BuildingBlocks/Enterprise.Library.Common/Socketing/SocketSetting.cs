using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Library.Common.Socketing
{
    /// <summary>
    /// socket setting
    /// </summary>
    public class SocketSetting
    {
        /// <summary>
        /// the size of send buffer of socket. default 64kb.
        /// </summary>
        public int SendBufferSize = 1024 * 64;
        /// <summary>
        /// the size of receive buffer of socket. default 64kb.
        /// </summary>
        public int ReceiveBufferSize = 1024 * 64;

        /// <summary>
        /// the max size of Send Packet of socket. default 64kb.
        /// </summary>
        public int MaxSendPacketSize = 1024 * 64;
        /// <summary>
        /// the Threshold of send message flow control of socket. default 1000.
        /// </summary>
        public int SendMessageFlowControlThreshold = 1000;

        /// <summary>
        /// the interval reconnecting the server.default 1 second.
        /// </summary>
        public int ReconnectToServerInterval = 1000;
        /// <summary>
        /// the interval scanning request timeout .default 1 second.
        /// </summary>
        public int ScanTimeoutRequestInterval = 1000;

        /// <summary>
        /// the size of buffer of receive data. default 64kb.
        /// </summary>
        public int ReceiveDataBufferSize = 1024 * 64;
        /// <summary>
        /// the size of buffer pool of receive data. default 50 size.
        /// </summary>
        public int ReceiveDataBufferPoolSize = 50;
    }
}
