using Enterprise.Library.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Enterprise.Library.EventStore
{
    public class StreamIdUtil
    {
        private static byte[] ipBytes;
        private static byte[] portBytes;

        public static string CreateStreamId(IPAddress ipaddress, int port, long streamPosition)
        {
            if (ipBytes == null)
            {
                ipBytes = ipaddress.GetAddressBytes();
            }
            if (portBytes == null)
            {
                portBytes = BitConverter.GetBytes(port);
            }
            byte[] positionBytes = BitConverter.GetBytes(streamPosition);
            byte[] streamIdBytes = ByteUtils.Combine(ipBytes, portBytes, positionBytes);

            return ObjectId.ToHexString(streamIdBytes);
        }

        public static StreamIdInfo ParseStreamId(string streamId)
        {
            byte[] streamIdBytes = ObjectId.ParseHexString(streamId);

            var ipBytes = new byte[4];
            var portBytes = new byte[4];
            var positionBytes = new byte[8];

            Buffer.BlockCopy(streamIdBytes, 0, ipBytes, 0, 4);
            Buffer.BlockCopy(streamIdBytes, 4, portBytes, 0, 4);
            Buffer.BlockCopy(streamIdBytes, 8, positionBytes, 0, 8);

            var port = BitConverter.ToInt32(portBytes, 0);
            var streamPosition = BitConverter.ToInt64(positionBytes, 0);

            return new StreamIdInfo() { IPAddress = new IPAddress(ipBytes), Port = port, LogPosition = streamPosition };
        }
    }

    public struct StreamIdInfo
    {
        public IPAddress IPAddress { get; set; }
        public int Port { get; set; }
        public long LogPosition { get; set; }
    }
}
