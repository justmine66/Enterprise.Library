using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Library.Common.Serializing
{
    public interface IBinarySerializer
    {
        byte[] Serialize(object obj);
        object Deserialize(byte[] data, Type type);
        T Deserialize<T>(byte[] data) where T : class;
    }
}
