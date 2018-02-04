using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InfrastructureTest
{
    public class CancellationTokenSourceTest
    {
        public static void Test()
        {
            var cts1 = new CancellationTokenSource();
            cts1.Token.Register(() => Console.WriteLine("cts1 canceled"));

            var cts2 = new CancellationTokenSource();
            cts2.Token.Register(() => Console.WriteLine("cts2 canceled"));

            var linkedcts = CancellationTokenSource.CreateLinkedTokenSource(cts1.Token, cts2.Token);
            linkedcts.Token.Register(() => Console.WriteLine("linkedcts canceled"));

            cts2.Cancel();
            Console.WriteLine("cts1:{0}  cts2:{1}  linkedcts:{2}",
                cts1.IsCancellationRequested, cts2.IsCancellationRequested, linkedcts.IsCancellationRequested);

            Console.Read();
        }

        public static void Test1()
        {
            var cancel = new CancellationTokenSource();
            cancel.Token.Register(() =>
            {
                Console.WriteLine("Register3");
            });
            cancel.Token.Register(() =>
            {
                Console.WriteLine("Register");
            });
            cancel.Token.Register(() =>
            {
                Console.WriteLine("Register1");
            });
            CancellationTokenRegistration registration = cancel.Token.Register(() =>
            {
                Console.WriteLine("Register2");
            });

            registration.Dispose();
            cancel.Cancel();

            Console.Read();
        }

        /// <summary>
        /// Task没有被运行就直接取消
        /// </summary>
        public static void Test2()
        {
            var cts = new CancellationTokenSource();
            var longTask = new Task<int>(() => TaskMethod("Task 1", 10, cts.Token), cts.Token);
            Console.WriteLine("取消前，第一个任务的状态：{0}", longTask.Status);
            cts.Cancel(); //取消任务！  
            Console.WriteLine("取消后，第一个任务的状态：{0}", longTask.Status);
            Console.WriteLine("第一个任务在被执行前就已经取消了！");

            Console.Read();
        }

        /// <summary>
        /// Task启动后，在cancel了以后，任务状态显示为RanToCompletion，这是因为从TPL的视角看，这个任务虽然取消了，但是它正常完成了工作并且返回了-1.
        /// </summary>
        public static void Test3()
        {
            var cts = new CancellationTokenSource();
            var longTask = new Task<int>(() => TaskMethod("Task 2", 10, cts.Token), cts.Token);
            longTask.Start(); //启动任务  
            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
                Console.WriteLine(longTask.Status);
            }
            cts.Cancel();
            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
                Console.WriteLine(longTask.Status);
            }

            Console.WriteLine("A task has been completed with result {0}.", longTask.Result);

            Console.Read();
        }

        private static int TaskMethod(string name, int seconds, CancellationToken token)
        {
            Console.WriteLine("Task {0} 运行在线程 {1} 上。是否是线程池线程: {2}",
            name, Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.IsThreadPoolThread);
            for (int i = 0; i < seconds; i++)
            {
                Thread.Sleep(1000);
                if (token.IsCancellationRequested) return -1;
            }
            return 42 * seconds;
        }
    }
}
