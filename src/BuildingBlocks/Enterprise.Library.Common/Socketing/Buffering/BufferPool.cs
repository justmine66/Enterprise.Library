using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Library.Common.Socketing.Buffering
{
    /// <summary>
    /// buffer pool 
    /// </summary>
    public class BufferPool : IntelligentPool<byte[]>, IBufferPool
    {
        public BufferPool(int bufferSize, int initialCount)
            : base(initialCount, new BufferItemCreator(bufferSize))
        {
            this.BufferSize = bufferSize;
        }

        /// <summary>
        /// The capacity of buffer item.
        /// </summary>
        public int BufferSize { get; private set; }
    }
}
