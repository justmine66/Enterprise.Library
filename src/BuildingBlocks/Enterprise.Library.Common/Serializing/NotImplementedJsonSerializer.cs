using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Library.Common.Serializing
{
    public class NotImplementedJsonSerializer : IJsonSerializer
    {
        public object Deserialize(string value, Type type)
        {
            throw new NotSupportedException(string.Format("{0} doest not support serializing object.", this.GetType().FullName));
        }

        public TResult Deserialize<TResult>(string value) where TResult : class
        {
            throw new NotSupportedException(string.Format("{0} doest not support serializing object.", this.GetType().FullName));
        }

        public string Serialize(object obj)
        {
            throw new NotSupportedException(string.Format("{0} doest not support serializing object.", this.GetType().FullName));
        }
    }
}
