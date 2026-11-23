using System.Collections.Generic;
using Guaxinim.Noise;
using NUnit.Framework;
using UnityEngine;

namespace Guaxinim.Tests
{
    /// <summary>GDD §5: raio do som de correr com carga e alcance dos barulhos.</summary>
    public class NoiseTests
    {
        private const float Tolerance = 0.001f;
        private const float RunRadius = 6f;
        private const float CarryBonus = 3f;
        private const float MaxWeight = 10f;

        private readonly List<FakeListener> _listeners = new List<FakeListener>();
        private readonly List<NoiseEvent> _emitted = new List<NoiseEvent>();

        [SetUp]
        public void SetUp() => NoiseSystem.OnNoiseEmitted += _emitted.Add;

        [TearDown]
        public void TearDown()
        {
            NoiseSystem.OnNoiseEmitted -= _emitted.Add;
            foreach (FakeListener listener in _listeners)
            {
                NoiseSystem.Unregister(listener);
            }

            _listeners.Clear();
            _emitted.Clear();
        }

        [TestCase(0f, 6f)]
        [TestCase(5f, 7.5f)]
        [TestCase(10f, 9f)]
        [TestCase(15f, 9f)]
        public void ForRunning_WithWeight_GrowsUpToBonus(float weight, float expectedRadius)
        {
            float radius = NoiseRadius.ForRunning(RunRadius, CarryBonus, weight, MaxWeight);

            Assert.AreEqual(expectedRadius, radius, Tolerance);
        }

        [Test]
        public void ForRunning_WithZeroMaxWeight_ReturnsBaseRadius()
        {
            float radius = NoiseRadius.ForRunning(RunRadius, CarryBonus, 5f, 0f);

            Assert.AreEqual(RunRadius, radius, Tolerance);
        }

        [Test]
        public void Emit_ListenerInsideRadius_HearsNoise()
        {
            FakeListener listener = AddListener(new Vector3(5f, 0f, 0f));

            NoiseSystem.Emit(Vector3.zero, 6f);

            Assert.AreEqual(1, listener.HeardCount);
        }

        [Test]
        public void Emit_ListenerOutsideRadius_DoesNotHear()
        {
            FakeListener listener = AddListener(new Vector3(7f, 0f, 0f));

            NoiseSystem.Emit(Vector3.zero, 6f);

            Assert.AreEqual(0, listener.HeardCount);
        }

        [Test]
        public void Emit_UnregisteredListener_DoesNotHear()
        {
            FakeListener listener = AddListener(Vector3.one);
            NoiseSystem.Unregister(listener);

            NoiseSystem.Emit(Vector3.zero, 6f);

            Assert.AreEqual(0, listener.HeardCount);
        }

        [Test]
        public void Emit_ZeroRadius_IsSilent()
        {
            FakeListener listener = AddListener(Vector3.zero);

            NoiseSystem.Emit(Vector3.zero, 0f);

            Assert.AreEqual(0, listener.HeardCount);
            Assert.AreEqual(0, _emitted.Count);
        }

        [Test]
        public void Emit_AnyRadius_RaisesEventForTheRing()
        {
            NoiseSystem.Emit(new Vector3(1f, 0f, 2f), 8f);

            Assert.AreEqual(1, _emitted.Count);
            Assert.AreEqual(8f, _emitted[0].Radius, Tolerance);
        }

        [Test]
        public void Emit_ListenerUnregistersWhileHearing_DoesNotBreakOthers()
        {
            FakeListener leaving = AddListener(Vector3.zero);
            leaving.UnregisterOnHear = true;
            FakeListener staying = AddListener(Vector3.zero);

            NoiseSystem.Emit(Vector3.zero, 1f);

            Assert.AreEqual(1, leaving.HeardCount);
            Assert.AreEqual(1, staying.HeardCount);
        }

        [Test]
        public void RingAnimation_AtEnd_ReachesRadiusAndFadesOut()
        {
            float progress = NoiseRingAnimation.GetProgress(1f, 0.8f);

            Assert.AreEqual(1f, progress, Tolerance);
            Assert.AreEqual(6f, NoiseRingAnimation.GetRadius(progress, 6f), Tolerance);
            Assert.AreEqual(0f, NoiseRingAnimation.GetAlpha(progress), Tolerance);
        }

        private FakeListener AddListener(Vector3 position)
        {
            var listener = new FakeListener(position);
            _listeners.Add(listener);
            NoiseSystem.Register(listener);
            return listener;
        }

        private class FakeListener : INoiseListener
        {
            public FakeListener(Vector3 position) => ListenerPosition = position;

            public Vector3 ListenerPosition { get; }
            public int HeardCount { get; private set; }
            public bool UnregisterOnHear { get; set; }

            public void Hear(NoiseEvent noise)
            {
                HeardCount++;
                if (UnregisterOnHear)
                {
                    NoiseSystem.Unregister(this);
                }
            }
        }
    }
}
