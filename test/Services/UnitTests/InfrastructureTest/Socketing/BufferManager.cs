using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace InfrastructureTest.Socketing
{
    class BufferManager
    {
        int _maximumBytes;
        byte[] _buffer;
        Stack<int> _freeIndexPool;
        int _currentIndex;
        int _bufferSize;

        public BufferManager(int totalBytes, int bufferSize)
        {
            _maximumBytes = totalBytes;
            _currentIndex = 0;
            _bufferSize = bufferSize;
            _freeIndexPool = new Stack<int>();
        }

        public void InitBuffer()
        {
            _buffer = new byte[_maximumBytes];
        }

        public bool SetBuffer(SocketAsyncEventArgs args)
        {
            if (_freeIndexPool.Count > 0)
            {
                args.SetBuffer(_buffer, _freeIndexPool.Pop(), _bufferSize);
            }
            else
            {
                if ((_maximumBytes - _bufferSize) < _currentIndex)
                {
                    return false;
                }

                args.SetBuffer(_buffer, _currentIndex, _bufferSize);
                _currentIndex += _bufferSize;
            }

            return true;
        }

        public void FreeBuffer(SocketAsyncEventArgs args)
        {
            _freeIndexPool.Push(args.Offset);
            args.SetBuffer(null, 0, 0);
        }
    }
}
