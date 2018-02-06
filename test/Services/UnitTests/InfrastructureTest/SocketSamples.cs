using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace InfrastructureTest
{
    // Implements the connection logic for the socket server.  
    // After accepting a connection, all data read from the client 
    // is sent back to the client. The read and echo back to the client pattern 
    // is continued until the client disconnects.
    public class Server
    {
        int m_numConnections;
        int m_receiveBufferSize;
        BufferManager m_bufferManager;
        const int opsToPreAlloc = 2;
        Socket listenSocket;
    }
}
