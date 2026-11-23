using Guaxinim.Effects;
using Guaxinim.Player;
using NUnit.Framework;

namespace Guaxinim.Tests
{
    /// <summary>GDD §8.1: energético e proteína, juntos, com duração e renovação.</summary>
    public class ConsumableEffectTests
    {
        private const float Tolerance = 0.001f;

        private static EnergyDrinkEffect EnergyDrink() => new EnergyDrinkEffect(8f, 1.5f);

        private static ProteinBarEffect ProteinBar() => new ProteinBarEffect(15f, 5f, 0.5f);

        [Test]
        public void NoEffects_ModifiersAreNeutral()
        {
            var active = new ActiveEffects();

            Assert.AreEqual(1f, active.SpeedMultiplier, Tolerance);
            Assert.AreEqual(0f, active.MaxWeightBonus, Tolerance);
            Assert.AreEqual(1f, active.WeightPenaltyScale, Tolerance);
        }

        [Test]
        public void BothEffects_CombineWithoutOverwriting()
        {
            var active = new ActiveEffects();

            active.Add(EnergyDrink());
            active.Add(ProteinBar());

            Assert.AreEqual(1.5f, active.SpeedMultiplier, Tolerance);
            Assert.AreEqual(5f, active.MaxWeightBonus, Tolerance);
            Assert.AreEqual(0.5f, active.WeightPenaltyScale, Tolerance);
        }

        [Test]
        public void Tick_PastDuration_RemovesEffect()
        {
            var active = new ActiveEffects();
            active.Add(EnergyDrink());

            bool anyFinished = active.Tick(8.1f);

            Assert.IsTrue(anyFinished);
            Assert.AreEqual(0, active.Effects.Count);
            Assert.AreEqual(1f, active.SpeedMultiplier, Tolerance);
        }

        [Test]
        public void Add_SameTypeAgain_RefreshesInsteadOfStacking()
        {
            var active = new ActiveEffects();
            active.Add(EnergyDrink());
            active.Tick(6f);

            active.Add(EnergyDrink());

            Assert.AreEqual(1, active.Effects.Count);
            Assert.AreEqual(8f, active.Effects[0].RemainingSeconds, Tolerance);
            Assert.AreEqual(1.5f, active.SpeedMultiplier, Tolerance);
        }

        [Test]
        public void ProteinBar_WithWatermelon_HalvesSpeedLoss()
        {
            // GDD §8.1: base × (1 − 0,25 × peso / pesoMáximo), com o peso máximo em 15.
            float speed = CarrySpeed.Calculate(6f, 6f, 15f, 0.5f * 0.5f);

            Assert.AreEqual(5.4f, speed, Tolerance);
        }
    }
}
