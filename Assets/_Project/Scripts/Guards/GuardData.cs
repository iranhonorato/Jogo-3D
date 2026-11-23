using UnityEngine;

namespace Guaxinim.Guards
{
    /// <summary>Números do segurança (GDD §6.2). Somente leitura durante o jogo.</summary>
    [CreateAssetMenu(fileName = "GuardData", menuName = "Guaxinim/Guard Data")]
    public class GuardData : ScriptableObject
    {
        [Header("Patrulha")]
        [Tooltip("Metros por segundo")]
        [SerializeField, Min(0f)] private float _patrolSpeed = 2f;

        [Tooltip("Segundos parado em cada waypoint, olhando em volta")]
        [SerializeField, Min(0f)] private float _waypointPauseSeconds = 2.5f;

        [Tooltip("Graus para cada lado ao olhar em volta")]
        [SerializeField, Range(0f, 180f)] private float _lookAroundAngleDegrees = 60f;

        [Tooltip("Segundos de uma olhada completa (esquerda, direita e volta)")]
        [SerializeField, Min(0.1f)] private float _lookAroundPeriodSeconds = 2.5f;

        [Tooltip("Metros: a esta distância do destino, conta como chegou")]
        [SerializeField, Min(0.05f)] private float _arrivalDistance = 0.3f;

        public float PatrolSpeed => _patrolSpeed;
        public float WaypointPauseSeconds => _waypointPauseSeconds;
        public float LookAroundAngleDegrees => _lookAroundAngleDegrees;
        public float LookAroundPeriodSeconds => _lookAroundPeriodSeconds;
        public float ArrivalDistance => _arrivalDistance;
    }
}
