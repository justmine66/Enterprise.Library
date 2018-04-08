using Enterprise.Library.Common.Components;
using Enterprise.Library.Common.Logging;
using Enterprise.Library.Common.Storage;
using Enterprise.Library.Common.Storage.FileNamingStrategies;
using System;
using System.Collections.Generic;
using System.Net;

namespace Enterprise.Library.EventStore
{
    /// <summary>
    /// 事件存储的默认实现类
    /// </summary>
    public class DefaultEventStore : IEventStore
    {
        private readonly object _lockObj = new object();
        private readonly IPAddress _ipaddress = IPAddress.Loopback;
        private readonly ILogger _logger;
        private ChunkManager _chunkManager;
        private ChunkWriter _chunkwriter;
        private int _Port = 10098;
        public DefaultEventStore()
        {
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(typeof(DefaultEventStore));
        }
        public int ChunkCount => _chunkManager.GetChunkCount();

        public int MinChunkNum => _chunkManager.GetFirstChunk().ChunkHeader.ChunkDataTotalSize;

        public int MaxChunkNum => _chunkManager.GetLastChunk().ChunkHeader.ChunkDataTotalSize;

        public EventAppendStatus AppendStream(EventStream stream)
        {
            lock (_lockObj)
            {
                var record = new StreamLogRecord(_ipaddress, _Port)
                {
                    SourceId = stream.SourceId,
                    Name = stream.Name,
                    Version = stream.Version,
                    Events = stream.Events,
                    CommandId = stream.CommandId,
                    Timestamp = stream.Timestamp,
                    Items = stream.Items
                };
                _chunkwriter.Write(record);
                return EventAppendStatus.Success;
            }
        }

        public EventAppendStatus AppendStreams(IEnumerable<EventStream> streams)
        {
            //todo
            return EventAppendStatus.Success;
        }

        public void Load()
        {
            var path = @"c:\estore-files\event-chunks";
            var chunkDataSize = 512 * 1024 * 1024;
            var maxLogRecordSize = 4 * 1024 * 1024;
            var config = new ChunkManagerConfig(
                path,
                new DefaultFileNamingStrategy("event-chunk-"),
                chunkDataSize,
                0,
                0,
                100,
                false,
                false,
                FlushOption.FlushToOS,
                Environment.ProcessorCount * 2,
                maxLogRecordSize,
                128 * 1024,
                128 * 1024,
                90,
                45,
                1,
                5,
                1000000,
                false);
            _chunkManager = new ChunkManager("EventChunk", config, false);
            _chunkwriter = new ChunkWriter(_chunkManager);
        }

        public void Shutdown()
        {
            _chunkwriter.Close();
            _chunkManager.Close();
        }

        public void Start()
        {
            _chunkwriter.Open();
        }
    }
}
