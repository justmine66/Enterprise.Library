using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Enterprise.Library.Common.Storage
{
    public class ReaderWorkItem
    {
        public readonly Stream Stream;
        public readonly BinaryReader Reader;

        public ReaderWorkItem(Stream stream, BinaryReader reader)
        {
            this.Stream = stream;
            this.Reader = reader;
        }
    }
}
