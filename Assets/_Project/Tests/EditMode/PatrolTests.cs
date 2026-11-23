using Guaxinim.Guards;
using NUnit.Framework;

namespace Guaxinim.Tests
{
    /// <summary>GDD §6.2: patrulha pelos waypoints e olhar em volta nas pausas.</summary>
    public class PatrolTests
    {
        private const float Tolerance = 0.001f;

        [Test]
        public void Advance_PastLastWaypoint_ReturnsToFirst()
        {
            var route = new PatrolRoute(3);

            route.Advance();
            route.Advance();
            route.Advance();

            Assert.AreEqual(0, route.CurrentIndex);
        }

        [Test]
        public void Advance_EmptyRoute_StaysAtZero()
        {
            var route = new PatrolRoute(0);

            route.Advance();

            Assert.IsTrue(route.IsEmpty);
            Assert.AreEqual(0, route.CurrentIndex);
        }

        [Test]
        public void GetYawOffset_AtStart_LooksForward()
        {
            Assert.AreEqual(0f, LookAround.GetYawOffset(0f, 60f, 2f), Tolerance);
        }

        [Test]
        public void GetYawOffset_AtQuarterPeriod_LooksFullySideways()
        {
            Assert.AreEqual(60f, LookAround.GetYawOffset(0.5f, 60f, 2f), Tolerance);
        }

        [Test]
        public void GetYawOffset_AtThreeQuarters_LooksToTheOtherSide()
        {
            Assert.AreEqual(-60f, LookAround.GetYawOffset(1.5f, 60f, 2f), Tolerance);
        }
    }
}
