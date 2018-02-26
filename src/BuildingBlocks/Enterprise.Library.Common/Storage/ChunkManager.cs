using Enterprise.Library.Common.Components;
using Enterprise.Library.Common.Logging;
using Enterprise.Library.Common.Scheduling;
using Enterprise.Library.Common.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Enterprise.Library.Common.Storage
{
    public class ChunkManager : IDisposable
    {
        private static readonly ILogger _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(typeof(ChunkManager));
        private readonly object _lockObj = new object();
        private readonly ChunkManagerConfig _config;
        private readonly IDictionary<int, Chunk> _chunks;
        private readonly string _chunkPath;
        private readonly IScheduleService _scheduleService;
        private readonly bool _isMemoryMode;
        private int _nextChunkNumber;
        private int _uncachingChunks;
        private int _isCachingNextChunk;
        private ConcurrentDictionary<int, BytesInfo> _bytesWriteDict;
        private ConcurrentDictionary<int, CountInfo> _fileReadDict;
        private ConcurrentDictionary<int, CountInfo> _unmanagedReadDict;
        private ConcurrentDictionary<int, CountInfo> _cachedReadDict;

        class BytesInfo
        {
            public long PreviousBytes;
            public long CurrentBytes;

            public long UpgradeBytes()
            {
                var incrementBytes = CurrentBytes - PreviousBytes;
                PreviousBytes = CurrentBytes;
                return incrementBytes;
            }
        }

        class CountInfo
        {
            public long PreviousCount;
            public long CurrentCount;

            public long UpgradeCount()
            {
                var incrementCount = CurrentCount - PreviousCount;
                PreviousCount = CurrentCount;
                return incrementCount;
            }
        }

        public ChunkManager(string name, ChunkManagerConfig config, bool isMemoryMode, IEnumerable<string> relativePaths = null)
        {
            Ensure.NotNull(name, "name");
            Ensure.NotNull(config, "config");

            this.Name = name;
            _config = config;
            _isMemoryMode = isMemoryMode;
            if (relativePaths == null)
            {
                _chunkPath = _config.BasePath;
            }
            else
            {
                var chunkPath = _config.BasePath;
                foreach (var relativePath in relativePaths)
                {
                    chunkPath = Path.Combine(chunkPath, relativePath);
                }
                _chunkPath = chunkPath;
            }
            if (!Directory.Exists(_chunkPath))
            {
                Directory.CreateDirectory(_chunkPath);
            }
            _chunks = new ConcurrentDictionary<int, Chunk>();
            _scheduleService = ObjectContainer.Resolve<IScheduleService>();
            _bytesWriteDict = new ConcurrentDictionary<int, BytesInfo>();
            _fileReadDict = new ConcurrentDictionary<int, CountInfo>();
            _unmanagedReadDict = new ConcurrentDictionary<int, CountInfo>();
            _cachedReadDict = new ConcurrentDictionary<int, CountInfo>();
        }

        public void Load<T>(Func<byte[], T> readRecordFunc) where T : ILogRecord
        {
            if (_isMemoryMode) return;

            lock (_lockObj)
            {
                if (!Directory.Exists(_chunkPath))
                {
                    Directory.CreateDirectory(_chunkPath);
                }

                string[] tempFiles = _config.FileNamingStrategy.GetTempFiles(_chunkPath);
                if (tempFiles != null && tempFiles.Length > 0)
                {
                    foreach (var file in tempFiles)
                    {
                        File.SetAttributes(file, FileAttributes.Normal);
                        File.Delete(file);
                    }
                }

                var files = _config.FileNamingStrategy.GetChunkFiles(_chunkPath);
                if (files != null && files.Length > 0)
                {

                }
            }
        }

        public string Name { get; private set; }
        public ChunkManagerConfig Config { get { return _config; } }
        public string ChunkPath { get { return _chunkPath; } }
        public bool IsMemoryMode { get { return _isMemoryMode; } }

        public IList<Chunk> GetCachedFileChunks(bool checkIsCompleted = false, bool checkInactive = false)
        {
            return _chunks.Values.Where(x => (!checkIsCompleted || x.IsCompleted) && !x.IsMemoryChunk && x.HasCachedChunk && (!checkInactive || (DateTime.Now - x.LastActiveTime).TotalSeconds >= _config.ChunkInactiveTimeMaxSeconds)).OrderBy(x => x.LastActiveTime).ToList();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
