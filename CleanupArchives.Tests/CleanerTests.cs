using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using Moq;
using NUnit.Framework;

namespace CleanupArchives.Tests
{
    public class CleanerTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestClean()
        {
            var mockFileManager = new Mock<IFileManager>();
            int count = 0;
            mockFileManager.Setup(f => f.Delete(It.IsAny<string>())).Callback(() => count++);

            var cleaner = new Cleaner(mockFileManager.Object, new FakeTimeProvider(new DateTime(2020, 5, 6, 11, 32, 0)));
            var files = new List<string>
            {
                @"G:\My Drive\BAK\VREMOND_D-EDGE_BACKUP_20200416_103809.7z",
                @"G:\My Drive\BAK\VREMOND_D-EDGE_BACKUP_20200416_103809.7z",
            };
            cleaner.RemoveExtraFiles(files);

            Assert.AreEqual(files.Count, count);
        }

        [Test] public void TestA() => TestPeriod("2017-01-29 11:31:00", "2017-01-01", "2017-07-02 12:00:00");
        [Test] public void TestB() => TestPeriod("2017-01-29 11:31:00", "2017-01-01", "2017-07-02 12:00:00");
        [Test] public void TestC() => TestPeriod("2017-06-05 11:31:00", "2017-01-01", "2020-06-05 11:31:00");
        [Test] public void TestD() => TestPeriod("2017-12-31 11:31:00", "2017-01-01", "2020-06-05 11:31:00");
        [Test] public void TestE() => TestPeriod("2018-06-05 11:31:00", "2018-01-01", "2020-06-05 11:31:00");
        [Test] public void TestF() => TestPeriod("2018-12-31 11:31:00", "2018-01-01", "2020-06-05 11:31:00");
        [Test] public void TestG() => TestPeriod("2019-02-05 11:31:00", "2019-02-01", "2020-06-05 11:31:00");
        [Test] public void TestH() => TestPeriod("2019-03-05 11:31:00", "2019-03-01", "2020-06-05 11:31:00");
        [Test] public void TestI() => TestPeriod("2020-01-05 11:31:00", "2020-01-01", "2020-06-05 11:31:00");
        [Test] public void TestJ() => TestPeriod("2020-02-05 11:31:00", "2020-02-01", "2020-06-05 11:31:00");
        [Test] public void TestK() => TestPeriod("2020-03-05 11:31:00", "2020-03-01", "2020-06-05 11:31:00");
        [Test] public void TestL() => TestPeriod("2020-06-05 11:31:00", "2020-06-05", "2020-06-05 11:31:00");
        [Test] public void TestM() => TestPeriod("2018-04-01 00:00:00", "2018-04-01", "2019-06-05 12:00:00");
		
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
            var cleanOnceResult = CleanOnce(@from, to, path, daySteps);
            var cleanEverydayResult = CleanEveryday(@from, to, path, daySteps);

            // File.WriteAllText($@"D:\TMP\2020.06.05-Clean\{nameof(cleanEverydayResult)}", ObjectDumper.Dump(cleanEverydayResult, DumpStyle.CSharp));
            // File.WriteAllText($@"D:\TMP\2020.06.05-Clean\{nameof(cleanOnceResult)}", ObjectDumper.Dump(cleanOnceResult, DumpStyle.CSharp));

            Assert.AreEqual(cleanEverydayResult, cleanOnceResult);
        }

        private static IEnumerable<string> CleanEveryday(DateTime @from, DateTime to, string path, int daySteps)
        {
            var fakeFileManager = new FakeFileManager();
            var fakeTimeProvider = new FakeTimeProvider();
            var cleaner = new Cleaner(fakeFileManager, fakeTimeProvider);
            for (var current = @from; current <= to; current = current.AddDays(daySteps))
            {
                fakeTimeProvider.Now = current.AddHours(12);
                fakeFileManager.Add($@"{path}\BACKUP_{current:yyyyMMdd_HHmmss}.7z");
                cleaner.Clean(path);
            }

            return fakeFileManager.GetAllFiles();
        }

        private static IEnumerable<string> CleanOnce(DateTime @from, DateTime to, string path, int daySteps)
        {
            var fakeFileManager = new FakeFileManager();
            var fakeTimeProvider = new FakeTimeProvider();
            var cleaner = new Cleaner(fakeFileManager, fakeTimeProvider);
            for (var current = @from; current <= to; current = current.AddDays(daySteps))
            {
                fakeFileManager.Add($@"{path}\BACKUP_{current:yyyyMMdd_HHmmss}.7z");
            }

            fakeTimeProvider.Now = to.AddHours(12);
            cleaner.Clean(path);

            return fakeFileManager.GetAllFiles();
        }

        [Test]
        public void TestParseDateTime()
        {
            var dt = "2020-06-05 11:31:00".ParseDateTime();
            Assert.AreEqual(new DateTime(2020, 6, 5, 11, 31, 0), dt);
        }

        [Test]
        public void TestParseDate()
        {
            var dt = "2020-06-05".ParseDate();
            Assert.AreEqual(new DateTime(2020, 6, 5), dt);
        }
    }

    public class FakeTimeProvider : ITimeProvider
    {
        public FakeTimeProvider(DateTime now)
        {
            Now = now;
        }

        public FakeTimeProvider() : this(DateTime.Now)
        {
        }

        public DateTime Now { get; set; }
    }
}