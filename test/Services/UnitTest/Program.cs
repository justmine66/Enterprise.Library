using System;
using System.Threading;
using UnitTest.Infrastructure;

namespace UnitTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //SpinWaitTest.DirectMain();
            TwoPhraseWaitOfSpinWaitImplTest.DirectMain();

            Console.Read();
        }
    }
}
