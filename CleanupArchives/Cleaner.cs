using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace CleanupArchives
{
    public class Cleaner
    {
        private readonly IFileManager fileManager;
        private readonly ITimeProvider timeProvider;

        public Cleaner(IFileManager fileManager, ITimeProvider timeProvider)
        {
            this.fileManager = fileManager;
            this.timeProvider = timeProvider;
        }

        public void RemoveExtraFiles(IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                Console.WriteLine($@"Removing {file}");
                fileManager.Delete(file);
            }
        }

        public static DateTime ReferencePeriod(DateTime date, DateTime now)
        {
            // Keep all from current month and previous month
            var previousMonth = new DateTime(now.Year, now.Month, 1).AddMonths(-1);

            if (date > previousMonth)
            {
                return date.Date;
            }

            // Keep one by month for 3 months before that
            var someMonths = new DateTime(now.Year, 1, 1).AddYears(-1); // previousMonth.AddMonths(-3);
            if (date > someMonths)
            {
                return new DateTime(date.Year, date.Month, 1);
            }

            // // Some semesters
            // var semesterStart = SemesterStart(someMonths);
            // for (int i = 0; i >= -12; i -= 3)
            // {
            //     var start = semesterStart.AddMonths(i);
            //     if (date > start)
            //     {
            //         return start;
            //     }
            // }

            return new DateTime(date.Year, 1, 1);
        }

        private static DateTime SemesterStart(in DateTime someMonths)
        {
            return new DateTime(someMonths.Year, someMonths.Month switch
            {
                1 => 1,
                2 => 1,
                3 => 1,
                4 => 4,
                5 => 4,
                6 => 4,
                7 => 7,
                8 => 7,
                9 => 7,
                10 => 10,
                11 => 10,
                12 => 10,
            }, 1);
        }

        public void Clean(string path)
        {
            var valueTuples = fileManager.GetFiles(path)
                .Select(ExtractDates)
                .Select(EnrichPeriod)
                .GroupBy(x => x.Period)
                .SelectMany(AddFileStatus)
                .ToList();

            var filesToDelete = valueTuples
                .Where(ToDelete)
                .Select(t => t.Path)
                .ToList();

            foreach (var fileToDelete in filesToDelete)
            {
                fileManager.Delete(fileToDelete);
            }
        }

        private bool ToDelete((DateTime Period, string Path, bool ToDelete) arg)
        {
            return arg.ToDelete;
        }

        private static IEnumerable<(DateTime Period, string Path, bool ToDelete)> AddFileStatus(IGrouping<DateTime, (string Path, DateTime Time, DateTime Period)> grouping)
        {
            return grouping
                .OrderByDescending(grp => grp.Time)
                .Select(
                    (grp, index) => (Period: grouping.Key, Path: grp.Path, ToDelete: index > 0)
                );
        }

        private (string Path, DateTime Time) ExtractDates(string path)
        {
            var reg = new Regex(@"(?<TimeStamp>(\d\d\d\d)(\d\d)(\d\d)_(\d\d)(\d\d)(\d\d))");
            var m = reg.Match(path).Groups["TimeStamp"].Value;
            var time = DateTime.ParseExact(m, @"yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            return (path, time);
        }

        private (string Path, DateTime Time, DateTime Period) EnrichPeriod((string Path, DateTime Time) arg)
        {
            var period = ReferencePeriod(arg.Time, timeProvider.Now);
            return (arg.Path, arg.Time, period);
        }
    }
}