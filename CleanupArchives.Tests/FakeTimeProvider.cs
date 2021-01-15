using System;

namespace CleanupArchives.Tests
{
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
