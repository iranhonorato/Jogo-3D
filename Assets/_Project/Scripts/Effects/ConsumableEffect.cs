namespace Guaxinim.Effects
{
    /// <summary>
    /// Efeito temporário de um consumível (GDD §8.1). A base controla a duração; cada efeito só diz o que
    /// modifica. Efeitos não escrevem nos atributos do guaxinim: o <see cref="ActiveEffects"/> combina os
    /// modificadores de todos os efeitos ativos, para energético e proteína funcionarem juntos.
    /// </summary>
    public abstract class ConsumableEffect
    {
        protected ConsumableEffect(float durationSeconds)
        {
            DurationSeconds = durationSeconds;
            RemainingSeconds = durationSeconds;
        }

        public float DurationSeconds { get; }
        public float RemainingSeconds { get; private set; }
        public bool IsFinished => RemainingSeconds <= 0f;

        /// <summary>Multiplica a velocidade final (1 = sem efeito).</summary>
        public virtual float SpeedMultiplier => 1f;

        /// <summary>Soma ao peso máximo (0 = sem efeito).</summary>
        public virtual float MaxWeightBonus => 0f;

        /// <summary>Multiplica a penalidade do peso na velocidade (1 = sem efeito).</summary>
        public virtual float WeightPenaltyScale => 1f;

        public void Tick(float deltaTime)
        {
            if (!IsFinished)
            {
                RemainingSeconds -= deltaTime;
            }
        }

        /// <summary>Pegar outro igual renova o tempo, em vez de acumular (GDD §8.1).</summary>
        public void Refresh() => RemainingSeconds = DurationSeconds;
    }
}
