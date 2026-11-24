using Guaxinim.Core;
using NUnit.Framework;

namespace Guaxinim.Tests
{
    /// <summary>GDD §11: as dicas do tutorial saem na ordem, cada uma uma vez só, quando o gatilho acontece.</summary>
    public class TutorialTipQueueTests
    {
        private static TutorialTipQueue CreateQueue() => new TutorialTipQueue(new[]
        {
            new TutorialTip("andar", TipTrigger.AfterSeconds, 1f),
            new TutorialTip("correr", TipTrigger.AfterSeconds, 7f),
            new TutorialTip("pegar", TipTrigger.NearPickup),
            new TutorialTip("levar", TipTrigger.FirstPickup),
            new TutorialTip("lixeira", TipTrigger.NearHidingSpot),
        });

        [Test]
        public void NotifyElapsedSeconds_BeforeTheTime_QueuesNothing()
        {
            TutorialTipQueue queue = CreateQueue();

            queue.NotifyElapsedSeconds(0.5f);

            Assert.IsFalse(queue.HasPending);
        }

        [Test]
        public void NotifyElapsedSeconds_AfterTheTime_QueuesTipsInListOrder()
        {
            TutorialTipQueue queue = CreateQueue();

            queue.NotifyElapsedSeconds(8f);

            Assert.IsTrue(queue.TryDequeue(out TutorialTip first));
            Assert.IsTrue(queue.TryDequeue(out TutorialTip second));
            Assert.AreEqual("andar", first.Text);
            Assert.AreEqual("correr", second.Text);
            Assert.IsFalse(queue.TryDequeue(out TutorialTip _));
        }

        [Test]
        public void NotifyTrigger_QueuesTheMatchingTip()
        {
            TutorialTipQueue queue = CreateQueue();

            queue.NotifyTrigger(TipTrigger.NearPickup);

            Assert.IsTrue(queue.TryDequeue(out TutorialTip tip));
            Assert.AreEqual("pegar", tip.Text);
        }

        [Test]
        public void NotifyTrigger_SameTriggerTwice_ShowsTheTipOnlyOnce()
        {
            TutorialTipQueue queue = CreateQueue();
            queue.NotifyTrigger(TipTrigger.NearHidingSpot);
            queue.TryDequeue(out TutorialTip _);

            queue.NotifyTrigger(TipTrigger.NearHidingSpot);

            Assert.IsFalse(queue.HasPending);
        }

        [Test]
        public void NotifyTrigger_AfterSecondsTrigger_IsIgnored()
        {
            TutorialTipQueue queue = CreateQueue();

            queue.NotifyTrigger(TipTrigger.AfterSeconds);

            Assert.IsFalse(queue.HasPending);
        }
    }
}
