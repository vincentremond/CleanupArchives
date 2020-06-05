using System.Collections.Generic;

namespace CleanupArchives
{
    public interface IFileManager
    {
        void Delete(string file);
        IEnumerable<string> GetFiles(string path);
    }
}