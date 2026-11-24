using UnityEngine;

namespace Guaxinim.Animation
{
    /// <summary>
    /// Curvas das animações procedurais (GDD §15.4: animações pequenas feitas na Unity). Um ciclo de passos vai de
    /// 0 a 2: o primeiro passo de 0 a 1 e o segundo de 1 a 2. Tudo em metros, graus e frações da escala.
    /// </summary>
    public static class MotionMath
    {
        /// <summary>Quique: sobe no meio de cada passo e volta ao chão no fim.</summary>
        public static float Bounce(float stepPhase, float heightMeters, float intensity) =>
            Mathf.Abs(Mathf.Sin(stepPhase * Mathf.PI)) * heightMeters * intensity;

        /// <summary>Balanço lateral: para um lado no primeiro passo e para o outro no segundo.</summary>
        public static float Sway(float stepPhase, float maxDegrees, float intensity) =>
            Mathf.Sin(stepPhase * Mathf.PI) * maxDegrees * intensity;

        /// <summary>Inclinação para a frente, que cresce com a velocidade até <paramref name="maxDegrees"/>.</summary>
        public static float Lean(float speed, float referenceSpeed, float maxDegrees) =>
            referenceSpeed <= 0f ? 0f : Mathf.Clamp01(speed / referenceSpeed) * maxDegrees;

        /// <summary>Respiração parado: a escala sobe e desce devagar.</summary>
        public static float Breath(float timeSeconds, float periodSeconds, float amplitude) =>
            periodSeconds <= 0f ? 0f : Mathf.Sin(2f * Mathf.PI * timeSeconds / periodSeconds) * amplitude;

        /// <summary>Pulinho de susto: um arco de 0 a 1 do tempo, que sobe até a altura e volta.</summary>
        public static float Hop(float progress, float heightMeters) =>
            progress < 0f || progress > 1f ? 0f : 4f * heightMeters * progress * (1f - progress);

        /// <summary>De 0 (parado) a 1 (andando ou mais rápido): liga o passo e desliga a respiração.</summary>
        public static float MovingIntensity(float speed, float fullAtSpeed) =>
            fullAtSpeed <= 0f ? 1f : Mathf.Clamp01(speed / fullAtSpeed);
    }
}
