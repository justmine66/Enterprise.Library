using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Library.Common.Storage
{
    public interface IStream
    {
        long Length { get; }
        long Position { get; set; }
        void Write(byte[] buffer, int offset, int count);
        void Flush();
        void SetLength(long value);
        void Dispose();
    }
}
