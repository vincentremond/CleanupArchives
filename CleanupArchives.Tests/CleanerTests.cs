using System;
using System.Collections.Generic;
using System.IO;
using Moq;
using NUnit.Framework;

namespace CleanupArchives.Tests;

public class CleanerTests
{
    [Test]
    public void TestClean()
    {
        var mockFileManager = new Mock<IFileManager>();
        var count = 0;
        mockFileManager.Setup(f => f.Delete(It.IsAny<FileInfo>())).Callback(() => count++);

        var cleaner = new Cleaner(
            mockFileManager.Object,
            new FakeTimeProvider(
                new DateTime(
                    2020,
                    5,
                    6,
                    11,
                    32,
                    0
                )
            ),
            new ArchiveTimeStampConverter()
        );
        var files = new List<FileInfo>
        {
            new(@"G:\My Drive\BAK\VREMOND_D-EDGE_BACKUP_20200416_103809.7z"),
            new(@"G:\My Drive\BAK\VREMOND_D-EDGE_BACKUP_20200416_103809.7z"),
        };
        cleaner.RemoveExtraFiles(files);

        Assert.That(count, Is.EqualTo(files.Count));
    }

    [Test]
    [TestCase("2017-01-29 11:31:00", "2017-01-01", "2017-07-02 12:00:00")]
    [TestCase("2017-01-29 11:31:00", "2017-01-01", "2017-07-02 12:00:00")]
    [TestCase("2017-06-05 11:31:00", "2017-01-01", "2020-06-05 11:31:00")]
    [TestCase("2017-12-31 11:31:00", "2017-01-01", "2020-06-05 11:31:00")]
    [TestCase("2018-06-05 11:31:00", "2018-01-01", "2020-06-05 11:31:00")]
    [TestCase("2018-12-31 11:31:00", "2018-01-01", "2020-06-05 11:31:00")]
    [TestCase("2019-02-05 11:31:00", "2019-02-01", "2020-06-05 11:31:00")]
    [TestCase("2019-03-05 11:31:00", "2019-03-01", "2020-06-05 11:31:00")]
    [TestCase("2020-01-05 11:31:00", "2020-01-01", "2020-06-05 11:31:00")]
    [TestCase("2020-02-05 11:31:00", "2020-02-01", "2020-06-05 11:31:00")]
    [TestCase("2020-03-05 11:31:00", "2020-03-01", "2020-06-05 11:31:00")]
    [TestCase("2020-06-05 11:31:00", "2020-06-05", "2020-06-05 11:31:00")]
    [TestCase("2018-04-01 00:00:00", "2018-04-01", "2019-06-05 12:00:00")]
    public void TestPeriod(string fileDate, string expectedPeriod, string now)
    {
        var referencePeriod = Cleaner.ReferencePeriod(fileDate.ParseDateTime(), now.ParseDateTime());
        Assert.That(referencePeriod, Is.EqualTo(expectedPeriod.ParseDate()));
    }

    [Test]
    public void TestRolling()
    {
        var from = "2017-01-01".ParseDate();
        var to = "2019-06-05".ParseDate();
        var path = @"D:\TEST\";
        var daySteps = 6;
        var cleanOnceResult = BackupEverydayAndCleanOnce(@from, to, path, daySteps);
        var cleanEverydayResult = BackupEverydayAndCleanEveryday(@from, to, path, daySteps);

        // CollectionAssert.AreEqual(cleanEverydayResult, cleanOnceResult, new FileInfoComparer());
        Assert.That(cleanEverydayResult, Is.EqualTo(cleanOnceResult).Using(new FileInfoComparer()));
    }

    private static IEnumerable<FileInfo> BackupEverydayAndCleanEveryday(DateTime @from, DateTime to, string path, int daySteps)
    {
        var fakeFileManager = new FakeFileManager();
        var fakeTimeProvider = new FakeTimeProvider();
        var archiveTimeStampConverter = new ArchiveTimeStampConverter();
        var cleaner = new Cleaner(fakeFileManager, fakeTimeProvider, archiveTimeStampConverter);
        for (var current = @from; current <= to; current = current.AddDays(daySteps))
        {
            fakeTimeProvider.Now = current.AddHours(12);
            fakeFileManager.Add(new FileInfo($@"{path}\BACKUP_{archiveTimeStampConverter.ConvertToString(current)}.7z"));
            cleaner.Clean(path);
        }

        return fakeFileManager.GetAllFiles();
    }

    private static IEnumerable<FileInfo> BackupEverydayAndCleanOnce(DateTime @from, DateTime to, string path, int daySteps)
    {
        var fakeFileManager = new FakeFileManager();
        var fakeTimeProvider = new FakeTimeProvider();
        var archiveTimeStampConverter = new ArchiveTimeStampConverter();
        var cleaner = new Cleaner(fakeFileManager, fakeTimeProvider, archiveTimeStampConverter);
        for (var current = @from; current <= to; current = current.AddDays(daySteps))
        {
            fakeFileManager.Add(new FileInfo($@"{path}\BACKUP_{archiveTimeStampConverter.ConvertToString(current)}.7z"));
        }

        fakeTimeProvider.Now = to.AddHours(12);
        cleaner.Clean(path);

        return fakeFileManager.GetAllFiles();
    }
}

public class FileInfoComparer : IComparer<FileInfo>
{
    public int Compare(object? x, object? y) => Compare(x as FileInfo, y as FileInfo);

    public int Compare(FileInfo? x, FileInfo? y)
    {
        return (x, y) switch
        {
            (null, null) => 0,
            (null, _) => -1,
            (_, null) => 1,
            _ => string.Compare(x.FullName, y.FullName, StringComparison.Ordinal),
        };
    }
}
