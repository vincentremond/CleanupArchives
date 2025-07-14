using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CleanupArchives;

internal class FileManager : IFileManager
{
    public void Delete(FileInfo fileInfo)
    {
        Console.WriteLine($"Delete {fileInfo}");
        fileInfo.Delete();
    }

    public IEnumerable<FileInfo> GetFiles(string path) => Directory.GetFiles(path).Select(f => new FileInfo(f));
}