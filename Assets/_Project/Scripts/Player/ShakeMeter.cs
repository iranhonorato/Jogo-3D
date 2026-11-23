using UnityEngine;

namespace Guaxinim.Player
{
    /// <summary>
    /// Barra de chacoalhar dos itens frágeis (GDD §7.5), de 0 a 1: enche correndo (cheia depois de
    /// <c>fillSeconds</c> de corrida seguida) e esvazia andando. <see cref="Tick"/> devolve true no frame em
    /// que enche, e a barra volta a zero (o item quebrou).
    /// </summary>
    public class ShakeMeter
    {
        private readonly float _fillSeconds;
        private readonly float _drainPerSecond;

        public ShakeMeter(float fillSeconds, float drainPerSecond)
        {
            _fillSeconds = Mathf.Max(fillSeconds, Mathf.Epsilon);
            _drainPerSecond = drainPerSecond;
        }

        public float Value { get; private set; }

        public bool Tick(bool isRunning, float deltaTime)
        {
            float change = isRunning ? deltaTime / _fillSeconds : -_drainPerSecond * deltaTime;
            Value = Mathf.Clamp01(Value + change);
            if (Value < 1f)
            {
                return false;
            }

            Value = 0f;
            return true;
        }

        public void Reset() => Value = 0f;
    }
}
