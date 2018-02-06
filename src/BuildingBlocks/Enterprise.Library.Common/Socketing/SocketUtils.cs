using Enterprise.Library.Common.Utilities;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Enterprise.Library.Common.Socketing
{
    /// <summary>
    /// socket utils class
    /// </summary>
    public class SocketUtils
    {
        /// <summary>
        /// get local ipv4
        /// </summary>
        /// <returns></returns>
        public static IPAddress GetLocalIPV4()
        {
            return Dns.GetHostEntry(Dns.GetHostName())
                                    .AddressList
                                    .First(x => x.AddressFamily == AddressFamily.InterNetwork);
        }

        /// <summary>
        /// Creates socket. 
        /// </summary>
        /// <param name="sendBufferSize">the size of the send buffer of the socket.</param>
        /// <param name="receiveBufferSize">the size of the receive buffer of the socket.</param>
        /// <returns></returns>
        public static Socket CreateSocket(int sendBufferSize, int receiveBufferSize)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.NoDelay = true;
            socket.Blocking = false;
            socket.SendBufferSize = sendBufferSize;
            socket.ReceiveBufferSize = receiveBufferSize;
            return socket;
        }

        /// <summary>
        /// shut down socket
        /// </summary>
        /// <param name="socket"></param>
        public static void ShutdownSocket(Socket socket)
        {
            if (socket == null) return;

            Helper.EatException(() => socket.Shutdown(SocketShutdown.Both));
            Helper.EatException(() => socket.Close(10000));
        }

        /// <summary>
        /// close socket
        /// </summary>
        /// <param name="socket"></param>
        public static void CloseSocket(Socket socket)
        {
            if (socket == null) return;

            Helper.EatException(() => socket.Close(10000));
        }
    }
}
