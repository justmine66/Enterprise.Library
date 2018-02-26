using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Library.Common.Storage.Exceptions
{
    public class ChunkBadDataException : Exception
    {
        public ChunkBadDataException(string message) : base(message)
        {
        }
    }
}
