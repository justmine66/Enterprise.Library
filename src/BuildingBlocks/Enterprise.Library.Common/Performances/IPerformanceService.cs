using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Library.Common.Performances
{
    public interface IPerformanceService
    {
        string Name { get; }
        PerformanceServiceSetting Setting { get; }
        IPerformanceService Initialize(string name, PerformanceServiceSetting setting = null);
        void Start();
        void Stop();
        void IncrementKeyCount(string key, double rtMilliseconds);
        void UpdateKeyCount(string key, long count, double rtMilliseconds);
        PerformanceInfo GetKeyPerformanceInfo(string key);
    }
}
