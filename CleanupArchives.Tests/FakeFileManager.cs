using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CleanupArchives.Tests
{
    public class FakeFileManager : IFileManager
    {
        private readonly List<FileInfo> _files = new();

        public void Delete(FileInfo fileInfo) => _files.Remove(fileInfo);
        public IEnumerable<FileInfo> GetFiles(string path) => _files.Where(f => f.FullName.StartsWith(path, StringComparison.CurrentCultureIgnoreCase));
        public void Add(FileInfo file) => _files.Add(file);
        public IEnumerable<FileInfo> GetAllFiles() => _files;
    }
}
