using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Library.Common.Storage.Exceptions
{
    public class ChunkFileNotExistException : Exception
    {
        public ChunkFileNotExistException(string fileName) : base(string.Format("Chunk file '{0}' not exist.", fileName)) { }
    }
}
