using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Library.Common.Socketing.Buffering
{
    /// <summary>
    /// buffer item creator
    /// </summary>
    public class BufferItemCreator : IPoolItemCreator<byte[]>
    {
        private int _bufferSize;

        public BufferItemCreator(int bufferSize)
        {
            this._bufferSize = bufferSize;
        }

        public IEnumerable<byte[]> Create(int count)
        {
            return new BufferItemEnumerable(this._bufferSize, count);
        }
    }

    public class BufferItemEnumerable : IEnumerable<byte[]>
    {
        private int _bufferSize;
        private int _count;

        public BufferItemEnumerable(int bufferSize, int count)
        {
            this._bufferSize = bufferSize;
            this._count = count;
        }

        public IEnumerator<byte[]> GetEnumerator()
        {
            int count = this._count;

            for (int i = 0; i < count; i++)
            {
                yield return new byte[this._bufferSize];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
