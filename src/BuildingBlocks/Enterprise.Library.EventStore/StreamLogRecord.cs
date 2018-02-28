using Enterprise.Library.Common.Extensions;
using Enterprise.Library.Common.Storage;
using Enterprise.Library.Common.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Enterprise.Library.EventStore
{
    public class StreamLogRecord : EventStream, ILogRecord
    {
        public long LogPosition { get; set; }
        public string StreamId { get; set; }
        public IPAddress IPAddress { get; set; }
        public int Port { get; set; }

        public StreamLogRecord(IPAddress address, int port)
        {
            this.IPAddress = address;
            this.Port = port;
        }

        public void ReadFrom(byte[] recordBuffer)
        {
            var srcOffset = 0;
            this.LogPosition = ByteUtils.DecodeLong(recordBuffer, srcOffset, out srcOffset);
            this.StreamId = ByteUtils.DecodeString(recordBuffer, srcOffset, out srcOffset);
            this.SourceId = ByteUtils.DecodeString(recordBuffer, srcOffset, out srcOffset);
            this.Name = ByteUtils.DecodeString(recordBuffer, srcOffset, out srcOffset);
            this.Version = ByteUtils.DecodeInt(recordBuffer, srcOffset, out srcOffset);
            this.Events = ByteUtils.DecodeString(recordBuffer, srcOffset, out srcOffset);
            this.Timestamp = ByteUtils.DecodeDateTime(recordBuffer, srcOffset, out srcOffset);
            this.CommandId = ByteUtils.DecodeString(recordBuffer, srcOffset, out srcOffset);
            this.Items = ByteUtils.DecodeString(recordBuffer, srcOffset, out srcOffset);
        }

        public void WriteTo(long logPosition, BinaryWriter writer)
        {
            this.LogPosition = logPosition;
            var streamId = StreamIdUtil.CreateStreamId(this.IPAddress, this.Port, this.LogPosition);

            writer.WriteLong(this.LogPosition)
                  .WriteString(this.StreamId)
                  .WriteString(this.SourceId)
                  .WriteString(this.Name)
                  .WriteInt(this.Version)
                  .WriteString(this.Events)
                  .WriteDatetime(this.Timestamp)
                  .WriteString(this.CommandId)
                  .WriteString(this.Items);
        }
    }
}
