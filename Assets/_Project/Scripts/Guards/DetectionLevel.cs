using UnityEngine;

namespace Guaxinim.Guards
{
    /// <summary>
    /// Nível do medidor de detecção, de 0 a 1 (GDD §6.3): enche vendo o alvo, mais rápido de perto,
    /// e esvazia devagar sem ver. <see cref="Tick"/> devolve true no frame em que fica cheio.
    /// </summary>
    public class DetectionLevel
    {
        private readonly float _fillSecondsAtMaxRange;
        private readonly float _closeRangeMultiplier;
        private readonly float _drainPerSecond;

        public DetectionLevel(float fillSecondsAtMaxRange, float closeRangeMultiplier, float drainPerSecond)
        {
            _fillSecondsAtMaxRange = Mathf.Max(fillSecondsAtMaxRange, Mathf.Epsilon);
            _closeRangeMultiplier = Mathf.Max(closeRangeMultiplier, 1f);
            _drainPerSecond = drainPerSecond;
        }

        public float Value { get; private set; }
        public bool IsFull => Value >= 1f;

        /// <summary>Velocidade de enchimento (fração por segundo) a <paramref name="distance"/> metros.</summary>
        public float GetFillRate(float distance, float range)
        {
            float closeness = range > 0f ? 1f - Mathf.Clamp01(distance / range) : 1f;
            float multiplier = Mathf.Lerp(1f, _closeRangeMultiplier, closeness);
            return multiplier / _fillSecondsAtMaxRange;
        }

        public bool Tick(bool isTargetSeen, float distance, float range, float deltaTime)
        {
            bool wasFull = IsFull;
            float change = isTargetSeen ? GetFillRate(distance, range) : -_drainPerSecond;
            Value = Mathf.Clamp01(Value + change * deltaTime);
            return IsFull && !wasFull;
        }

        public void Reset() => Value = 0f;
    }
}
