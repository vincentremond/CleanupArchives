using System;
using System.Collections.Generic;
using System.IO;

namespace CleanupArchives
{
    internal class FileManager : IFileManager
    {
        public void Delete(string file)
        {
            Console.WriteLine($"Delete {file}");
            File.Delete(file);
        }

        public IEnumerable<string> GetFiles(string path)
        {
            return Directory.GetFiles(path);
        }
    }
}