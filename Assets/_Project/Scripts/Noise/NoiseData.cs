using UnityEngine;

namespace Guaxinim.Noise
{
    /// <summary>Raios dos barulhos do guaxinim (GDD §5). Somente leitura durante o jogo.</summary>
    [CreateAssetMenu(fileName = "NoiseData", menuName = "Guaxinim/Noise Data")]
    public class NoiseData : ScriptableObject
    {
        [Header("Correr")]
        [Tooltip("Metros, correndo sem carga")]
        [SerializeField, Min(0f)] private float _runRadius = 6f;

        [Tooltip("Metros a mais com o peso máximo (cresce com peso / peso máximo)")]
        [SerializeField, Min(0f)] private float _runCarryBonusRadius = 3f;

        [Tooltip("Segundos entre dois anéis de som enquanto corre")]
        [SerializeField, Min(0.05f)] private float _runEmitIntervalSeconds = 0.4f;

        [Header("Assobio")]
        [Tooltip("Metros")]
        [SerializeField, Min(0f)] private float _whistleRadius = 8f;

        public float RunRadius => _runRadius;
        public float RunCarryBonusRadius => _runCarryBonusRadius;
        public float RunEmitIntervalSeconds => _runEmitIntervalSeconds;
        public float WhistleRadius => _whistleRadius;
    }
}
