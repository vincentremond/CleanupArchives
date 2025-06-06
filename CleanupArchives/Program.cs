namespace CleanupArchives;

internal class Program
{
    private static void Main()
    {
        var paths = new[]
        {
            @"D:\BAK",
            @"G:\My Drive\BAK",
        };
        foreach (var path in paths)
        {
            var cleaner = new Cleaner(new FileManager(), new TimeProvider(), new ArchiveTimeStampConverter());
            cleaner.Clean(path);
        }
    }
}
