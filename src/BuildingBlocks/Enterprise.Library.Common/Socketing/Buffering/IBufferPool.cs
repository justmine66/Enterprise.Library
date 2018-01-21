using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Library.Common.Socketing.Buffering
{
    /// <summary>
    /// The buffer pool interface supporting bytes
    /// </summary>
    public interface IBufferPool : IPool<byte[]>
    {
        /// <summary>
        /// buffer pool size
        /// </summary>
        int BufferSize { get; }
    }
}
