using UnityEngine;

namespace Guaxinim.Core
{
    /// <summary>Tempos do ciclo da fase (GDD §6.5 e §13). Somente leitura durante o jogo.</summary>
    [CreateAssetMenu(fileName = "LevelFlowData", menuName = "Guaxinim/Level Flow Data")]
    public class LevelFlowData : ScriptableObject
    {
        [Tooltip("Segundos com o aviso \"Você foi pego!\" antes de reiniciar a fase")]
        [SerializeField, Min(0f)] private float _caughtRestartDelaySeconds = 1.5f;

        public float CaughtRestartDelaySeconds => _caughtRestartDelaySeconds;
    }
}
