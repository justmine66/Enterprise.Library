using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Library.Common.Socketing.Buffering
{
    /// <summary>
    /// The pool item creator interface.
    /// </summary>
    public interface IPoolItemCreator<out T>
    {
        /// <summary>
        /// Creates the items of the specified count.
        /// </summary>
        /// <param name="count">the count.</param>
        /// <returns></returns>
        IEnumerable<T> Create(int count);
    }
}
