using System.Collections.Generic;

namespace Guaxinim.Effects
{
    /// <summary>
    /// Efeitos ativos no guaxinim e a combinação dos modificadores (GDD §8.1): multiplicadores se
    /// multiplicam e bônus se somam. Um efeito do mesmo tipo que já está ativo só renova o tempo.
    /// </summary>
    public class ActiveEffects
    {
        private readonly List<ConsumableEffect> _effects = new List<ConsumableEffect>();

        public IReadOnlyList<ConsumableEffect> Effects => _effects;

        public float SpeedMultiplier
        {
            get
            {
                float multiplier = 1f;
                for (int i = 0; i < _effects.Count; i++)
                {
                    multiplier *= _effects[i].SpeedMultiplier;
                }

                return multiplier;
            }
        }

        public float MaxWeightBonus
        {
            get
            {
                float bonus = 0f;
                for (int i = 0; i < _effects.Count; i++)
                {
                    bonus += _effects[i].MaxWeightBonus;
                }

                return bonus;
            }
        }

        public float WeightPenaltyScale
        {
            get
            {
                float scale = 1f;
                for (int i = 0; i < _effects.Count; i++)
                {
                    scale *= _effects[i].WeightPenaltyScale;
                }

                return scale;
            }
        }

        public void Add(ConsumableEffect effect)
        {
            for (int i = 0; i < _effects.Count; i++)
            {
                if (_effects[i].GetType() == effect.GetType())
                {
                    _effects[i].Refresh();
                    return;
                }
            }

            _effects.Add(effect);
        }

        /// <summary>Conta o tempo e tira os que acabaram. Devolve true se algum acabou neste frame.</summary>
        public bool Tick(float deltaTime)
        {
            bool anyFinished = false;
            for (int i = _effects.Count - 1; i >= 0; i--)
            {
                _effects[i].Tick(deltaTime);
                if (_effects[i].IsFinished)
                {
                    _effects.RemoveAt(i);
                    anyFinished = true;
                }
            }

            return anyFinished;
        }
    }
}
