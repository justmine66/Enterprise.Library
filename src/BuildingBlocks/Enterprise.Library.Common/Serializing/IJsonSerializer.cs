using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Library.Common.Serializing
{
    public interface IJsonSerializer
    {
        string Serialize(object obj);
        object Deserialize(string value, Type type);
        TResult Deserialize<TResult>(string value) where TResult : class;
    }
}
