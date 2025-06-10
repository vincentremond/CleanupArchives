using System;

namespace CleanupArchives;

internal class TimeProvider : ITimeProvider
{
    DateTime ITimeProvider.Now => DateTime.Now;
}