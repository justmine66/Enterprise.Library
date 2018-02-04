using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Library.Common.Remoting.Exceptions
{
    public class ResponseFutureAddFailedException : Exception
    {
        public ResponseFutureAddFailedException(long requestSequence)
            : base(string.Format("Add remoting request response future failed. request sequence:{0}", requestSequence))
        {
        }
    }
}
