using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Enterprise.Library.Common.Performances
{
    public class CountInfo
    {
        DefaultPerformanceService _service;

        long _totalCount;
        long _previousCount;
        long _throughput;
        long _averageThroughput;
        long _throughputCalculateCount;

        long _rtCount;
        long _totalRTTime;
        long _rtTime;
        double _rt;
        double _averageRT;
        long _rtCalculateCount;

        public CountInfo(DefaultPerformanceService service, long initialCount, double rtMilliseconds)
        {
            _service = service;
            _totalCount = initialCount;
            _rtCount = initialCount;
            Interlocked.Add(ref _rtTime, (long)(rtMilliseconds * 1000));
            Interlocked.Add(ref _totalRTTime, (long)(rtMilliseconds * 1000));
        }

        public void IncrementTotalCount(double rtMilliseconds)
        {
            Interlocked.Increment(ref _totalCount);
            Interlocked.Increment(ref _rtCount);
            Interlocked.Add(ref _rtTime, (long)(rtMilliseconds * 1000));
            Interlocked.Add(ref _totalRTTime, (long)(rtMilliseconds * 1000));
        }

        public void UpdateTotalCount(long count, double rtMilliseconds)
        {
            _totalCount = count;
            _rtCount = count;
            Interlocked.Add(ref _rtTime, (long)(rtMilliseconds * 1000));
            Interlocked.Add(ref _totalRTTime, (long)(rtMilliseconds * 1000));
        }

        public long TotalCount
        {
            get { return _totalCount; }
        }
        public long Throughput
        {
            get { return _throughput; }
        }
        public long AverageThroughput
        {
            get { return _averageThroughput; }
        }
        public double RT
        {
            get { return _rt; }
        }
        public double AverageRT
        {
            get { return _averageRT; }
        }

        public void Calculate()
        {
            throw new NotImplementedException();
        }

        private void CalculateThroughput()
        {
            var totalCount = _totalCount;
            _throughput = totalCount - _previousCount;
            _previousCount = totalCount;


        }
    }
}
