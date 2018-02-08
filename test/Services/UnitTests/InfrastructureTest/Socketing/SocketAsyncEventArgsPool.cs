using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace InfrastructureTest.Socketing
{
    public class SocketAsyncEventArgsPool
    {
        Stack<SocketAsyncEventArgs> _pool;
        readonly object _latchLock = new object();

        public SocketAsyncEventArgsPool(int capacity)
        {
            _pool = new Stack<SocketAsyncEventArgs>(capacity);
        }

        public void Push(SocketAsyncEventArgs item)
        {
            if (item == null)
            {
                throw new ArgumentNullException($"{nameof(item)} added to a SocketAsyncEventArgsPool cannot be null.");
            }

            lock (_latchLock)
            {
                _pool.Push(item);
            }
        }

        /// <summary>
        /// Removes a SocketAsyncEventArgs instance from the pool and returns the object removed from the pool.
        /// </summary>
        /// <returns></returns>
        public SocketAsyncEventArgs Pop()
        {
            lock (_latchLock)
            {
                return _pool.Pop();
            }
        }

        /// <summary>
        /// The numbers of SocketAsyncEventArgs instances in the pool.
        /// </summary>
        public int Count { get { return _pool.Count; } }

        public void Clear()
        {
            _pool.Clear();
        }
    }
}
