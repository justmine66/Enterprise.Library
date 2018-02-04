using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Enterprise.Library.Common.Serializing
{
    /// <summary>
    /// Defines a serializer to serialize object to byte array or desrialize byte array to object.
    /// </summary>
    public class DefaultBinarySerializer : IBinarySerializer
    {
        private static readonly BinaryFormatter _binaryFormatter = new BinaryFormatter();

        /// <summary>
        /// Deserialize an object from a byte array.
        /// </summary>
        /// <param name="data">the array of byte.</param>
        /// <param name="type">the type of object.</param>
        /// <returns></returns>
        public object Deserialize(byte[] data, Type type)
        {
            using (var stream = new MemoryStream(data))
            {
                return _binaryFormatter.Deserialize(stream);
            }
        }

        /// <summary>
        /// Deserialize a typed object from a byte array.
        /// </summary>
        /// <typeparam name="T">the type of object.</typeparam>
        /// <param name="data">the array of byte.</param>
        /// <returns></returns>
        public T Deserialize<T>(byte[] data) where T : class
        {
            using (var stream = new MemoryStream(data))
            {
                return _binaryFormatter.Deserialize(stream) as T;
            }
        }

        /// <summary>
        /// Serialize an object to byte array.
        /// </summary>
        /// <param name="obj">The object at the root of the graph to serialize.</param>
        /// <returns>an array of byte.</returns>
        public byte[] Serialize(object obj)
        {
            using (var stream = new MemoryStream())
            {
                _binaryFormatter.Serialize(stream, obj);
                return stream.ToArray();
            }
        }
    }
}
