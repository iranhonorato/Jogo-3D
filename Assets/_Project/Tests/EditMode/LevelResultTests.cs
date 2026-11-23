using Guaxinim.Core;
using NUnit.Framework;

namespace Guaxinim.Tests
{
    /// <summary>GDD §12: estrelas por lista, tempo-alvo e alerta, e pontos de bônus.</summary>
    public class LevelResultTests
    {
        [Test]
        public void Stars_PerfectRun_IsThree()
        {
            LevelResult result = LevelResult.Create(90f, 120f, true, false, 0, 100);

            Assert.AreEqual(3, result.Stars);
        }

        [Test]
        public void Stars_OverTargetTime_LosesOne()
        {
            LevelResult result = LevelResult.Create(130f, 120f, true, false, 0, 100);

            Assert.IsFalse(result.BeatTargetTime);
            Assert.AreEqual(2, result.Stars);
        }

        [Test]
        public void Stars_ExactlyOnTargetTime_KeepsTimeStar()
        {
            Assert.IsTrue(LevelResult.Create(120f, 120f, true, false, 0, 100).BeatTargetTime);
        }

        [Test]
        public void Stars_WithAlertAndSlow_IsOne()
        {
            LevelResult result = LevelResult.Create(200f, 120f, true, true, 0, 100);

            Assert.AreEqual(1, result.Stars);
        }

        [Test]
        public void BonusPoints_PerItem_AddUp()
        {
            LevelResult result = LevelResult.Create(90f, 120f, true, false, 3, 100);

            Assert.AreEqual(300, result.BonusPoints);
        }
    }
}
