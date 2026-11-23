using Guaxinim.Guards;
using NUnit.Framework;

namespace Guaxinim.Tests
{
    /// <summary>GDD §9: só quem via o guaxinim quando ele se escondeu sabe que ele está na lixeira.</summary>
    public class HidingWitnessTests
    {
        [Test]
        public void SawTargetHide_SeenThenHidden_ReturnsTrue()
        {
            Assert.IsTrue(HidingWitness.SawTargetHide(wasSeenLastFrame: true, isSeenNow: false, isTargetVisible: false));
        }

        [Test]
        public void SawTargetHide_LeftConeWhileVisible_ReturnsFalse()
        {
            Assert.IsFalse(HidingWitness.SawTargetHide(wasSeenLastFrame: true, isSeenNow: false, isTargetVisible: true));
        }

        [Test]
        public void SawTargetHide_AlreadyLostBeforeHiding_ReturnsFalse()
        {
            Assert.IsFalse(HidingWitness.SawTargetHide(wasSeenLastFrame: false, isSeenNow: false, isTargetVisible: false));
        }

        [Test]
        public void SawTargetHide_StillSeen_ReturnsFalse()
        {
            Assert.IsFalse(HidingWitness.SawTargetHide(wasSeenLastFrame: true, isSeenNow: true, isTargetVisible: true));
        }
    }
}
