using InfrastructureTest.Socketing;
using System;

namespace InfrastructureTest
{
    class Program
    {
        //Packing library to nuget
        //https://docs.microsoft.com/zh-cn/nuget/quickstart/create-and-publish-a-package-using-visual-studio
        //https://docs.microsoft.com/zh-cn/nuget/quickstart/create-and-publish-a-package-using-the-dotnet-cli
        //nuget api key: oy2nqrtr4dlu4bs2lfdqzkfatnsthlouhepjz4skg5q3ci
        //dotnet nuget push Enterprise.Library.Common.1.0.0.nupkg -k oy2nqrtr4dlu4bs2lfdqzkfatnsthlouhepjz4skg5q3ci -s https://api.nuget.org/v3/index.json
        //dotnet nuget push Enterprise.Library.Common.Autofac.1.0.0.nupkg -k oy2nqrtr4dlu4bs2lfdqzkfatnsthlouhepjz4skg5q3ci -s https://api.nuget.org/v3/index.json
        //dotnet nuget push Enterprise.Library.Common.Log4NetLogging.1.0.0.nupkg -k oy2nqrtr4dlu4bs2lfdqzkfatnsthlouhepjz4skg5q3ci -s https://api.nuget.org/v3/index.json
        //dotnet nuget push Enterprise.Library.Common.ConsoleLogging.1.0.0.nupkg -k oy2nqrtr4dlu4bs2lfdqzkfatnsthlouhepjz4skg5q3ci -s https://api.nuget.org/v3/index.json
        static void Main(string[] args)
        {
            //SpinWaitTest.DirectMain();
            //TwoPhraseWaitOfSpinWaitImplTest.DirectMain();
            //BufferPoolTest.Get_item_of_buffer_pool();
            //BufferPoolTest.Expanding_buffer_pool();
            //TCSTest.MainInternal();
            //CancellationTokenSourceTest.Test();
            //CancellationTokenSourceTest.Test1();
            //CancellationTokenSourceTest.Test2();
            //CancellationTokenSourceTest.Test3();
            //TimerTest.Test();
            //SemaphoreTest.Test();
            //SocketingTest.Test();
            //Base64UrlTextEncoderTest.Test();
            //MessageLenghtTest.Test();
            //MemoryManageTest.Test();
            TailRecursionOptimizationTest.Test();
        }
    }
}
