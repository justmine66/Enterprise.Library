using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace InfrastructureTest
{
    public class SemaphoreTest
    {
        static Semaphore sema = new Semaphore(9, 9);
        const int cycleNum = 9;
        public static void Test()
        {
            for (int i = 0; i < cycleNum; i++)
            {
                var td = new Thread(new ParameterizedThreadStart(testFun));
                td.Name = string.Format("编号{0}", i.ToString());
                td.Start(td.Name);
            }
            Console.ReadKey();
        }
        public static void testFun(object obj)
        {
            sema.WaitOne();
            Console.WriteLine(obj.ToString() + "进洗手间：" + DateTime.Now.ToString());
            Thread.Sleep(2000);
            Console.WriteLine("\t" + obj.ToString() + "出洗手间：" + DateTime.Now.ToString());
            sema.Release();
        }
    }
}
