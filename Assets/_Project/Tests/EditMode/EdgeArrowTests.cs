using Guaxinim.UI;
using NUnit.Framework;
using UnityEngine;

namespace Guaxinim.Tests
{
    /// <summary>Seta da toca: visível na tela, ou presa na borda na direção dela.</summary>
    public class EdgeArrowTests
    {
        private const float Tolerance = 0.01f;
        private const float Margin = 50f;
        private static readonly Vector2 ScreenSize = new Vector2(1000f, 600f);

        [Test]
        public void IsOnScreen_PointInsideMargins_IsTrue()
        {
            Assert.IsTrue(EdgeArrow.IsOnScreen(new Vector3(500f, 300f, 10f), ScreenSize, Margin));
        }

        [Test]
        public void IsOnScreen_PointInsideTheMarginBand_IsFalse()
        {
            Assert.IsFalse(EdgeArrow.IsOnScreen(new Vector3(980f, 300f, 10f), ScreenSize, Margin));
        }

        [Test]
        public void IsOnScreen_BehindCamera_IsFalse()
        {
            Assert.IsFalse(EdgeArrow.IsOnScreen(new Vector3(500f, 300f, -1f), ScreenSize, Margin));
        }

        [Test]
        public void ClampToEdge_FarToTheRight_SitsOnRightEdgeAtCenterHeight()
        {
            Vector2 edge = EdgeArrow.ClampToEdge(new Vector3(5000f, 300f, 10f), ScreenSize, Margin);

            Assert.AreEqual(950f, edge.x, Tolerance);
            Assert.AreEqual(300f, edge.y, Tolerance);
        }

        [Test]
        public void ClampToEdge_FarDiagonal_StaysInsideTheScreen()
        {
            Vector2 edge = EdgeArrow.ClampToEdge(new Vector3(3000f, 3000f, 10f), ScreenSize, Margin);

            Assert.AreEqual(550f, edge.y, Tolerance); // bate primeiro na borda de cima
            Assert.LessOrEqual(edge.x, 950f);
        }

        [Test]
        public void ClampToEdge_BehindCamera_PointsTheOtherWay()
        {
            Vector2 edge = EdgeArrow.ClampToEdge(new Vector3(5000f, 300f, -10f), ScreenSize, Margin);

            Assert.AreEqual(50f, edge.x, Tolerance);
        }

        [Test]
        public void AngleDegrees_StraightUp_IsNinety()
        {
            Assert.AreEqual(90f, EdgeArrow.AngleDegrees(Vector2.zero, Vector2.up), Tolerance);
        }
    }
}
