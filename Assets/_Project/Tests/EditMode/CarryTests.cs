using Guaxinim.Player;
using NUnit.Framework;

namespace Guaxinim.Tests
{
    /// <summary>GDD §4 e §7.3: limites de carga e velocidade com peso.</summary>
    public class CarryTests
    {
        private const float Tolerance = 0.001f;
        private const float MaxWeight = 10f;
        private const float MaxVolume = 6f;
        private const float Penalty = 0.5f;

        [Test]
        public void Calculate_WithoutWeight_KeepsBaseSpeed()
        {
            Assert.AreEqual(6f, CarrySpeed.Calculate(6f, 0f, MaxWeight, Penalty), Tolerance);
        }

        [Test]
        public void Calculate_WithMaxWeight_ReturnsHalfSpeed()
        {
            Assert.AreEqual(3f, CarrySpeed.Calculate(6f, 10f, MaxWeight, Penalty), Tolerance);
        }

        [Test]
        public void Calculate_WithWatermelon_RunsSlowerThanChasingGuard()
        {
            float speed = CarrySpeed.Calculate(6f, 6f, MaxWeight, Penalty);

            Assert.AreEqual(4.2f, speed, Tolerance);
            Assert.Less(speed, 5.5f); // GDD §6.5: com carga ele é pego; fugir obriga a largar itens
        }

        [Test]
        public void Calculate_WithZeroMaxWeight_KeepsBaseSpeed()
        {
            Assert.AreEqual(6f, CarrySpeed.Calculate(6f, 5f, 0f, Penalty), Tolerance);
        }

        [Test]
        public void CanCarry_ExactlyAtLimits_ReturnsTrue()
        {
            Assert.IsTrue(CarryCapacity.CanCarry(4f, 1f, 6f, 5f, MaxWeight, MaxVolume));
        }

        [Test]
        public void CanCarry_OverWeight_ReturnsFalse()
        {
            Assert.IsFalse(CarryCapacity.CanCarry(5f, 0f, 6f, 2f, MaxWeight, MaxVolume));
        }

        [Test]
        public void CanCarry_OverVolume_ReturnsFalse()
        {
            Assert.IsFalse(CarryCapacity.CanCarry(1f, 4f, 2f, 5f, MaxWeight, MaxVolume));
        }
    }
}
