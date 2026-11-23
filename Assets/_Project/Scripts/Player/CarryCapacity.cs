namespace Guaxinim.Player
{
    /// <summary>GDD §4 e §7: um item só entra se o peso e o volume somados couberem nos limites.</summary>
    public static class CarryCapacity
    {
        public static bool CanCarry(float currentWeight, float currentVolume, float addedWeight, float addedVolume,
            float maxWeight, float maxVolume) =>
            currentWeight + addedWeight <= maxWeight && currentVolume + addedVolume <= maxVolume;
    }
}
