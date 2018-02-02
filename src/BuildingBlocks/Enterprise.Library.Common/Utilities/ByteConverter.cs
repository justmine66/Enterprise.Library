using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Enterprise.Library.Common.Utilities
{
    /// <summary>
    /// Converts base data types to an array of bytes, and an array of bytes to base data types.
    /// </summary>
    public class ByteConverter
    {
        public static readonly byte[] ZeroLengthBytes = BitConverter.GetBytes(0);
        public static readonly byte[] EmptyBytes = new byte[0];

        /// <summary>
        /// Encodes string.
        /// </summary>
        /// <param name="data">The value to convert.</param>
        /// <param name="lengthBytes">Returns data.Length as an array of bytes.</param>
        /// <param name="dataBytes">Returns data as an array of bytes.</param>
        public static void GetBytes(string value, out byte[] lengthBytes, out byte[] dataBytes)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                dataBytes = Encoding.UTF8.GetBytes(value);
                lengthBytes = BitConverter.GetBytes(value.Length);
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
        public static byte[] GetBytes(DateTime value)
        {
            return BitConverter.GetBytes(value.Ticks);
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
