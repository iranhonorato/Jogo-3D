using Guaxinim.Guards;
using NUnit.Framework;

namespace Guaxinim.Tests
{
    /// <summary>GDD §14.6: só as lanternas mais perto do guaxinim ficam acesas.</summary>
    public class LightBudgetTests
    {
        private static readonly float[] SqrDistances = { 25f, 4f, 100f };

        [Test]
        public void IsLit_TwoAllowed_LightsTheTwoClosest()
        {
            Assert.IsTrue(LightBudget.IsLit(0, SqrDistances, 2));
            Assert.IsTrue(LightBudget.IsLit(1, SqrDistances, 2));
            Assert.IsFalse(LightBudget.IsLit(2, SqrDistances, 2));
        }

        [Test]
        public void IsLit_NoneAllowed_LightsNothing()
        {
            Assert.IsFalse(LightBudget.IsLit(1, SqrDistances, 0));
        }

        [Test]
        public void IsLit_TieAtTheLimit_KeepsExactlyTheLimit()
        {
            float[] tied = { 9f, 9f, 9f };

            int lit = 0;
            for (int i = 0; i < tied.Length; i++)
            {
                lit += LightBudget.IsLit(i, tied, 2) ? 1 : 0;
            }

            Assert.AreEqual(2, lit);
        }
    }
}
