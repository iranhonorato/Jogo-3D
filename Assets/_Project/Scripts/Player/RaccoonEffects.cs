using System;
using Guaxinim.Effects;
using UnityEngine;

namespace Guaxinim.Player
{
    /// <summary>
    /// Efeitos de consumíveis ativos no guaxinim (GDD §8.1). Conta o tempo e expõe os modificadores
    /// combinados; o movimento e o inventário perguntam aqui. A HUD escuta <see cref="OnChanged"/>.
    /// </summary>
    public class RaccoonEffects : MonoBehaviour
    {
        private readonly ActiveEffects _active = new ActiveEffects();

        public event Action OnChanged;

        public ActiveEffects Active => _active;
        public float SpeedMultiplier => _active.SpeedMultiplier;
        public float MaxWeightBonus => _active.MaxWeightBonus;
        public float WeightPenaltyScale => _active.WeightPenaltyScale;

        private void Update()
        {
            if (_active.Tick(Time.deltaTime))
            {
                OnChanged?.Invoke();
            }
        }

        public void Apply(ConsumableEffect effect)
        {
            _active.Add(effect);
            OnChanged?.Invoke();
        }
    }
}
