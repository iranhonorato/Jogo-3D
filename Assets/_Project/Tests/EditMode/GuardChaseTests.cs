using Guaxinim.Guards;
using Guaxinim.Player;
using NUnit.Framework;
using UnityEngine;

namespace Guaxinim.Tests
{
    /// <summary>GDD §6.2 e §6.5: perder de vista na Perseguição e o balanço de velocidades.</summary>
    public class GuardChaseTests
    {
        [Test]
        public void LostSight_ShortGap_DoesNotGiveUp()
        {
            var timer = new LostSightTimer(0.75f);

            bool lost = timer.Tick(false, 0.5f);

            Assert.IsFalse(lost);
        }

        [Test]
        public void LostSight_GapLongerThanGrace_GivesUp()
        {
            var timer = new LostSightTimer(0.75f);
            timer.Tick(false, 0.5f);

            bool lost = timer.Tick(false, 0.5f);

            Assert.IsTrue(lost);
        }

        [Test]
        public void LostSight_SeenAgain_RestartsCount()
        {
            var timer = new LostSightTimer(0.75f);
            timer.Tick(false, 0.5f);
            timer.Tick(true, 0.1f);

            bool lost = timer.Tick(false, 0.5f);

            Assert.IsFalse(lost);
        }

        [Test]
        public void DefaultData_UnloadedRunningRaccoon_OutrunsChasingGuard()
        {
            var playerData = ScriptableObject.CreateInstance<PlayerData>();
            var guardData = ScriptableObject.CreateInstance<GuardData>();

            float raccoonSpeed = playerData.SprintSpeed;
            float guardSpeed = guardData.ChaseSpeed;
            Object.DestroyImmediate(playerData);
            Object.DestroyImmediate(guardData);

            Assert.Greater(raccoonSpeed, guardSpeed);
        }
    }
}
