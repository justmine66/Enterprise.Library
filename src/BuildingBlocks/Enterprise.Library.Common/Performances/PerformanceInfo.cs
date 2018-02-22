using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Library.Common.Performances
{
    public class PerformanceInfo
    {
        public long TotalCount { get; private set; }
        public long Throughput { get; private set; }
        public long AverageThroughput { get; private set; }
        /// <summary>
        /// The response time.
        /// </summary>
        public double RT { get; private set; }
        /// <summary>
        /// The average response time.
        /// </summary>
        public double AverageRT { get; private set; }

        public PerformanceInfo(long totalCount, long throughput, long averageThroughput, double rt, double averageRT)
        {
            this.TotalCount = totalCount;
            this.Throughput = throughput;
            this.AverageThroughput = averageThroughput;
            this.RT = rt;
            this.AverageRT = averageRT;
        }
    }
}
