namespace Guaxinim.Effects
{
    /// <summary>Barra de proteína (GDD §8.1): peso máximo de 10 para 15 e metade da penalidade do peso, por 15 s.</summary>
    public class ProteinBarEffect : ConsumableEffect
    {
        private readonly float _maxWeightBonus;
        private readonly float _weightPenaltyScale;

        public ProteinBarEffect(float durationSeconds, float maxWeightBonus, float weightPenaltyScale)
            : base(durationSeconds)
        {
            _maxWeightBonus = maxWeightBonus;
            _weightPenaltyScale = weightPenaltyScale;
        }

        public override float MaxWeightBonus => _maxWeightBonus;
        public override float WeightPenaltyScale => _weightPenaltyScale;
    }
}
