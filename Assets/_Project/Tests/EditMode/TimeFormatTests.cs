using Guaxinim.UI;
using NUnit.Framework;

namespace Guaxinim.Tests
{
    /// <summary>Cronômetro, resultado e mapa mostram o tempo como "mm:ss".</summary>
    public class TimeFormatTests
    {
        [TestCase(0f, "00:00")]
        [TestCase(59.9f, "00:59")]
        [TestCase(90f, "01:30")]
        [TestCase(754f, "12:34")]
        public void MinutesAndSeconds_ReturnsWholeSecondsAsMinutes(float seconds, string expected)
        {
            Assert.AreEqual(expected, TimeFormat.MinutesAndSeconds(seconds));
        }

        [Test]
        public void MinutesAndSeconds_NegativeTime_ReturnsZero()
        {
            Assert.AreEqual("00:00", TimeFormat.MinutesAndSeconds(-5f));
        }
    }
}
