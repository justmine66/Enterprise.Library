using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Library.Common.Socketing.Framing
{
    /// <summary>
    /// message framer
    /// </summary>
    public interface IMessageFramer
    {
        void UnFrameData(IEnumerable<ArraySegment<byte>> data);
        void UnFrameData(ArraySegment<byte> data);
        IEnumerable<ArraySegment<byte>> FrameData(ArraySegment<byte> data);
        void RegisterMessageArrivedCallback(Action<ArraySegment<byte>> handler);
    }
}
