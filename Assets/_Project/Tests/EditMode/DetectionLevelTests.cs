using Guaxinim.Guards;
using NUnit.Framework;

namespace Guaxinim.Tests
{
    /// <summary>GDD §6.3: enche em 1 s a 9 m, mais rápido de perto, e esvazia devagar.</summary>
    public class DetectionLevelTests
    {
        private const float Tolerance = 0.001f;
        private const float Range = 9f;

        private static DetectionLevel CreateLevel() => new DetectionLevel(1f, 3f, 0.35f);

        [Test]
        public void Tick_OneSecondAtMaxRange_BecomesFull()
        {
            DetectionLevel level = CreateLevel();

            bool becameFull = level.Tick(true, Range, Range, 1f);

            Assert.IsTrue(becameFull);
            Assert.AreEqual(1f, level.Value, Tolerance);
        }

        [Test]
        public void Tick_HalfSecondAtMaxRange_IsHalfFull()
        {
            DetectionLevel level = CreateLevel();

            level.Tick(true, Range, Range, 0.5f);

            Assert.AreEqual(0.5f, level.Value, Tolerance);
        }

        [Test]
        public void GetFillRate_AtZeroDistance_IsThreeTimesFaster()
        {
            Assert.AreEqual(3f, CreateLevel().GetFillRate(0f, Range), Tolerance);
        }

        [Test]
        public void GetFillRate_HalfwayIn_IsTwiceAsFast()
        {
            Assert.AreEqual(2f, CreateLevel().GetFillRate(4.5f, Range), Tolerance);
        }

        [Test]
        public void Tick_NotSeen_DrainsSlowly()
        {
            DetectionLevel level = CreateLevel();
            level.Tick(true, Range, Range, 1f);

            level.Tick(false, 0f, Range, 1f);

            Assert.AreEqual(0.65f, level.Value, Tolerance);
        }

        [Test]
        public void Tick_AlreadyFull_ReportsFullOnlyOnce()
        {
            DetectionLevel level = CreateLevel();
            level.Tick(true, Range, Range, 1f);

            bool becameFullAgain = level.Tick(true, Range, Range, 1f);

            Assert.IsFalse(becameFullAgain);
            Assert.AreEqual(1f, level.Value, Tolerance);
        }

        [Test]
        public void Tick_EmptyAndNotSeen_StaysAtZero()
        {
            DetectionLevel level = CreateLevel();

            level.Tick(false, 0f, Range, 5f);

            Assert.AreEqual(0f, level.Value, Tolerance);
        }

        [Test]
        public void Reset_AfterFilling_ReturnsToZero()
        {
            DetectionLevel level = CreateLevel();
            level.Tick(true, Range, Range, 1f);

            level.Reset();

            Assert.AreEqual(0f, level.Value, Tolerance);
            Assert.IsFalse(level.IsFull);
        }
    }
}
