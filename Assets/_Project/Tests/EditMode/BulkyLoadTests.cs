using Guaxinim.Player;
using Guaxinim.Shared;
using NUnit.Framework;
using UnityEngine;

namespace Guaxinim.Tests
{
    /// <summary>GDD §7.4: volume 4 ou mais tapa a visão além de 6 m.</summary>
    public class BulkyLoadTests
    {
        private GameObject _viewer;

        [SetUp]
        public void SetUp() => _viewer = new GameObject("Viewer");

        [TearDown]
        public void TearDown()
        {
            ViewObstruction.End(_viewer.transform);
            Object.DestroyImmediate(_viewer);
        }

        [TestCase(3f, false)]
        [TestCase(4f, true)]
        [TestCase(5f, true)]
        public void IsBulky_ByVolume_MatchesThreshold(float volume, bool expected)
        {
            Assert.AreEqual(expected, CarryCapacity.IsBulky(volume, 4f));
        }

        [Test]
        public void IsHidden_WhenInactive_ShowsEverything()
        {
            Assert.IsFalse(ViewObstruction.IsHidden(new Vector3(50f, 0f, 0f)));
        }

        [Test]
        public void IsHidden_WhenActive_HidesOnlyBeyondRadius()
        {
            ViewObstruction.Begin(_viewer.transform, 6f);

            Assert.IsFalse(ViewObstruction.IsHidden(new Vector3(5f, 3f, 0f)));
            Assert.IsTrue(ViewObstruction.IsHidden(new Vector3(0f, 0f, 7f)));
        }

        [Test]
        public void End_ByAnotherViewer_KeepsObstruction()
        {
            var other = new GameObject("Other");
            ViewObstruction.Begin(_viewer.transform, 6f);

            ViewObstruction.End(other.transform);
            Object.DestroyImmediate(other);

            Assert.IsTrue(ViewObstruction.IsActive);
        }
    }
}
