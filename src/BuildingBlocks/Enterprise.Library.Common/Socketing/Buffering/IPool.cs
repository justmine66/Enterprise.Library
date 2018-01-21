using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Library.Common.Socketing.Buffering
{
    /// <summary>
    /// The basic pool interface
    /// </summary>
    public interface IPool
    {
        /// <summary>
        /// Gets the total count.
        /// </summary>
        /// <value>
        /// The total count.
        /// </value>
        int TotalCount { get; }

        /// <summary>
        /// Gets the available count, the items count which are available to be used.
        /// </summary>
        /// <value>
        /// The available count.
        /// </value>
        int AvailableCount { get; }

        /// <summary>
        /// Shrinks this pool.
        /// </summary>
        /// <returns></returns>
        bool Shrink();
    }
}
