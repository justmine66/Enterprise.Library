using System.Collections.Generic;

namespace InfrastructureTest.Socketing.Client
{
    public class ApiResponse
    {
        private int 心跳;

        public ApiResponse(int 心跳)
        {
            this.心跳 = 心跳;
        }

        public Dictionary<string, object> data { get; internal set; }
    }
}