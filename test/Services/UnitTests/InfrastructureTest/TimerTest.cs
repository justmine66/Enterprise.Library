using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace InfrastructureTest
{
    public class TimerTest
    {
        public static void Test()
        {
            var timer = new Timer(self =>
            {
                
            });
            timer.Change(0, 3000);

            Console.Read();
        }
    }
}
