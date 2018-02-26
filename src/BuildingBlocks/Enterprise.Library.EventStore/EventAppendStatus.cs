using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Library.EventStore
{
    public enum EventAppendStatus
    {
        Success = 1,
        Failed = 2,
        DuplicateEvent = 3,
        DuplicateCommand = 4
    }
}
