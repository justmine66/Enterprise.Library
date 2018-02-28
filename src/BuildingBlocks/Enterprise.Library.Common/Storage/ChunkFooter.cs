using Enterprise.Library.Common.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Enterprise.Library.Common.Storage
{
    public class ChunkFooter
    {
        public const int Size = 128;
        public readonly int ChunkDataTotalSize;

        public ChunkFooter(int chunkDataTotalSize)
        {
            Ensure.Nonnegative(chunkDataTotalSize, "chunkDataTotalSize");
            this.ChunkDataTotalSize = chunkDataTotalSize;
        }

        public byte[] AsByteArray()
        {
            var array = new byte[Size];
            using (var stream = new MemoryStream(array))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(this.ChunkDataTotalSize);
                }
            }

            return array;
        }

        public static ChunkFooter FromStream(BinaryReader reader, Stream stream)
        {
            int chunkDataTotalSize = reader.ReadInt32();
            return new ChunkFooter(chunkDataTotalSize);
        }

        public override string ToString()
        {
            return string.Format("[ChunkDataTotalSize: {0}]", this.ChunkDataTotalSize);
        }
    }
}
