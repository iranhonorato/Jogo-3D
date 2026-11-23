using UnityEngine;

namespace Guaxinim.Noise
{
    /// <summary>Raios de barulho que dependem do estado do guaxinim (GDD §5).</summary>
    public static class NoiseRadius
    {
        /// <summary>Correr: raio base + bônus × (peso / peso máximo). Sem carga, só o raio base.</summary>
        public static float ForRunning(float baseRadius, float carryBonusRadius, float currentWeight, float maxWeight)
        {
            if (maxWeight <= 0f)
            {
                return baseRadius;
            }

            float load = Mathf.Clamp01(currentWeight / maxWeight);
            return baseRadius + carryBonusRadius * load;
        }
    }
}
