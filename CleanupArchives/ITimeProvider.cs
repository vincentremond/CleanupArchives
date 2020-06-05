using System;

namespace CleanupArchives
{
    public interface ITimeProvider
    {
        DateTime Now { get; }
    }
}