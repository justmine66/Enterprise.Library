using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Library.Common.Storage.Exceptions
{
    public class ChunkReadException : Exception
    {
        public ChunkReadException(string message) : base(message) { }
    }
}
