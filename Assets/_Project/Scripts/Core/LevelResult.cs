namespace Guaxinim.Core
{
    /// <summary>
    /// Resultado de uma fase vencida (GDD §12): três estrelas (lista completa, abaixo do tempo-alvo, sem
    /// alerta geral) e os pontos dos itens bônus.
    /// </summary>
    public readonly struct LevelResult
    {
        private LevelResult(float elapsedSeconds, float targetSeconds, bool isListComplete, bool hadAlert,
            int bonusCount, int bonusPoints)
        {
            ElapsedSeconds = elapsedSeconds;
            TargetSeconds = targetSeconds;
            IsListComplete = isListComplete;
            HadAlert = hadAlert;
            BonusCount = bonusCount;
            BonusPoints = bonusPoints;
        }

        public float ElapsedSeconds { get; }
        public float TargetSeconds { get; }
        public bool IsListComplete { get; }
        public bool HadAlert { get; }
        public int BonusCount { get; }
        public int BonusPoints { get; }

        public bool BeatTargetTime => ElapsedSeconds <= TargetSeconds;

        public int Stars => (IsListComplete ? 1 : 0) + (BeatTargetTime ? 1 : 0) + (HadAlert ? 0 : 1);

        public static LevelResult Create(float elapsedSeconds, float targetSeconds, bool isListComplete, bool hadAlert,
            int bonusCount, int pointsPerBonusItem) =>
            new LevelResult(elapsedSeconds, targetSeconds, isListComplete, hadAlert, bonusCount,
                bonusCount * pointsPerBonusItem);
    }
}
