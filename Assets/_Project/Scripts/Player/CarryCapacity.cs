namespace Guaxinim.Player
{
    /// <summary>GDD §4 e §7: limites de carga e a regra do volume que atrapalha.</summary>
    public static class CarryCapacity
    {
        /// <summary>Um item só entra se o peso e o volume somados couberem nos limites.</summary>
        public static bool CanCarry(float currentWeight, float currentVolume, float addedWeight, float addedVolume,
            float maxWeight, float maxVolume) =>
            currentWeight + addedWeight <= maxWeight && currentVolume + addedVolume <= maxVolume;

        /// <summary>GDD §7.4: com volume igual ou maior que o limite (4), a carga tapa a visão e não passa em vãos.</summary>
        public static bool IsBulky(float currentVolume, float bulkyThreshold) => currentVolume >= bulkyThreshold;
    }
}
