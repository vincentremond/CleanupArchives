using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CleanupArchives
{
    public partial class Cleaner
    {
        private readonly IFileManager fileManager;
        private readonly ITimeProvider timeProvider;
        private readonly IArchiveTimeStampConverter _archiveTimeStampConverter;

        public Cleaner(IFileManager fileManager, ITimeProvider timeProvider, IArchiveTimeStampConverter archiveTimeStampConverter)
        {
            this.fileManager = fileManager;
            this.timeProvider = timeProvider;
            _archiveTimeStampConverter = archiveTimeStampConverter;
        }

        public void RemoveExtraFiles(IEnumerable<FileInfo> files)
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

            return new DateTime(date.Year, 1, 1);
        }

        public void Clean(string path)
        {
            var valueTuples =
                fileManager
                    .GetFiles(path)
                    .Where(MatchExpectedPattern)
                    .Select(ExtractDates)
                    .Select(EnrichPeriod)
                    .GroupBy(x => x.Period)
                    .SelectMany(AddFileStatus)
                    .ToList();

            var filesToDelete = valueTuples
                .Where(ToDelete)
                .Select(t => t.FileInfo)
                .ToList();

            foreach (var fileToDelete in filesToDelete)
            {
                fileManager.Delete(fileToDelete);
            }
        }

        private bool ToDelete((DateTime Period, FileInfo FileInfo, bool ToDelete) arg) => arg.ToDelete;

        private static IEnumerable<(DateTime Period, FileInfo FileInfo, bool ToDelete)> AddFileStatus(IGrouping<DateTime, (FileInfo FileInfo, DateTime BackupTimeStamp, DateTime Period)> grouping) =>
            grouping
                .OrderByDescending(grp => grp.BackupTimeStamp)
                .Select(
                    (grp, index) => (Period: grouping.Key, grp.FileInfo, ToDelete: index > 0)
                );


        [GeneratedRegex(@"(?<TimeStamp>(\d\d\d\d)(\d\d)(\d\d)_(\d\d)(\d\d)(\d\d))")]
        private static partial Regex TimeStampRegex();

        [GeneratedRegex(@"^VREMOND_D-EDGE_BACKUP_(?<TimeStamp>(\d\d\d\d)(\d\d)(\d\d)_(\d\d)(\d\d)(\d\d))\.7z$")]
        private static partial Regex FileNameRegex();

        
        
        private (FileInfo FileInfo, DateTime BackupTimeStamp) ExtractDates(FileInfo fileInfo)
        {
            var reg = TimeStampRegex();

            var match = reg.Match(fileInfo.FullName);
            if (!match.Success)
            {
                throw new Exception($"Could not parse date from '{fileInfo.FullName}'");
            }

            var m = match.Groups["TimeStamp"].Value;
            var timeStamp = _archiveTimeStampConverter.ConvertToDateTime(m);
            return (fileInfo, timeStamp);
        }

        private (FileInfo FileInfo, DateTime BackupTimeStamp, DateTime Period) EnrichPeriod((FileInfo FileInfo, DateTime BackupTimeStamp) p)
        {
            var period = ReferencePeriod(p.BackupTimeStamp, timeProvider.Now);
            return (p.FileInfo, p.BackupTimeStamp, period);
        }
        
        private static bool MatchExpectedPattern(FileSystemInfo fileInfo) => FileNameRegex().IsMatch(fileInfo.Name);
    }
}