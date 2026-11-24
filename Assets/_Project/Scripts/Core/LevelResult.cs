namespace Guaxinim.Core
{
    /// <summary>
    /// Resultado de uma fase vencida (GDD §12): três estrelas (lista completa, abaixo do tempo-alvo, sem
    /// ser perseguido) e os pontos dos itens bônus.
    /// </summary>
    public readonly struct LevelResult
    {
        /// <summary>Estrelas possíveis em cada fase.</summary>
        public const int MaxStars = 3;

        private LevelResult(float elapsedSeconds, float targetSeconds, bool isListComplete, bool wasChased,
            int bonusCount, int bonusPoints)
        {
            ElapsedSeconds = elapsedSeconds;
            TargetSeconds = targetSeconds;
            IsListComplete = isListComplete;
            WasChased = wasChased;
            BonusCount = bonusCount;
            BonusPoints = bonusPoints;
        }

        public float ElapsedSeconds { get; }
        public float TargetSeconds { get; }
        public bool IsListComplete { get; }
        public bool WasChased { get; }
        public int BonusCount { get; }
        public int BonusPoints { get; }

        public bool BeatTargetTime => ElapsedSeconds <= TargetSeconds;

        public int Stars => (IsListComplete ? 1 : 0) + (BeatTargetTime ? 1 : 0) + (WasChased ? 0 : 1);

        public static LevelResult Create(float elapsedSeconds, float targetSeconds, bool isListComplete, bool wasChased,
            int bonusCount, int pointsPerBonusItem) =>
            new LevelResult(elapsedSeconds, targetSeconds, isListComplete, wasChased, bonusCount,
                bonusCount * pointsPerBonusItem);
    }
}
