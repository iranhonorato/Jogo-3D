using System.Collections.Generic;

namespace Guaxinim.Core
{
    /// <summary>
    /// Fila das dicas do tutorial: cada dica entra na fila uma vez só, quando o gatilho acontece, e sai na ordem em
    /// que entrou. Lógica pura; a <c>TutorialTipsUI</c> só liga os acontecimentos do jogo a ela.
    /// </summary>
    public sealed class TutorialTipQueue
    {
        private readonly IReadOnlyList<TutorialTip> _tips;
        private readonly bool[] _wasQueued;
        private readonly Queue<TutorialTip> _pending = new Queue<TutorialTip>();

        public TutorialTipQueue(IReadOnlyList<TutorialTip> tips)
        {
            _tips = tips;
            _wasQueued = new bool[tips.Count];
        }

        public bool HasPending => _pending.Count > 0;

        /// <summary>Enfileira as dicas ainda não mostradas desse gatilho (menos o de tempo, que é por segundos).</summary>
        public void NotifyTrigger(TipTrigger trigger)
        {
            for (int i = 0; i < _tips.Count; i++)
            {
                if (_tips[i].Trigger == trigger && trigger != TipTrigger.AfterSeconds)
                {
                    Enqueue(i);
                }
            }
        }

        /// <summary>Enfileira as dicas de tempo cujos segundos já passaram, na ordem da lista.</summary>
        public void NotifyElapsedSeconds(float elapsedSeconds)
        {
            for (int i = 0; i < _tips.Count; i++)
            {
                bool isDue = _tips[i].Trigger == TipTrigger.AfterSeconds && elapsedSeconds >= _tips[i].TriggerSeconds;
                if (isDue)
                {
                    Enqueue(i);
                }
            }
        }

        public bool TryDequeue(out TutorialTip tip)
        {
            if (_pending.Count == 0)
            {
                tip = null;
                return false;
            }

            tip = _pending.Dequeue();
            return true;
        }

        private void Enqueue(int index)
        {
            if (_wasQueued[index])
            {
                return;
            }

            _wasQueued[index] = true;
            _pending.Enqueue(_tips[index]);
        }
    }
}
