using Guaxinim.Guards;
using Guaxinim.Shared;
using NUnit.Framework;
using UnityEngine;

namespace Guaxinim.Tests
{
    /// <summary>GDD §6.1: cone de 9 m e 70°, no plano do chão.</summary>
    public class VisionTests
    {
        private const float Range = 9f;
        private const float Angle = 70f;

        [Test]
        public void IsInCone_TargetAheadInRange_ReturnsTrue()
        {
            Assert.IsTrue(VisionMath.IsInCone(Vector3.zero, Vector3.forward, new Vector3(0f, 0f, 8f), Range, Angle));
        }

        [Test]
        public void IsInCone_TargetBeyondRange_ReturnsFalse()
        {
            Assert.IsFalse(VisionMath.IsInCone(Vector3.zero, Vector3.forward, new Vector3(0f, 0f, 9.5f), Range, Angle));
        }

        [Test]
        public void IsInCone_TargetInsideHalfAngle_ReturnsTrue()
        {
            Vector3 target = Quaternion.Euler(0f, 34f, 0f) * Vector3.forward * 5f;

            Assert.IsTrue(VisionMath.IsInCone(Vector3.zero, Vector3.forward, target, Range, Angle));
        }

        [Test]
        public void IsInCone_TargetOutsideHalfAngle_ReturnsFalse()
        {
            Vector3 target = Quaternion.Euler(0f, 36f, 0f) * Vector3.forward * 5f;

            Assert.IsFalse(VisionMath.IsInCone(Vector3.zero, Vector3.forward, target, Range, Angle));
        }

        [Test]
        public void IsInCone_TargetBehind_ReturnsFalse()
        {
            Assert.IsFalse(VisionMath.IsInCone(Vector3.zero, Vector3.forward, new Vector3(0f, 0f, -2f), Range, Angle));
        }

        [Test]
        public void IsInCone_HeightDifference_IsIgnored()
        {
            var eye = new Vector3(0f, 1.5f, 0f);

            Assert.IsTrue(VisionMath.IsInCone(eye, Vector3.forward, new Vector3(0f, 0.3f, 8.9f), Range, Angle));
        }

        [Test]
        public void FlatDistance_IgnoresHeight()
        {
            float distance = VisionMath.FlatDistance(new Vector3(0f, 2f, 0f), new Vector3(3f, 0f, 4f));

            Assert.AreEqual(5f, distance, 0.001f);
        }

        [Test]
        public void Registry_RegisterTwice_KeepsOneEntry()
        {
            var detectable = new FakeDetectable();

            DetectableRegistry.Register(detectable);
            DetectableRegistry.Register(detectable);
            int count = CountOf(detectable);
            DetectableRegistry.Unregister(detectable);

            Assert.AreEqual(1, count);
            Assert.AreEqual(0, CountOf(detectable));
        }

        private static int CountOf(IDetectable detectable)
        {
            int count = 0;
            for (int i = 0; i < DetectableRegistry.All.Count; i++)
            {
                if (DetectableRegistry.All[i] == detectable)
                {
                    count++;
                }
            }

            return count;
        }

        private class FakeDetectable : IDetectable
        {
            public Vector3 DetectionPoint => Vector3.zero;
            public bool IsVisible => true;
            public bool IsUntouchable => false;

            public void Catch()
            {
            }
        }
    }
}
