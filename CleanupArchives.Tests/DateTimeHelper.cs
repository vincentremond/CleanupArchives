using System;
using System.Globalization;

namespace CleanupArchives.Tests
{
    public static class DateTimeHelper
    {
        public static DateTime ParseDateTime(this string fileDateStr)
        {
            return DateTime.ParseExact(fileDateStr, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        public static DateTime ParseDate(this string fileDateStr)
        {
            return DateTime.ParseExact(fileDateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
    }
}