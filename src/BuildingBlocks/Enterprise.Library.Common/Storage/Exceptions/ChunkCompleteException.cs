using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Library.Common.Storage.Exceptions
{
    public class ChunkCompleteException : Exception
    {
        public ChunkCompleteException(string message) : base(message) { }
    }
}
