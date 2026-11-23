using Guaxinim.Items;
using NUnit.Framework;
using UnityEngine;

namespace Guaxinim.Tests
{
    /// <summary>GDD §7.1 e §7.7: entrega marca a lista, o resto é bônus, lista completa vence.</summary>
    public class ShoppingListTests
    {
        private ItemData _bread;
        private ItemData _banana;
        private ItemData _cookies;

        [SetUp]
        public void SetUp()
        {
            _bread = ScriptableObject.CreateInstance<ItemData>();
            _banana = ScriptableObject.CreateInstance<ItemData>();
            _cookies = ScriptableObject.CreateInstance<ItemData>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_bread);
            Object.DestroyImmediate(_banana);
            Object.DestroyImmediate(_cookies);
        }

        [Test]
        public void Deliver_ItemOnList_MarksIt()
        {
            var progress = new ShoppingListProgress(new[] { _bread, _banana });

            bool wasOnList = progress.Deliver(_bread);

            Assert.IsTrue(wasOnList);
            Assert.IsTrue(progress.IsDelivered(0));
            Assert.AreEqual(1, progress.DeliveredCount);
            Assert.IsFalse(progress.IsComplete);
        }

        [Test]
        public void Deliver_ItemNotOnList_CountsAsBonus()
        {
            var progress = new ShoppingListProgress(new[] { _bread });

            bool wasOnList = progress.Deliver(_cookies);

            Assert.IsFalse(wasOnList);
            Assert.AreEqual(1, progress.BonusCount);
        }

        [Test]
        public void Deliver_SameItemTwice_SecondIsBonus()
        {
            var progress = new ShoppingListProgress(new[] { _bread });
            progress.Deliver(_bread);

            bool wasOnList = progress.Deliver(_bread);

            Assert.IsFalse(wasOnList);
            Assert.AreEqual(1, progress.BonusCount);
        }

        [Test]
        public void Deliver_RepeatedOnList_NeedsBothUnits()
        {
            var progress = new ShoppingListProgress(new[] { _bread, _bread });
            progress.Deliver(_bread);

            Assert.IsTrue(progress.IsPending(_bread));
            progress.Deliver(_bread);
            Assert.IsTrue(progress.IsComplete);
        }

        [Test]
        public void IsComplete_AllDelivered_ReturnsTrue()
        {
            var progress = new ShoppingListProgress(new[] { _bread, _banana });

            progress.Deliver(_banana);
            progress.Deliver(_bread);

            Assert.IsTrue(progress.IsComplete);
            Assert.IsFalse(progress.IsPending(_bread));
        }
    }
}
