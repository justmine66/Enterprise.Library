using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Library.Common.Performances
{
    public class PerformanceServiceSetting
    {
        public int StatIntervalSeconds { get; set; }
        public bool AutoLogging { get; set; }
        public Func<string> GetLogContextTextFunc { get; set; }
        public Action<PerformanceInfo> PerformanceInfoHandler { get; set; }
    }
}
