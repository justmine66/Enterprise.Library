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
            ByteUtils.EncodeString(request.Id, out IdLengthBytes, out IdBytes);

            byte[] sequenceBytes = BitConverter.GetBytes(request.Sequence);
            byte[] codeBytes = BitConverter.GetBytes(request.Code);
            byte[] typeBytes = BitConverter.GetBytes(request.Type);
            byte[] createdTimeBytes = ByteUtils.EncodeDateTime(request.CreatedTime);
            byte[] headerBytes = HeaderUtils.EncodeHeader(request.Header);
            byte[] headerLengthBytes = BitConverter.GetBytes(headerBytes.Length);

            return ByteUtils.Combine(
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
        public static RemotingRequest ParseRequest(byte[] value)
        {
            int srcOffset = 0;

            string id = ByteUtils.DecodeString(value, srcOffset, out srcOffset);
            long sequence = ByteUtils.DecodeLong(value, srcOffset, out srcOffset);
            short code = ByteUtils.DecodeShort(value, srcOffset, out srcOffset);
            short type = ByteUtils.DecodeShort(value, srcOffset, out srcOffset);
            DateTime createdTime = ByteUtils.DecodeDateTime(value, srcOffset, out srcOffset);
            int headerLength = ByteUtils.DecodeInt(value, srcOffset, out srcOffset);
            IDictionary<string, string> header = HeaderUtils.DecodeHeader(value, srcOffset, out srcOffset);
            int bodyLength = value.Length - srcOffset;
            byte[] body = new byte[bodyLength];

            Buffer.BlockCopy(value, srcOffset, body, 0, bodyLength);

            return new RemotingRequest(id, code, sequence, body, createdTime, header) { Type = type };
        }

        public static byte[] BuildResponseMessage(RemotingResponse response)
        {
            var requestSequenceBytes = BitConverter.GetBytes(response.RequestSequence);
            var requestCodeBytes = BitConverter.GetBytes(response.RequestCode);
            var requestTypeBytes = BitConverter.GetBytes(response.RequestType);
            var requestTimeBytes = ByteUtils.EncodeDateTime(response.RequestTime);
            var requestHeaderBytes = HeaderUtils.EncodeHeader(response.RequestHeader);
            var requestHeaderLengthBytes = BitConverter.GetBytes(requestHeaderBytes.Length);

            var responseCodeBytes = BitConverter.GetBytes(response.ResponseCode);
            var responseTimeBytes = ByteUtils.EncodeDateTime(response.ResponseTime);
            var responseHeaderBytes = HeaderUtils.EncodeHeader(response.ResponseHeader);
            var responseHeaderLengthBytes = BitConverter.GetBytes(requestHeaderBytes.Length);

            return ByteUtils.Combine(
                requestSequenceBytes,
                requestCodeBytes,
                requestTypeBytes,
                requestTimeBytes,
                requestHeaderLengthBytes,
                requestHeaderBytes,
                responseCodeBytes,
                responseTimeBytes,
                responseHeaderLengthBytes,
                responseHeaderBytes,
                response.ResponseBody);
        }
        public static RemotingResponse ParseResponse(byte[] data)
        {
            var srcOffset = 0;

            long requestSequence = ByteUtils.DecodeLong(data, srcOffset, out srcOffset);
            short requestCode = ByteUtils.DecodeShort(data, srcOffset, out srcOffset);
            short requestType = ByteUtils.DecodeShort(data, srcOffset, out srcOffset);
            DateTime requestTime = ByteUtils.DecodeDateTime(data, srcOffset, out srcOffset);
            int requestHeaderLength = ByteUtils.DecodeInt(data, srcOffset, out srcOffset);
            IDictionary<string, string> requestHeader = HeaderUtils.DecodeHeader(data, srcOffset, out srcOffset);
            short responseCode = ByteUtils.DecodeShort(data, srcOffset, out srcOffset);
            DateTime responseTime = ByteUtils.DecodeDateTime(data, srcOffset, out srcOffset);
            int responseHeaderLength = ByteUtils.DecodeInt(data, srcOffset, out srcOffset);
            IDictionary<string, string> responseHeader = HeaderUtils.DecodeHeader(data, srcOffset, out srcOffset);

            int responseBodyLength = data.Length - srcOffset;
            var responseBody = new byte[responseBodyLength];

            Buffer.BlockCopy(data, srcOffset, responseBody, 0, responseBodyLength);

            return new RemotingResponse(
                requestType,
                requestCode,
                requestSequence,
                requestTime,
                responseCode,
                responseBody,
                responseTime,
                requestHeader,
                responseHeader);
        }

        public static byte[] BuildRemotingServerMessage(RemotingServerMessage message)
        {
            byte[] IdBytes;
            byte[] IdLengthBytes;
            ByteUtils.EncodeString(message.Id, out IdLengthBytes, out IdBytes);

            var typeBytes = BitConverter.GetBytes(message.Type);
            var codeBytes = BitConverter.GetBytes(message.Code);
            var createdTimeBytes = ByteUtils.EncodeDateTime(message.CreatedTime);
            var headerBytes = HeaderUtils.EncodeHeader(message.Header);
            var headerLengthBytes = BitConverter.GetBytes(headerBytes.Length);

            return ByteUtils.Combine(
                IdLengthBytes,
                IdBytes,
                typeBytes,
                codeBytes,
                createdTimeBytes,
                headerLengthBytes,
                headerBytes,
                message.Body);
        }
        public static RemotingServerMessage ParseRemotingServerMessage(byte[] data)
        {
            var srcOffset = 0;

            string id = ByteUtils.DecodeString(data, srcOffset, out srcOffset);
            short type = ByteUtils.DecodeShort(data, srcOffset, out srcOffset);
            short code = ByteUtils.DecodeShort(data, srcOffset, out srcOffset);
            DateTime createdTime = ByteUtils.DecodeDateTime(data, srcOffset, out srcOffset);
            int headerLength = ByteUtils.DecodeInt(data, srcOffset, out srcOffset);
            IDictionary<string, string> header = HeaderUtils.DecodeHeader(data, srcOffset, out srcOffset);
            int bodyLength = data.Length - srcOffset;
            var body = new byte[bodyLength];

            Buffer.BlockCopy(data, srcOffset, body, 0, bodyLength);

            return new RemotingServerMessage(type, id, code, body, createdTime, header);
        }
    }

    public class HeaderUtils
    {
        public static readonly byte[] ZeroLengthBytes = BitConverter.GetBytes(0);
        public static readonly byte[] EmptyBytes = new byte[0];

        public static byte[] EncodeHeader(IDictionary<string, string> header)
        {
            int headerKeyCount = header == null ? 0 : header.Count;
            byte[] headerKeyCountBytes = BitConverter.GetBytes(headerKeyCount);
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

                    ByteUtils.EncodeString(entry.Key, out keyLengthBytes, out keyBytes);
                    ByteUtils.EncodeString(entry.Key, out valueLengthBytes, out valueBytes);


                    bytesList.Add(keyLengthBytes);
                    bytesList.Add(keyBytes);
                    bytesList.Add(valueLengthBytes);
                    bytesList.Add(valueBytes);
                }
            }

            return ByteUtils.Combine(bytesList.ToArray());
        }

        public static IDictionary<string, string> DecodeHeader(
            byte[] data,
            int startOffset,
            out int nextStartOffset)
        {
            var dict = new Dictionary<string, string>();
            int srcOffset = startOffset;
            int headerKeyCount = ByteUtils.DecodeInt(data, srcOffset, out srcOffset);

            for (int i = 0; i < headerKeyCount; i++)
            {
                string key = ByteUtils.DecodeString(data, srcOffset, out srcOffset);
                string value = ByteUtils.DecodeString(data, srcOffset, out srcOffset);
                dict.Add(key, value);
            }
            nextStartOffset = srcOffset;
            return dict;
        }
    }
}
