using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Library.Common.IO
{
    /// <summary>
    /// Represents an async task result.
    /// </summary>
    public class AsyncTaskResult
    {
        public readonly static AsyncTaskResult Success = new AsyncTaskResult(AsyncTaskStatus.Success, null);

        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        public AsyncTaskResult(AsyncTaskStatus status, string errorMessage)
        {
            this.Status = status;
            this.ErrorMessage = errorMessage;
        }
        /// <summary>
        /// Represents the async task result status.
        /// </summary>
        public AsyncTaskStatus Status { get; private set; }
        /// <summary>
        /// Represents the error message if the async task is failed.
        /// </summary>
        public string ErrorMessage { get; private set; }
    }
    /// <summary>
    /// Represents an async task result status enum.
    /// </summary>
    public enum AsyncTaskStatus
    {
        Success,
        IOException,
        Failed
    }
}
