using Enterprise.Library.Common.Autofac;
using Enterprise.Library.Common.Components;
using Enterprise.Library.Common.Log4NetLogging;
using Enterprise.Library.Common.Performances;
using Enterprise.Library.Common.Storage;
using Enterprise.Library.Common.Storage.FileNamingStrategies;
using Enterprise.Library.Common.Utilities;
using System;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ECommonConfig = Enterprise.Library.Common.Configurations.Configuration;

namespace StoragePerformanceTest
{
    class Program
    {
        static void Main(string[] args)
        {
            WriteChunkPerformanceTest();
        }
        static void WriteChunkPerformanceTest()
        {
            ECommonConfig
                .Create()
                .UseAutofac()
                .RegisterCommonComponents()
                .UseLog4net()
                .RegisterUnhandledExceptionHandler()
                .BuildContainer();

            var storeRootPath = ConfigurationManager.AppSettings["storeRootPath"];                   //文件存储根目录
            var threadCount = int.Parse(ConfigurationManager.AppSettings["concurrentThreadCount"]);  //并行写数据的线程数
            var recordSize = int.Parse(ConfigurationManager.AppSettings["recordSize"]);              //记录大小，字节为单位
            var recordCount = int.Parse(ConfigurationManager.AppSettings["recordCount"]);            //总共要写入的记录数
            var syncFlush = bool.Parse(ConfigurationManager.AppSettings["syncFlush"]);               //是否同步刷盘
            FlushOption flushOption;
            Enum.TryParse(ConfigurationManager.AppSettings["flushOption"], out flushOption);         //同步刷盘方式
            var chunkSize = 1024 * 1024 * 10;//1G
            var flushInterval = 100;
            var maxRecordSize = 5 * 1024 * 1024;//5M
            var chunkWriteBuffer = 128 * 1024;//128KB
            var chunkReadBuffer = 128 * 1024;//128KB
            var chunkManagerConfig = new ChunkManagerConfig(
                Path.Combine(storeRootPath, @"sample-chunks"),
                new DefaultFileNamingStrategy("message-chunk-"),
                chunkSize,
                0,
                0,
                flushInterval,
                false,
                syncFlush,
                flushOption,
                Environment.ProcessorCount * 8,
                maxRecordSize,
                chunkWriteBuffer,
                chunkReadBuffer,
                5,
                1,
                1,
                5,
                10 * 10000,
                true);
            var chunkManager = new ChunkManager("sample-chunk", chunkManagerConfig, false);
            var chunkWriter = new ChunkWriter(chunkManager);
            chunkManager.Load(ReadRecord);
            chunkWriter.Open();
            var record = new BufferLogRecord
            {
                RecordBuffer = new byte[recordSize]
            };
            var count = 0L;
            var performanceService = ObjectContainer.Resolve<IPerformanceService>();
            performanceService.Initialize("WriteChunk").Start();

            for (var i = 0; i < threadCount; i++)
            {
                Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        var current = Interlocked.Increment(ref count);
                        if (current > recordCount)
                        {
                            break;
                        }
                        var start = DateTime.Now;
                        chunkWriter.Write(record);
                        performanceService.IncrementKeyCount("WriteChunk", (DateTime.Now - start).TotalMilliseconds);
                    }
                });
            }

            Console.ReadLine();

            chunkWriter.Close();
            chunkManager.Close();
        }
        static BufferLogRecord ReadRecord(byte[] recordBuffer)
        {
            var record = new BufferLogRecord();
            record.ReadFrom(recordBuffer);
            return record;
        }
        class BufferLogRecord : ILogRecord
        {
            public byte[] RecordBuffer { get; set; }

            public void ReadFrom(byte[] recordBuffer)
            {
                var srcOffset = 0;
                var logPosion = ByteUtils.DecodeLong(recordBuffer, srcOffset, out srcOffset);
                this.RecordBuffer = ByteUtils.DecodeBytes(recordBuffer, srcOffset, out srcOffset);
            }

            public void WriteTo(long logPosition, BinaryWriter writer)
            {
                writer.Write(logPosition);
                writer.Write(RecordBuffer.Length);
                writer.Write(RecordBuffer);
            }
        }
    }
}
