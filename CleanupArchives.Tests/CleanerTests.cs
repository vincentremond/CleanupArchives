using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;

namespace CleanupArchives.Tests
{
    public class CleanerTests
    {
        [Test]
        public void TestClean()
        {
            var mockFileManager = new Mock<IFileManager>();
            var count = 0;
            mockFileManager.Setup(f => f.Delete(It.IsAny<string>())).Callback(() => count++);

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
            var files = new List<string>
            {
                @"G:\My Drive\BAK\VREMOND_D-EDGE_BACKUP_20200416_103809.7z",
                @"G:\My Drive\BAK\VREMOND_D-EDGE_BACKUP_20200416_103809.7z",
            };
            cleaner.RemoveExtraFiles(files);

            Assert.AreEqual(files.Count, count);
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
            Assert.AreEqual(expectedPeriod.ParseDate(), referencePeriod);
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

            Assert.AreEqual(cleanEverydayResult, cleanOnceResult);
        }

        private static IEnumerable<string> BackupEverydayAndCleanEveryday(DateTime @from, DateTime to, string path, int daySteps)
        {
            var fakeFileManager = new FakeFileManager();
            var fakeTimeProvider = new FakeTimeProvider();
            var archiveTimeStampConverter = new ArchiveTimeStampConverter();
            var cleaner = new Cleaner(fakeFileManager, fakeTimeProvider, archiveTimeStampConverter);
            for (var current = @from; current <= to; current = current.AddDays(daySteps))
            {
                fakeTimeProvider.Now = current.AddHours(12);
                fakeFileManager.Add($@"{path}\BACKUP_{archiveTimeStampConverter.ConvertToString(current)}.7z");
                cleaner.Clean(path);
            }

            return fakeFileManager.GetAllFiles();
        }

        private static IEnumerable<string> BackupEverydayAndCleanOnce(DateTime @from, DateTime to, string path, int daySteps)
        {
            var fakeFileManager = new FakeFileManager();
            var fakeTimeProvider = new FakeTimeProvider();
            var archiveTimeStampConverter = new ArchiveTimeStampConverter();
            var cleaner = new Cleaner(fakeFileManager, fakeTimeProvider, archiveTimeStampConverter);
            for (var current = @from; current <= to; current = current.AddDays(daySteps))
            {
                fakeFileManager.Add($@"{path}\BACKUP_{archiveTimeStampConverter.ConvertToString(current)}.7z");
            }

            fakeTimeProvider.Now = to.AddHours(12);
            cleaner.Clean(path);

            return fakeFileManager.GetAllFiles();
        }

    }
}
