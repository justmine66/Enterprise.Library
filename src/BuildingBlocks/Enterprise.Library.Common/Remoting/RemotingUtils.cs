using Enterprise.Library.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Library.Common.Remoting
{
    public class RemotingUtils
    {
        public static byte[] BuildRequestMessage(RemotingRequest request)
        {
            byte[] IdBytes;
            byte[] IdLengthBytes;
            ByteConverter.GetBytes(request.Id, out IdLengthBytes, out IdBytes);

            byte[] sequenceBytes = BitConverter.GetBytes(request.Sequence);
            byte[] codeBytes = BitConverter.GetBytes(request.Code);
            byte[] typeBytes = BitConverter.GetBytes(request.Type);
            byte[] createdTimeBytes = ByteConverter.GetBytes(request.CreatedTime);
            byte[] headerBytes = HeaderConverter.GetBytes(request.Header);
            byte[] headerLengthBytes = BitConverter.GetBytes(headerBytes.Length);

            return ByteConverter.Combine(
                IdLengthBytes,
                IdBytes,
                sequenceBytes,
                codeBytes,
                typeBytes,
                createdTimeBytes,
                headerLengthBytes,
                headerBytes,
                request.Body);
        }
    }

    public class HeaderConverter
    {
        public static readonly byte[] ZeroLengthBytes = BitConverter.GetBytes(0);
        public static readonly byte[] EmptyBytes = new byte[0];

        public static byte[] GetBytes(IDictionary<string, string> header)
        {
            var headerKeyCount = header == null ? 0 : header.Count;
            var headerKeyCountBytes = BitConverter.GetBytes(headerKeyCount);
            var bytesList = new List<byte[]>();

            bytesList.Add(headerKeyCountBytes);

            if (headerKeyCount > 0)
            {
                foreach (KeyValuePair<string, string> entry in header)
                {
                    byte[] keyBytes;
                    byte[] keyLengthBytes;
                    byte[] valueBytes;
                    byte[] valueLengthBytes;

                    ByteConverter.GetBytes(entry.Key, out keyLengthBytes, out keyBytes);
                    ByteConverter.GetBytes(entry.Key, out valueLengthBytes, out valueBytes);

                    bytesList.Add(keyBytes);
                    bytesList.Add(keyLengthBytes);
                    bytesList.Add(valueBytes);
                    bytesList.Add(valueLengthBytes);
                }
            }

            return ByteConverter.Combine(bytesList.ToArray());
        }

        public static IDictionary<string, string> ToHeader()
        {
            return null;
        }
    }
}
