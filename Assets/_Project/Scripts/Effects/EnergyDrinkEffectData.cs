using UnityEngine;

namespace Guaxinim.Effects
{
    /// <summary>Números do energético (GDD §8.1: +50% de velocidade por 8 s).</summary>
    [CreateAssetMenu(fileName = "Effect_EnergyDrink", menuName = "Guaxinim/Effects/Energy Drink")]
    public class EnergyDrinkEffectData : ConsumableEffectData
    {
        [Tooltip("Multiplicador da velocidade (1,5 = +50%)")]
        [SerializeField, Min(1f)] private float _speedMultiplier = 1.5f;

        public override ConsumableEffect CreateEffect() => new EnergyDrinkEffect(DurationSeconds, _speedMultiplier);
    }
}
