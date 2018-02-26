using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Library.Common.Storage.Exceptions
{
    public class ChunkNotExistException : Exception
    {
        public ChunkNotExistException(long position, int chunkNum) : base(string.Format("Chunk not exist, position: {0}, chunkNum: {1}", position, chunkNum)) { }
    }
}
