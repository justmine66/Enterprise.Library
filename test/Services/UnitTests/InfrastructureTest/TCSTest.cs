using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InfrastructureTest
{
    public class TCSTest
    {
        public static void MainInternal()
        {
            var tcs1 = new TaskCompletionSource<int>();
            var t1 = tcs1.Task;
            var t2 = tcs1.Task;

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(1000);
                tcs1.SetResult(15);
            });

            // The attempt to get the result of t1 blocks the current thread until the completion source gets signaled.
            // It should be a wait of ~1000 ms.
            var sw = Stopwatch.StartNew();
            int result1 = t1.Result;
            int result2 = t2.Result;
            sw.Stop();

            Console.WriteLine("(ElapsedTime={0}): t1.Result={1} (expected 15) ", sw.ElapsedMilliseconds, result1);
            Console.WriteLine("(ElapsedTime={0}): t2.Result={1} (expected 15) ", sw.ElapsedMilliseconds, result2);

            // ------------------------------------------------------------------
            // Alternatively, an exception can be manually set on a TaskCompletionSource.Task
            var tcs2 = new TaskCompletionSource<int>();
            var t3 = tcs2.Task;

            // Start a background Task that will complete tcs2.Task with an exception
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(1000);
                tcs2.SetException(new InvalidOperationException("simulated exception"));
            });

            // The attempt to get the result of t2 blocks the current thread until the completion source gets signaled with either a result or an exception.
            // In either case it should be a wait of ~1000 ms.
            sw = Stopwatch.StartNew();
            try
            {
                result1 = t3.Result;

                Console.WriteLine("t2.Result succeeded. THIS WAS NOT EXPECTED.");
            }
            catch (AggregateException e)
            {
                Console.Write("(ElapsedTime={0}): ", sw.ElapsedMilliseconds);
                Console.WriteLine("The following exceptions have been thrown by t3.Result: (THIS WAS EXPECTED)");
                for (int j = 0; j < e.InnerExceptions.Count; j++)
                {
                    Console.WriteLine("\n-------------------------------------------------\n{0}", e.InnerExceptions[j].ToString());
                }
            }

            Console.Read();
        }

        public static Task<T> RunAsync<T>(Func<T> function)
        {
            if (function == null) throw new ArgumentNullException("function");
            var tcs = new TaskCompletionSource<T>();
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    T result = function();
                    tcs.SetResult(result);
                }
                catch (Exception exc) { tcs.SetException(exc); }
            });

            return tcs.Task;
        }

        public static Task RunAsync(Action action)
        {
            var tcs = new TaskCompletionSource<Object>();
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    action();
                    tcs.SetResult(null);
                }
                catch (Exception exc) { tcs.SetException(exc); }
            });
            return tcs.Task;
        }
    }
}
