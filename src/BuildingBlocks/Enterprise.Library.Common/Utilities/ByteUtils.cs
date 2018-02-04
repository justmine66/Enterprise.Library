using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Enterprise.Library.Common.Utilities
{
    /// <summary>
    /// Converts base data types to an array of bytes, and an array of bytes to base data types.
    /// </summary>
    public class ByteUtils
    {
        public static readonly byte[] ZeroLengthBytes = BitConverter.GetBytes(0);
        public static readonly byte[] EmptyBytes = new byte[0];

        /// <summary>
        /// Encodes string.
        /// </summary>
        /// <param name="data">The value to convert.</param>
        /// <param name="lengthBytes">Returns data.Length as an array of bytes.</param>
        /// <param name="dataBytes">Returns data as an array of bytes.</param>
        public static void EncodeString(string value, out byte[] lengthBytes, out byte[] dataBytes)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                dataBytes = Encoding.UTF8.GetBytes(value);
                lengthBytes = BitConverter.GetBytes(dataBytes.Length);
            }
            else
            {
                dataBytes = EmptyBytes;
                lengthBytes = ZeroLengthBytes;
            }
        }

        /// <summary>
        /// Encodes string.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>Returns data as an array of bytes.</returns>
        public static byte[] EncodeDateTime(DateTime value)
        {
            return BitConverter.GetBytes(value.Ticks);
        }

        public static string DecodeString(byte[] value, int startOffset, out int nextOffset)
        {
            byte[] bytes = DecodeBytes(value, startOffset, out nextOffset);
            return Encoding.UTF8.GetString(bytes);
        }

        public static byte[] DecodeBytes(byte[] value, int startOffset, out int nextStartOffset)
        {
            var lengthBytes = new byte[4];
            Buffer.BlockCopy(value, startOffset, lengthBytes, 0, 4);
            startOffset += 4;

            var length = BitConverter.ToInt32(lengthBytes, 0);
            var dataBytes = new byte[length];
            Buffer.BlockCopy(value, startOffset, dataBytes, 0, length);
            startOffset += length;

            nextStartOffset = startOffset;

            return dataBytes;
        }

        public static long DecodeLong(byte[] value, int startOffset, out int nextStartOffset)
        {
            var longBytes = new byte[sizeof(long)];
            Buffer.BlockCopy(value, startOffset, longBytes, 0, sizeof(long));
            nextStartOffset = startOffset + sizeof(long);
            return BitConverter.ToInt64(longBytes, 0);
        }

        public static int DecodeInt(byte[] sourceBuffer, int startOffset, out int nextStartOffset)
        {
            var intBytes = new byte[sizeof(int)];
            Buffer.BlockCopy(sourceBuffer, startOffset, intBytes, 0, sizeof(int));
            nextStartOffset = startOffset + sizeof(int);
            return BitConverter.ToInt32(intBytes, 0);
        }

        public static DateTime DecodeDateTime(byte[] value, int startOffset, out int nextStartOffset)
        {
            long ticks = DecodeLong(value, startOffset, out nextStartOffset);
            return new DateTime(ticks);
        }

        public static short DecodeShort(byte[] sourceBuffer, int startOffset, out int nextStartOffset)
        {
            var shortBytes = new byte[sizeof(short)];
            Buffer.BlockCopy(sourceBuffer, startOffset, shortBytes, 0, sizeof(short));
            nextStartOffset = startOffset + sizeof(short);
            return BitConverter.ToInt16(shortBytes, 0);
        }

        /// <summary>
        /// Combines array vector.
        /// </summary>
        /// <param name="arrays">the array vector to combine.</param>
        /// <returns></returns>
        public static byte[] Combine(params byte[][] arrays)
        {
            var destination = new byte[arrays.Sum(x => x.Length)];
            int offset = 0;
            foreach (byte[] data in arrays)
            {
                Buffer.BlockCopy(data, 0, destination, offset, data.Length);
                offset += data.Length;
            }
            return destination;
        }
    }
}
