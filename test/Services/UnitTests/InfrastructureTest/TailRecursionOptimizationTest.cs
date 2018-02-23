using System;
using System.Collections.Generic;
using System.Text;

namespace InfrastructureTest
{
    /// <summary>
    /// 尾递归优化测试
    /// 递归最大的好处就简化代码，它可以把一个复杂的问题用很简单的代码描述出来。注意：递归的精髓是描述问题，而这正是函数式编程的精髓。
    /// </summary>
    public class TailRecursionOptimizationTest
    {
        //斐波那契数列公式=> F(0)=0，F(1)=1, F(n)=F(n-1)+F(n-2)(n>=2，n∈N*)
        /// <summary>
        /// 用线性递归实现Fibonacci函数
        /// 在递归调用的过程当中系统为每一层的返回点、局部量等开辟了栈来存储，因此递归次数过多容易造成栈溢出。
        /// 调用链见图片LinearRecursion.jpg
        /// </summary>
        public static long FibonacciLinearRecursion(long n)
        {
            try
            {
                if (n < 2) return n;
                return FibonacciLinearRecursion(n - 1) + FibonacciLinearRecursion(n - 2);
            }
            catch (StackOverflowException)
            {
                Console.WriteLine("LINEAR RECURSION stack overflow.");
                return -1;
            }
            catch (OverflowException)
            {
                Console.WriteLine("LINEAR RECURSION Arithmetic operation resulted in an overflow.");
                return -1;
            }
        }
        /// <summary>
        /// 用尾递归实现Fibonacci函数
        /// 尾递归
        /// 顾名思义，尾递归就是从最后开始计算, 每递归一次就算出相应的结果, 也就是说, 函数调用出现在调用者函数的尾部, 因为是尾部, 所以根本没有必要去保存任何局部变量.直接让被调用的函数返回时越过调用者, 返回到调用者的调用者去。尾递归就是把当前的运算结果(或路径)放在参数里传给下层函数，深层函数所面对的不是越来越简单的问题，而是越来越复杂的问题，因为参数里带有前面若干步的运算路径。
        /// 尾递归是极其重要的，不用尾递归，函数的堆栈耗用难以估量，需要保存很多中间函数的堆栈。比如f(n, sum) = f(n-1) + value(n) + sum; 会保存n个函数调用堆栈，而使用尾递归f(n, sum) = f(n-1, sum+value(n)); 这样则只保留后一个函数堆栈即可，之前的可优化删去。
        /// 调用链见图片TailRecursion.jpg
        /// 尾递归不需要向上返回了，但是需要引入而外的两个空间来保持当前的结果。
        /// </summary>
        /// <param name="n"></param>
        /// <param name="ret1">当前结果</param>
        /// <param name="ret2"></param>
        /// <returns></returns>
        public static long FibonacciTailRecursion(long n, long ret1, long ret2)
        {
            try
            {
                if (n == 0) return ret1;
                return FibonacciTailRecursion(n - 1, checked(ret2), checked(ret1 + ret2));
            }
            catch (StackOverflowException)
            {
                Console.WriteLine("TAIL RECURSION stack overflow.");
                return ret1;
            }
            catch (OverflowException)
            {
                Console.WriteLine("TAIL RECURSION Arithmetic operation resulted in an overflow.");
                return ret1;
            }
        }
        public static void Test()
        {
            Console.WriteLine("尾递归和线性递归性能对比");
            Console.WriteLine(new string(' ', 50));
            Console.WriteLine("尾递归: " + FibonacciTailRecursion(40, 0, 1));
            Console.WriteLine("尾递归: " + FibonacciTailRecursion(100, 0, 1));
            Console.WriteLine("尾递归: " + FibonacciTailRecursion(100000000, 0, 1));
            Console.WriteLine(new string('-', 50));
            Console.WriteLine("线性递归: " + FibonacciLinearRecursion(40));
            Console.WriteLine("线性递归: " + FibonacciLinearRecursion(100));
            Console.WriteLine("线性递归: " + FibonacciLinearRecursion(100000000));
            Console.Read();
        }
    }
}
