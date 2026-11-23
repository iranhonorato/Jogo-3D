using UnityEngine;

namespace Guaxinim.Guards
{
    /// <summary>Velocidade do medidor de detecção (GDD §6.3 e §6.6). Somente leitura durante o jogo.</summary>
    [CreateAssetMenu(fileName = "DetectionData", menuName = "Guaxinim/Detection Data")]
    public class DetectionData : ScriptableObject
    {
        [Tooltip("Segundos para encher de 0 a 100% com o alvo no alcance máximo do cone")]
        [SerializeField, Min(0.05f)] private float _fillSecondsAtMaxRange = 1f;

        [Tooltip("Quantas vezes mais rápido enche com o alvo colado no detector (1 = sempre igual)")]
        [SerializeField, Min(1f)] private float _closeRangeMultiplier = 3f;

        [Tooltip("Fração do medidor (0 a 1) que esvazia por segundo sem ver o alvo")]
        [SerializeField, Min(0f)] private float _drainPerSecond = 0.35f;

        public float FillSecondsAtMaxRange => _fillSecondsAtMaxRange;
        public float CloseRangeMultiplier => _closeRangeMultiplier;
        public float DrainPerSecond => _drainPerSecond;
    }
}
