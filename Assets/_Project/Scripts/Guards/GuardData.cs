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

        [Header("Desconfiado")]
        [Tooltip("Metros por segundo, indo até o som ou o relance")]
        [SerializeField, Min(0f)] private float _suspiciousSpeed = 3f;

        [Tooltip("Segundos olhando em volta ao chegar")]
        [SerializeField, Min(0f)] private float _suspiciousLookSeconds = 4f;

        [Header("Perseguição")]
        [Tooltip("Metros por segundo. GDD §6.5: menor que a corrida do guaxinim sem carga (6 m/s)")]
        [SerializeField, Min(0f)] private float _chaseSpeed = 5.5f;

        [Tooltip("Segundos sem ver o guaxinim até desistir e passar para a Busca")]
        [SerializeField, Min(0f)] private float _lostSightGraceSeconds = 0.75f;

        [Tooltip("Segundos entre dois recálculos do caminho até o guaxinim")]
        [SerializeField, Min(0.02f)] private float _chaseRepathSeconds = 0.2f;

        [Tooltip("Metros: mais perto que isto, em Perseguição, o guaxinim é pego (GDD §6.5)")]
        [SerializeField, Min(0.1f)] private float _catchDistance = 1f;

        [Header("Busca")]
        [Tooltip("Metros por segundo, indo até a última posição conhecida")]
        [SerializeField, Min(0f)] private float _searchSpeed = 3f;

        [Tooltip("Segundos procurando em volta antes de voltar à patrulha")]
        [SerializeField, Min(0f)] private float _searchLookSeconds = 6f;

        public float PatrolSpeed => _patrolSpeed;
        public float WaypointPauseSeconds => _waypointPauseSeconds;
        public float LookAroundAngleDegrees => _lookAroundAngleDegrees;
        public float LookAroundPeriodSeconds => _lookAroundPeriodSeconds;
        public float ArrivalDistance => _arrivalDistance;
        public float SuspiciousSpeed => _suspiciousSpeed;
        public float SuspiciousLookSeconds => _suspiciousLookSeconds;
        public float ChaseSpeed => _chaseSpeed;
        public float LostSightGraceSeconds => _lostSightGraceSeconds;
        public float ChaseRepathSeconds => _chaseRepathSeconds;
        public float CatchDistance => _catchDistance;
        public float SearchSpeed => _searchSpeed;
        public float SearchLookSeconds => _searchLookSeconds;
    }
}
