using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Library.Common.Storage.FileNamingStrategies
{
    public interface IFileNamingStrategy
    {
        string GetFileNameFor(string path, int index);
        string[] GetChunkFiles(string path);
        string[] GetTempFiles(string path);
    }
}
