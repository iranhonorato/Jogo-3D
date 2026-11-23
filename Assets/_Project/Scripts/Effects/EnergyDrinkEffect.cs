namespace Guaxinim.Effects
{
    /// <summary>Energético (GDD §8.1): +50% de velocidade, inclusive carregado, por 8 s.</summary>
    public class EnergyDrinkEffect : ConsumableEffect
    {
        private readonly float _speedMultiplier;

        public EnergyDrinkEffect(string displayName, float durationSeconds, float speedMultiplier)
            : base(displayName, durationSeconds) => _speedMultiplier = speedMultiplier;

        public override float SpeedMultiplier => _speedMultiplier;
    }
}
