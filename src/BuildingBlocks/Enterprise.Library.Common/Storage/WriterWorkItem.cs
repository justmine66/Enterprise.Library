using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Enterprise.Library.Common.Storage
{
    public class WriterWorkItem
    {
        public readonly MemoryStream BufferStream;
        public readonly BinaryWriter BufferWriter;
        public readonly IStream WorkingStream;
        public long LastFlushedPosition;

        public WriterWorkItem(IStream stream)
        {
            this.WorkingStream = stream;
            this.BufferStream = new MemoryStream(8192);
            this.BufferWriter = new BinaryWriter(BufferStream);
        }

        public void AppendData(byte[] buffer, int offset, int length)
        {
            this.WorkingStream.Write(buffer, offset, length);
        }
        public void FlushToDisk()
        {
            this.WorkingStream.Flush();
            this.LastFlushedPosition = this.WorkingStream.Position;
        }
        public void ResizeStream(long length)
        {
            this.WorkingStream.SetLength(length);
        }
        public void Dispose()
        {
            WorkingStream.Dispose();
        }
    }
}
