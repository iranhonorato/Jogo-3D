using Guaxinim.Guards;
using NUnit.Framework;

namespace Guaxinim.Tests
{
    /// <summary>GDD §6.5: pego a menos de 1 m em Perseguição, a não ser que intocável.</summary>
    public class CatchRuleTests
    {
        [Test]
        public void CanCatch_WithinOneMeter_ReturnsTrue()
        {
            Assert.IsTrue(CatchRule.CanCatch(0.9f, 1f, false));
        }

        [Test]
        public void CanCatch_FartherThanOneMeter_ReturnsFalse()
        {
            Assert.IsFalse(CatchRule.CanCatch(1.1f, 1f, false));
        }

        [Test]
        public void CanCatch_UntouchableTarget_ReturnsFalse()
        {
            Assert.IsFalse(CatchRule.CanCatch(0.2f, 1f, true));
        }
    }
}
