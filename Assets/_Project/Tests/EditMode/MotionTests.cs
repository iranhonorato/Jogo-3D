using Guaxinim.Animation;
using NUnit.Framework;
using UnityEngine;

namespace Guaxinim.Tests
{
    /// <summary>Animações procedurais: ciclo de passos pela distância, quique, balanço, inclinação e arco do item.</summary>
    public class MotionTests
    {
        private const float Tolerance = 0.001f;

        [Test]
        public void StepCycle_HalfAStepLength_IsHalfwayThroughTheFirstStep()
        {
            var cycle = new StepCycle();

            cycle.Advance(0.2f, 0.4f);

            Assert.AreEqual(0.5f, cycle.Phase, Tolerance);
        }

        [Test]
        public void StepCycle_TwoSteps_WrapsAround()
        {
            var cycle = new StepCycle();

            cycle.Advance(1f, 0.4f); // dois passos e meio

            Assert.AreEqual(0.5f, cycle.Phase, Tolerance);
        }

        [Test]
        public void StepCycle_StandingStill_DoesNotAdvance()
        {
            var cycle = new StepCycle();

            cycle.Advance(0f, 0.4f);

            Assert.AreEqual(0f, cycle.Phase, Tolerance);
        }

        [Test]
        public void Bounce_PeaksMidStepAndTouchesGroundAtTheEnd()
        {
            Assert.AreEqual(0.05f, MotionMath.Bounce(0.5f, 0.05f, 1f), Tolerance);
            Assert.AreEqual(0f, MotionMath.Bounce(1f, 0.05f, 1f), Tolerance);
        }

        [Test]
        public void Sway_AlternatesSidesBetweenSteps()
        {
            Assert.AreEqual(6f, MotionMath.Sway(0.5f, 6f, 1f), Tolerance);
            Assert.AreEqual(-6f, MotionMath.Sway(1.5f, 6f, 1f), Tolerance);
        }

        [Test]
        public void Lean_GrowsWithSpeedUpToTheMaximum()
        {
            Assert.AreEqual(6f, MotionMath.Lean(3f, 6f, 12f), Tolerance);
            Assert.AreEqual(12f, MotionMath.Lean(9f, 6f, 12f), Tolerance);
        }

        [Test]
        public void MovingIntensity_StandingStill_IsZero()
        {
            Assert.AreEqual(0f, MotionMath.MovingIntensity(0f, 1f), Tolerance);
            Assert.AreEqual(1f, MotionMath.MovingIntensity(3f, 1f), Tolerance);
        }

        [Test]
        public void Hop_PeaksInTheMiddleAndEndsOnTheGround()
        {
            Assert.AreEqual(0.25f, MotionMath.Hop(0.5f, 0.25f), Tolerance);
            Assert.AreEqual(0f, MotionMath.Hop(1f, 0.25f), Tolerance);
        }

        [Test]
        public void ArcPath_StartsAndEndsOnTheEndpoints()
        {
            var start = new Vector3(0f, 0f, 0f);
            var end = new Vector3(2f, 0f, 0f);

            Assert.AreEqual(start, ArcPath.Evaluate(start, end, 0.5f, 0f));
            Assert.AreEqual(end, ArcPath.Evaluate(start, end, 0.5f, 1f));
        }

        [Test]
        public void ArcPath_Midpoint_RisesToTheArcHeight()
        {
            Vector3 middle = ArcPath.Evaluate(Vector3.zero, new Vector3(2f, 0f, 0f), 0.5f, 0.5f);

            Assert.AreEqual(1f, middle.x, Tolerance);
            Assert.AreEqual(0.5f, middle.y, Tolerance);
        }
    }
}
