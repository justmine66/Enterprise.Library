using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Library.Common.Storage.Exceptions
{
    public class ChunkCreateException : Exception
    {
        public ChunkCreateException(string message) : base(message) { }
    }
}
