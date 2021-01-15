using System;
using NUnit.Framework;

namespace CleanupArchives.Tests
{
    public class DateTimeHelperTests
    {
        [Test]
        public void TestParseDateTime()
        {
            var dt = "2020-06-05 11:31:00".ParseDateTime();
            Assert.AreEqual(
                new DateTime(
                    2020,
                    6,
                    5,
                    11,
                    31,
                    0
                ),
                dt
            );
        }

        [Test]
        public void TestParseDate()
        {
            var dt = "2020-06-05".ParseDate();
            Assert.AreEqual(new DateTime(2020, 6, 5), dt);
        }
    }
}
