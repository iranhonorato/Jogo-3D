using UnityEngine;

namespace Guaxinim.Effects
{
    /// <summary>Números da barra de proteína (GDD §8.1: peso máximo +5 e metade da penalidade, por 15 s).</summary>
    [CreateAssetMenu(fileName = "Effect_ProteinBar", menuName = "Guaxinim/Effects/Protein Bar")]
    public class ProteinBarEffectData : ConsumableEffectData
    {
        [Tooltip("Unidades somadas ao peso máximo (10 + 5 = 15)")]
        [SerializeField, Min(0f)] private float _maxWeightBonus = 5f;

        [Tooltip("Multiplicador da penalidade do peso (0,5 = metade)")]
        [SerializeField, Range(0f, 1f)] private float _weightPenaltyScale = 0.5f;

        public override ConsumableEffect CreateEffect() =>
            new ProteinBarEffect(DurationSeconds, _maxWeightBonus, _weightPenaltyScale);
    }
}
