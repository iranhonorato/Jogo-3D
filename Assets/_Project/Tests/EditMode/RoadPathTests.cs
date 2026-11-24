using Guaxinim.UI;
using NUnit.Framework;
using UnityEngine;

namespace Guaxinim.Tests
{
    /// <summary>Etapa 4: o marcador vai de uma parada à outra em pulinhos e acende os pontos da estrada.</summary>
    public class RoadPathTests
    {
        private const float Tolerance = 0.001f;
        private const float HopHeight = 20f;
        private const int Hops = 4;
        private static readonly Vector2 Start = new Vector2(100f, 50f);
        private static readonly Vector2 End = new Vector2(500f, 50f);

        [Test]
        public void Evaluate_AtZero_IsTheStart()
        {
            Vector2 point = RoadPath.Evaluate(Start, End, 0f, HopHeight, Hops);

            Assert.AreEqual(Start.x, point.x, Tolerance);
            Assert.AreEqual(Start.y, point.y, Tolerance);
        }

        [Test]
        public void Evaluate_AtOne_IsTheEndOnTheGround()
        {
            Vector2 point = RoadPath.Evaluate(Start, End, 1f, HopHeight, Hops);

            Assert.AreEqual(End.x, point.x, Tolerance);
            Assert.AreEqual(End.y, point.y, Tolerance);
        }

        [Test]
        public void Evaluate_MiddleOfAHop_IsAtFullHeight()
        {
            Vector2 point = RoadPath.Evaluate(Start, End, 0.125f, HopHeight, Hops); // meio do primeiro de 4 pulos

            Assert.AreEqual(150f, point.x, Tolerance);
            Assert.AreEqual(Start.y + HopHeight, point.y, Tolerance);
        }

        [Test]
        public void Evaluate_BeyondOne_StaysAtTheEnd()
        {
            Assert.AreEqual(End.x, RoadPath.Evaluate(Start, End, 1.5f, HopHeight, Hops).x, Tolerance);
        }

        [Test]
        public void LitDotCount_AtTheStart_IsZero()
        {
            Assert.AreEqual(0, RoadPath.LitDotCount(5, 0f));
        }

        [Test]
        public void LitDotCount_HalfWay_LightsTheFirstHalf()
        {
            Assert.AreEqual(3, RoadPath.LitDotCount(5, 0.5f));
        }

        [Test]
        public void LitDotCount_AtTheEnd_LightsAll()
        {
            Assert.AreEqual(5, RoadPath.LitDotCount(5, 1f));
        }
    }
}
