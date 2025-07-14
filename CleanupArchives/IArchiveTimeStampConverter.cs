using System;
using System.Globalization;

namespace CleanupArchives;

public interface IArchiveTimeStampConverter
{
    string ConvertToString(DateTime dateTime);
    DateTime ConvertToDateTime(string str);
}

public class ArchiveTimeStampConverter : IArchiveTimeStampConverter
{
    private const string _dateFormat = "yyyyMMdd_HHmmss";
    public string ConvertToString(DateTime dateTime) => dateTime.ToString(_dateFormat);
    public DateTime ConvertToDateTime(string str) => DateTime.ParseExact(str, @"yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
}