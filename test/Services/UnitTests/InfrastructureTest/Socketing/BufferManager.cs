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
        int _currentIndex;//offset
        int _unitBufferSize;

        public BufferManager(int totalBytes, int unitBufferSize)
        {
            _maximumBytes = totalBytes;
            _currentIndex = 0;
            _unitBufferSize = unitBufferSize;
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
                args.SetBuffer(_buffer, _freeIndexPool.Pop(), _unitBufferSize);
            }
            else
            {
                if ((_maximumBytes - _unitBufferSize) < _currentIndex)
                {
                    return false;
                }

                args.SetBuffer(_buffer, _currentIndex, _unitBufferSize);
                _currentIndex += _unitBufferSize;
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
