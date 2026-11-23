using Guaxinim.Player;
using NUnit.Framework;

namespace Guaxinim.Tests
{
    /// <summary>GDD §7.5: enche correndo, esvazia andando e quebra após 2 s seguidos de corrida.</summary>
    public class ShakeMeterTests
    {
        private const float Tolerance = 0.001f;

        private static ShakeMeter CreateMeter() => new ShakeMeter(2f, 0.5f);

        [Test]
        public void Tick_OneSecondRunning_IsHalfFull()
        {
            ShakeMeter meter = CreateMeter();

            bool broke = meter.Tick(true, 1f);

            Assert.IsFalse(broke);
            Assert.AreEqual(0.5f, meter.Value, Tolerance);
        }

        [Test]
        public void Tick_TwoSecondsRunning_BreaksAndResets()
        {
            ShakeMeter meter = CreateMeter();
            meter.Tick(true, 1f);

            bool broke = meter.Tick(true, 1f);

            Assert.IsTrue(broke);
            Assert.AreEqual(0f, meter.Value, Tolerance);
        }

        [Test]
        public void Tick_Walking_Drains()
        {
            ShakeMeter meter = CreateMeter();
            meter.Tick(true, 1.5f);

            meter.Tick(false, 1f);

            Assert.AreEqual(0.25f, meter.Value, Tolerance);
        }

        [Test]
        public void Tick_RunWalkRun_NeedsMoreThanTwoSecondsOfRunning()
        {
            ShakeMeter meter = CreateMeter();
            meter.Tick(true, 1.5f);
            meter.Tick(false, 1f);

            bool broke = meter.Tick(true, 0.5f);

            Assert.IsFalse(broke);
        }

        [Test]
        public void Tick_WalkingWhenEmpty_StaysAtZero()
        {
            ShakeMeter meter = CreateMeter();

            meter.Tick(false, 3f);

            Assert.AreEqual(0f, meter.Value, Tolerance);
        }
    }
}
