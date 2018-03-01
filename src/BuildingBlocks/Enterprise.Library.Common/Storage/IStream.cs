using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Library.Common.Storage
{
    /// <summary>Provides a generic view of a sequence of bytes.This is an Interface.
    /// </summary>
    public interface IStream
    {
        /// <summary>Gets length of in bytes of stream.
        /// </summary>
        long Length { get; }
        /// <summary>Gets and sets the posion within the current stream.
        /// </summary>
        long Position { get; set; }
        /// <summary>Writes a sequence of bytes to the current stream and advances the current position within the stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes.This method copies count bytes to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin coping bytes to the current stream.</param>
        /// <param name="count">The number of bytes written to the current stream.</param>
        void Write(byte[] buffer, int offset, int count);
        /// <summary>Clears all buffers for this stream and causes any buffered data to be written the underlying device.
        /// </summary>
        void Flush();
        /// <summary>Set the length of the current stream.
        /// </summary>
        /// <param name="value"></param>
        void SetLength(long value);
        /// <summary>Released all unmanaged resources used by the current stream and optionally releases the managed resources.
        /// </summary>
        void Dispose();
    }
}
