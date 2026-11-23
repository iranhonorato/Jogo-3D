using UnityEngine;

namespace Guaxinim.Noise
{
    /// <summary>Curva do anel de som: abre rápido até o raio do barulho e vai sumindo.</summary>
    public static class NoiseRingAnimation
    {
        /// <summary>Progresso de 0 a 1 ao longo da vida do anel.</summary>
        public static float GetProgress(float elapsedSeconds, float durationSeconds) =>
            durationSeconds <= 0f ? 1f : Mathf.Clamp01(elapsedSeconds / durationSeconds);

        /// <summary>Raio atual, com desaceleração no fim (ease-out quadrático).</summary>
        public static float GetRadius(float progress, float maxRadius)
        {
            float eased = 1f - (1f - progress) * (1f - progress);
            return maxRadius * eased;
        }

        public static float GetAlpha(float progress) => 1f - progress;
    }
}
