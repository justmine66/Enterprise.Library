using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Enterprise.Library.Common.Storage
{
    public interface ILogRecord
    {
        void WriteTo(long logPosition, BinaryWriter writer);
        void ReadFrom(byte[] recordBuffer);
    }
}
