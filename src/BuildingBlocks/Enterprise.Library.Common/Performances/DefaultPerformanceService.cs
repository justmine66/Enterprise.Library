using Enterprise.Library.Common.Logging;
using Enterprise.Library.Common.Scheduling;
using Enterprise.Library.Common.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Enterprise.Library.Common.Performances
{
    public class DefaultPerformanceService : IPerformanceService
    {
        string _name;
        PerformanceServiceSetting _setting;
        string _taskName;

        readonly ILogger _logger;
        readonly IScheduleService _scheduleService;
        readonly ConcurrentDictionary<string, CountInfo> _countInfoDict;

        public DefaultPerformanceService(IScheduleService scheduleService, ILoggerFactory loggerFactory)
        {
            _scheduleService = scheduleService;
            _logger = loggerFactory.Create(this.GetType().FullName);
            _countInfoDict = new ConcurrentDictionary<string, CountInfo>();
        }
        public string Name => _name;
        public PerformanceServiceSetting Setting => _setting;
        public PerformanceInfo GetKeyPerformanceInfo(string key)
        {
            CountInfo countInfo;
            if (_countInfoDict.TryGetValue(key, out countInfo))
            {
                return countInfo.GetCurrentPerformanceInfo();
            }
            return null;
        }

        public IPerformanceService Initialize(string name, PerformanceServiceSetting setting = null)
        {
            Ensure.NotNullOrEmpty(name, "name");

            if (setting == null)
            {
                _setting = new PerformanceServiceSetting
                {
                    AutoLogging = true,
                    StatIntervalSeconds = 1
                };
            }
            else
            {
                _setting = setting;
            }

            Ensure.Positive(_setting.StatIntervalSeconds, "PerformanceServiceSetting.StatIntervalSeconds");

            _name = name;
            _taskName = name + ".Task";

            return this;
        }
        public void Start()
        {
            if (string.IsNullOrWhiteSpace(_taskName))
            {
                throw new Exception(string.Format("Please initialize the {0} before starting it.", this.GetType().FullName));
            }

            _scheduleService.StartTask(_taskName, () =>
            {
                foreach (KeyValuePair<string, CountInfo> entry in _countInfoDict)
                {
                    entry.Value.Calculate();
                }
            }, _setting.StatIntervalSeconds * 1000, _setting.StatIntervalSeconds * 1000);
        }
        public void Stop()
        {
            if (string.IsNullOrEmpty(_taskName))
            {
                return;
            }

            _scheduleService.StopTask(_taskName);
        }
        public void IncrementKeyCount(string key, double rtMilliseconds)
        {
            _countInfoDict.AddOrUpdate(key,
            x =>
            {
                return new CountInfo(this, 1, rtMilliseconds);
            },
            (x, y) =>
            {
                y.IncrementTotalCount(rtMilliseconds);
                return y;
            });
        }
        public void UpdateKeyCount(string key, long count, double rtMilliseconds)
        {
            _countInfoDict.AddOrUpdate(key,
            x =>
            {
                return new CountInfo(this, count, rtMilliseconds);
            },
            (x, y) =>
            {
                y.UpdateTotalCount(count, rtMilliseconds);
                return y;
            });
        }

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

            public void Calculate()
            {
                CalculateThroughput();
                CalculateRT();

                if (_service._setting.AutoLogging)
                {
                    var contextText = string.Empty;
                    if (_service._setting.GetLogContextTextFunc != null)
                    {
                        contextText = _service._setting.GetLogContextTextFunc();
                    }
                    if (!string.IsNullOrWhiteSpace(contextText))
                    {
                        contextText += ", ";
                    }
                    _service._logger.InfoFormat("{0}, {1}totalCount: {2}, throughput: {3}, averageThrughput: {4}, rt: {5:F3}ms, averageRT: {6:F3}ms", _service._name, contextText, _totalCount, _throughput, _averageThroughput, _rt, _averageRT);
                }
                if (_service._setting.PerformanceInfoHandler != null)
                {
                    try
                    {
                        _service._setting.PerformanceInfoHandler(GetCurrentPerformanceInfo());
                    }
                    catch (Exception ex)
                    {
                        _service._logger.Error("PerformanceInfo handler execution has exception.", ex);
                    }
                }
            }
            public PerformanceInfo GetCurrentPerformanceInfo()
            {
                return new PerformanceInfo(TotalCount, Throughput, AverageThroughput, RT, AverageRT);
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

            private void CalculateThroughput()
            {
                long totalCount = _totalCount;
                _throughput = totalCount - _previousCount;
                _previousCount = totalCount;

                if (_throughput > 0)
                {
                    _throughputCalculateCount++;
                    _averageThroughput = totalCount / _throughputCalculateCount;
                }
            }

            private void CalculateRT()
            {
                long rtCount = _rtCount;
                long rtTime = _rtTime;
                long totalRTTime = _totalRTTime;

                if (rtCount > 0)
                {
                    _rt = ((double)rtTime / 1000) / rtCount;
                    _rtCalculateCount += rtCount;
                    _averageRT = ((double)totalRTTime / 1000) / _rtCalculateCount;
                }

                _rtCount = 0L;
                _rtTime = 0L;
            }
        }
    }
}
