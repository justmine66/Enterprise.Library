using Enterprise.Library.Common.Logging;
using Enterprise.Library.Common.Scheduling;
using Enterprise.Library.Common.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Library.Common.Performances
{
    public class DefaultPerformanceService : IPerformanceService
    {
        private string _name;
        private PerformanceServiceSetting _setting;
        private string _taskName;

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
            throw new NotImplementedException();
        }

        public void IncrementKeyCount(string key, double rtMilliseconds)
        {
            throw new NotImplementedException();
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
                throw new Exception(string.Format("Please initialize the {0} before start it.", GetType().FullName));
            }

            _scheduleService.StartTask(_taskName, () =>
            {
                foreach (var entry in _countInfoDict)
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

        public void UpdateKeyCount(string key, long count, double rtMilliseconds)
        {
            throw new NotImplementedException();
        }
    }
}
