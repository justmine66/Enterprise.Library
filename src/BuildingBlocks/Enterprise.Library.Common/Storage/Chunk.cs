using Enterprise.Library.Common.Components;
using Enterprise.Library.Common.Logging;
using Enterprise.Library.Common.Storage.Exceptions;
using Enterprise.Library.Common.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Enterprise.Library.Common.Storage
{
    public unsafe class Chunk : IDisposable
    {
        #region [ Private Variables ]

        private static readonly ILogger _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(typeof(Chunk));

        private ChunkHeader _chunkHeader;
        private ChunkFooter _chunkFooter;

        private readonly string _filename;
        private readonly ChunkManager _chunkManager;
        private readonly ChunkManagerConfig _chunkConfig;
        private readonly bool _isMemoryChunk;
        private readonly ConcurrentQueue<ReaderWorkItem> _readerWorkItemQueue = new ConcurrentQueue<ReaderWorkItem>();

        private readonly object _writeSyncObj = new object();
        private readonly object _cacheSyncObj = new object();
        private readonly object _freeMemorySyncObj = new object();

        private int _dataPosition;
        private bool _isCompleted;
        private bool _isDestroying;
        private bool _isMemoryFreed;
        private int _cachingChunk;
        private DateTime _lastActiveTime;
        private bool _isReadersInitialized;
        private int _flushedDataPosition;

        private Chunk _memoryChunk;
        private CacheItem[] _cacheItems;
        private IntPtr _cachedData;
        private int _cachedLength;

        private WriterWorkItem _writerWorkItem;

        #endregion

        #region [ Public Properties ]

        public string FileName { get { return _filename; } }
        public ChunkHeader ChunkHeader { get { return _chunkHeader; } }
        public ChunkFooter ChunkFooter { get { return _chunkFooter; } }
        public ChunkManagerConfig Config { get { return _chunkConfig; } }
        public bool IsCompleted { get { return _isCompleted; } }
        public DateTime LastActiveTime
        {
            get
            {
                var lastActiveTimeOfMemoryChunk = DateTime.MinValue;
                if (_memoryChunk != null)
                {
                    lastActiveTimeOfMemoryChunk = _memoryChunk.LastActiveTime;
                }
                return lastActiveTimeOfMemoryChunk >= _lastActiveTime ? lastActiveTimeOfMemoryChunk : _lastActiveTime;
            }
        }
        public bool IsMemoryChunk { get { return _isMemoryChunk; } }
        public bool HasCachedChunk { get { return _memoryChunk != null; } }
        public int DataPosition { get { return _dataPosition; } }
        public long GlobalDataPosition
        {
            get
            {
                return ChunkHeader.ChunkDataStartPosition + DataPosition;
            }
        }
        public bool IsFixedDataSize()
        {
            return _chunkConfig.ChunkDataUnitSize > 0 && _chunkConfig.ChunkDataCount > 0;
        }

        #endregion

        #region [ Constructors ]

        private Chunk(string filename, ChunkManager chunkManager, ChunkManagerConfig chunkConfig, bool isMemoryChunk)
        {
            Ensure.NotNullOrEmpty(filename, "filename");
            Ensure.NotNull(chunkManager, "chunkManager");
            Ensure.NotNull(chunkConfig, "chunkConfig");

            _filename = filename;
            _chunkManager = chunkManager;
            _chunkConfig = chunkConfig;
            _isMemoryChunk = isMemoryChunk;
            _lastActiveTime = DateTime.Now;
        }
        ~Chunk()
        {
            this.UnCacheFromMemory();
        }

        #endregion

        #region [ Public Methods ]

        public bool TryCacheInMemory(bool shouldCacheNextChunk)
        {
            lock (_cacheSyncObj)
            {
                if (!_chunkConfig.EnableCache || _isMemoryChunk || !_isCompleted || _memoryChunk != null)
                {
                    _cachingChunk = 0;
                    return false;
                }

                try
                {
                    IList<Chunk> cachedFileChunks = _chunkManager.GetCachedFileChunks();
                    if (cachedFileChunks.Count >= _chunkConfig.ChunkCacheMaxCount)
                    {
                        return false;
                    }

                }
                catch (Exception exc)
                {

                }
            }
        }
        private bool UnCacheFromMemory()
        {
            lock (_cacheSyncObj)
            {
                if (!_chunkConfig.EnableCache || _isMemoryChunk || !_isCompleted || _memoryChunk == null)
                {
                    return false;
                }

                try
                {
                    var memoryChunk = _memoryChunk;
                    _memoryChunk = null;
                    memoryChunk.Dispose();
                    return true;
                }
                catch (Exception exc)
                {
                    _logger.Error(string.Format("Failed to uncache completed chunk {0}.", this), exc);
                    return false;
                }
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region [ Factory Methods ]
        public static Chunk CreateNew(string filename, int chunkNumber, ChunkManager manager, ChunkManagerConfig config, bool isMemoryChunk)
        {
            var chunk = new Chunk(filename, manager, config, isMemoryChunk);
            try
            {
                chunk.InitNew(chunkNumber);
            }
            catch (OutOfMemoryException)
            {
                chunk.Dispose();
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Chunk {0} create failed.", chunk), ex);
                chunk.Dispose();
                throw;
            }

            return chunk;
        }
        public static Chunk FromCompletedFile(string filename, ChunkManager chunkManager, ChunkManagerConfig config, bool isMemoryChunk)
        {
            var chunk = new Chunk(filename, chunkManager, config, isMemoryChunk);

            try
            {
                chunk.InitCompleted();
            }
            catch (OutOfMemoryException)
            {
                chunk.Dispose();
                throw;
            }
            catch (Exception exc)
            {
                _logger.Error(string.Format("Chunk {0} init from completed file failed.", chunk), exc);
                chunk.Dispose();
                throw;
            }

            return chunk;
        }
        #endregion

        #region [ Init Methods ]

        private void InitCompleted()
        {
            var fileInfo = new FileInfo(_filename);
            if (!fileInfo.Exists)
            {
                throw new ChunkFileNotExistException(_filename);
            }

            _isCompleted = true;

            using (var fileStream = new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, _chunkConfig.ChunkReadBuffer, FileOptions.None))
            {
                using (var reader = new BinaryReader(fileStream))
                {
                    _chunkHeader = this.ReadHeader(fileStream, reader);
                    _chunkFooter = this.ReadFooter(fileStream, reader);

                    this.CheckCompletedFileChunk();
                }
            }

            _dataPosition = _chunkFooter.ChunkDataTotalSize;
            _flushedDataPosition = _chunkFooter.ChunkDataTotalSize;

            if (_isMemoryChunk)
            {
                this.LoadFileChunkToMemory();
            }
            else
            {
                this.SetFileAttributes();
            }

            this.InitializeReaderWorkItems();
            _lastActiveTime = DateTime.Now;
        }

        private void InitNew(int chunkNumber)
        {
            int chunkDataSize = 0;
            if (_chunkConfig.ChunkDataSize > 0)
            {
                chunkDataSize = _chunkConfig.ChunkDataSize;
            }
            else
            {
                chunkDataSize = _chunkConfig.ChunkDataUnitSize * _chunkConfig.ChunkDataCount;
            }

            _chunkHeader = new ChunkHeader(chunkNumber, chunkDataSize);

            _isCompleted = false;

            int fileSize = ChunkHeader.Size + _chunkHeader.ChunkDataTotalSize + ChunkFooter.Size;

            var writeStream = default(Stream);
            var tempFilename = string.Format("{0}.{1}.tmp", _filename, Guid.NewGuid());
            var tempFileStream = default(FileStream);

            try
            {
                if (_isMemoryChunk)
                {
                    _cachedLength = fileSize;
                    _cachedData = Marshal.AllocHGlobal(_cachedLength);
                    writeStream = new UnmanagedMemoryStream((byte*)_cachedData, _cachedLength);
                    writeStream.Write(_chunkHeader.AsByteArray(), 0, ChunkHeader.Size);
                }
                else
                {
                    var fileInfo = new FileInfo(_filename);
                    if (fileInfo.Exists)
                    {
                        File.SetAttributes(_filename, FileAttributes.Normal);
                        File.Delete(_filename);
                    }

                    tempFileStream = new FileStream(tempFilename, FileMode.CreateNew, FileAccess.Read, FileShare.ReadWrite, _chunkConfig.ChunkWriteBuffer, FileOptions.None);
                    tempFileStream.SetLength(fileSize);
                    tempFileStream.Write(_chunkHeader.AsByteArray(), 0, ChunkHeader.Size);
                    tempFileStream.Flush(true);
                    tempFileStream.Close();

                    File.Move(tempFilename, _filename);
                }

                writeStream.Position = ChunkHeader.Size;

                _dataPosition = 0;
                _flushedDataPosition = 0;
                _writerWorkItem = new WriterWorkItem(new ChunkFileStream(writeStream, _chunkConfig.FlushOption));

                this.InitializeReaderWorkItems();

            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region [ Helper Methods ]
        private void InitializeReaderWorkItems()
        {
            for (var i = 0; i < _chunkConfig.ChunkReaderCount; i++)
            {
                _readerWorkItemQueue.Enqueue(CreateReaderWorkItem());
            }
            _isReadersInitialized = true;
        }
        private ReaderWorkItem CreateReaderWorkItem()
        {
            var stream = default(Stream);
            if (_isMemoryChunk)
            {
                stream = new UnmanagedMemoryStream((byte*)_cachedData, _cachedLength);
            }
            else
            {
                stream = new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, _chunkConfig.ChunkReadBuffer, FileOptions.None);
            }

            return new ReaderWorkItem(stream, new BinaryReader(stream));
        }
        private void SetFileAttributes()
        {
            Helper.EatException(() => File.SetAttributes(_filename, FileAttributes.NotContentIndexed));
        }
        private void CheckCompletedFileChunk()
        {
            using (var fileStream = new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, _chunkConfig.ChunkReadBuffer, FileOptions.None))
            {
                //检查Chunk文件的实际大小是否正确
                int chunkFileSize = ChunkHeader.Size + _chunkFooter.ChunkDataTotalSize + ChunkFooter.Size;
                if (chunkFileSize != fileStream.Length)
                {
                    throw new ChunkBadDataException(
                        string.Format("The size of chunk {0} should be equals with fileStream's length {1}, but instead it was {2}.",
                                        this,
                                        fileStream.Length,
                                        chunkFileSize));
                }

                //如果Chunk中的数据是固定大小的，则还需要检查数据总数是否正确
                if (this.IsFixedDataSize())
                {
                    if (_chunkFooter.ChunkDataTotalSize != _chunkHeader.ChunkDataTotalSize)
                    {
                        throw new ChunkBadDataException(
                            string.Format("For fixed-size chunk, the total data size of chunk {0} should be {1}, but instead it was {2}.",
                                            this,
                                            _chunkHeader.ChunkDataTotalSize,
                                            _chunkFooter.ChunkDataTotalSize));
                    }
                }
            }
        }
        private void LoadFileChunkToMemory()
        {
            using (var fileStream = new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 8192, FileOptions.None))
            {
                var cachedLength = (int)fileStream.Length;
                var cachedData = Marshal.AllocHGlobal(cachedLength);

                try
                {
                    using (var unmanagedStream = new UnmanagedMemoryStream((byte*)cachedData, cachedLength, cachedLength, FileAccess.ReadWrite))
                    {
                        fileStream.Seek(0, SeekOrigin.Begin);
                        var buffer = new byte[65536];
                        int toRead = cachedLength;
                        while (toRead > 0)
                        {
                            int read = fileStream.Read(buffer, 0, Math.Min(toRead, buffer.Length));
                            if (read == 0)
                            {
                                break;
                            }
                            toRead -= read;
                            unmanagedStream.Write(buffer, 0, read);
                        }
                    }
                }
                catch
                {
                    Marshal.FreeHGlobal(cachedData);
                    throw;
                }

                _cachedData = cachedData;
                _cachedLength = cachedLength;
            }
        }
        private ChunkHeader ReadHeader(FileStream stream, BinaryReader reader)
        {
            if (stream.Length < ChunkHeader.Size)
            {
                throw new Exception(string.Format("Chunk file '{0}' is too short to even read ChunkHeader, its size is {1} bytes.", _filename, stream.Length));
            }

            stream.Seek(0, SeekOrigin.Begin);
            return ChunkHeader.FromStream(reader, stream);
        }
        private ChunkFooter ReadFooter(FileStream stream, BinaryReader reader)
        {
            if (stream.Length < ChunkFooter.Size)
            {
                throw new Exception(string.Format("Chunk file '{0}' is too short to even read ChunkFooter, its size is {1} bytes.", _filename, stream.Length));
            }
            stream.Seek(-ChunkFooter.Size, SeekOrigin.End);
            return ChunkFooter.FromStream(reader, stream);
        }
        #endregion

        class CacheItem
        {
            public long RecordPosition;
            public byte[] RecordBuffer;
        }

        class ChunkFileStream : IStream
        {
            public Stream Stream;
            public FlushOption FlushOption;

            public ChunkFileStream(Stream stream, FlushOption flushOption)
            {
                Stream = stream;
                FlushOption = flushOption;
            }

            public long Length => this.Stream.Length;

            public long Position { get => this.Stream.Position; set => this.Stream.Position = value; }

            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public void Flush()
            {
                var fileStream = this.Stream as FileStream;
                if (fileStream != null)
                {
                    if (this.FlushOption == FlushOption.FlushToDisk)
                    {
                        fileStream.Flush(true);
                    }
                    else
                    {
                        fileStream.Flush();
                    }
                }
                else
                {
                    this.Stream.Flush();
                }
            }

            public void SetLength(long value)
            {
                this.Stream.SetLength(value);
            }

            public void Write(byte[] buffer, int offset, int count)
            {
                this.Stream.Write(buffer, offset, count);
            }
        }

        public override string ToString()
        {
            return string.Format("({0}-#{1})", _chunkManager.Name, _chunkHeader.ChunkNumber);
        }
    }
}
