using UnityEngine;

namespace Guaxinim.Player
{
    /// <summary>GDD §7.3: velocidade = base × (1 − penalidade × peso / pesoMáximo).</summary>
    public static class CarrySpeed
    {
        public static float Calculate(float baseSpeed, float currentWeight, float maxWeight, float weightPenalty)
        {
            if (maxWeight <= 0f)
            {
                return baseSpeed;
            }

            float load = Mathf.Clamp01(currentWeight / maxWeight);
            return baseSpeed * (1f - weightPenalty * load);
        }
    }
}
