using Enterprise.Library.Common.Components;
using Enterprise.Library.Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Enterprise.Library.Common.Socketing.Framing
{
    /// <summary>
    /// represents a no-op message framer
    /// </summary>
    public class NoopMessageFramer : IMessageFramer
    {
        private static readonly ILogger _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(typeof(NoopMessageFramer).FullName);

        private Action<ArraySegment<byte>> _receivedHandler;

        public IEnumerable<ArraySegment<byte>> FrameData(ArraySegment<byte> data)
        {
            yield return data;
        }

        public void RegisterMessageArrivedCallback(Action<ArraySegment<byte>> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            this._receivedHandler = handler;
        }

        public void UnFrameData(IEnumerable<ArraySegment<byte>> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            foreach (ArraySegment<byte> buffer in data)
            {
                Parse(buffer);
            }
        }

        public void UnFrameData(ArraySegment<byte> data)
        {
            Parse(data);
        }

        private void Parse(ArraySegment<byte> bytes)
        {
            if (_receivedHandler != null)
            {
                try
                {
                    _receivedHandler(bytes);
                }
                catch (Exception ex)
                {
                    _logger.Error("Handle received message fail.", ex);
                }
            }
        }
    }
}
