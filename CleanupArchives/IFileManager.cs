using System.Collections.Generic;
using System.IO;

namespace CleanupArchives
{
    public interface IFileManager
    {
        void Delete(FileInfo fileInfo);
        IEnumerable<FileInfo> GetFiles(string path);
    }
}
