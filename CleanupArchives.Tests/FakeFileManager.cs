using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;

namespace CleanupArchives.Tests
{
    public class FakeFileManager : IFileManager
    {
        private readonly List<string> files;
        public FakeFileManager() => files = new List<string>();
        public void Delete(string file) => files.Remove(file);
        public IEnumerable<string> GetFiles(string path) => files.Where(f => f.StartsWith(path, StringComparison.CurrentCultureIgnoreCase));
        public IEnumerable<string> GetAllFiles() => files;
        public void Add(string file) => files.Add(file);
    }
}