using UnityEngine;

namespace Guaxinim.Player
{
    /// <summary>Números do movimento do guaxinim (GDD §4). Somente leitura durante o jogo.</summary>
    [CreateAssetMenu(fileName = "PlayerData", menuName = "Guaxinim/Player Data")]
    public class PlayerData : ScriptableObject
    {
        [Header("Velocidade")]
        [Tooltip("Metros por segundo, andando (silencioso)")]
        [SerializeField, Min(0f)] private float _walkSpeed = 3f;

        [Tooltip("Metros por segundo, correndo (faz barulho)")]
        [SerializeField, Min(0f)] private float _sprintSpeed = 6f;

        [Tooltip("Metros por segundo ao quadrado, ganhando velocidade")]
        [SerializeField, Min(0.01f)] private float _acceleration = 40f;

        [Tooltip("Metros por segundo ao quadrado, freando")]
        [SerializeField, Min(0.01f)] private float _deceleration = 50f;

        [Header("Giro e gravidade")]
        [Tooltip("Graus por segundo ao virar para a direção do movimento")]
        [SerializeField, Min(0f)] private float _turnSpeedDegrees = 900f;

        [Tooltip("Metros por segundo ao quadrado, para baixo")]
        [SerializeField, Min(0f)] private float _gravity = 25f;

        [Tooltip("Metros por segundo para baixo no chão, para manter o contato em rampas e degraus")]
        [SerializeField, Min(0f)] private float _groundedFallSpeed = 2f;

        [Header("Carga")]
        [Tooltip("Unidades de peso que o guaxinim consegue carregar (GDD §4)")]
        [SerializeField, Min(0.1f)] private float _maxWeight = 10f;

        [Tooltip("Unidades de volume que o guaxinim consegue carregar (GDD §4)")]
        [SerializeField, Min(0.1f)] private float _maxVolume = 6f;

        [Tooltip("Fração da velocidade perdida com o peso máximo (GDD §7.3: 0,5 = metade da velocidade)")]
        [SerializeField, Range(0f, 1f)] private float _weightSpeedPenalty = 0.5f;

        [Tooltip("Fração da velocidade com excesso de peso, quando a proteína acaba (GDD §8.1: 0,4 = 40%)")]
        [SerializeField, Range(0.05f, 1f)] private float _overweightSpeedFraction = 0.4f;

        [Tooltip("Volume a partir do qual a carga tapa a visão e não passa em vãos (GDD §7.4)")]
        [SerializeField, Min(0f)] private float _bulkyVolumeThreshold = 4f;

        [Tooltip("Metros: com carga volumosa, cones de visão mais longe que isto deixam de ser desenhados")]
        [SerializeField, Min(0f)] private float _bulkyClearViewRadius = 6f;

        [Header("Frágeis")]
        [Tooltip("Segundos de corrida seguida até um item frágil quebrar (GDD §7.5)")]
        [SerializeField, Min(0.1f)] private float _shakeFillSeconds = 2f;

        [Tooltip("Fração da barra de chacoalhar que esvazia por segundo andando")]
        [SerializeField, Min(0f)] private float _shakeDrainPerSecond = 0.5f;

        [Header("Interação")]
        [Tooltip("Metros: alcance do E para pegar itens e usar objetos")]
        [SerializeField, Min(0.1f)] private float _interactionRadius = 1.1f;

        public float WalkSpeed => _walkSpeed;
        public float SprintSpeed => _sprintSpeed;
        public float Acceleration => _acceleration;
        public float Deceleration => _deceleration;
        public float TurnSpeedDegrees => _turnSpeedDegrees;
        public float Gravity => _gravity;
        public float GroundedFallSpeed => _groundedFallSpeed;
        public float MaxWeight => _maxWeight;
        public float MaxVolume => _maxVolume;
        public float WeightSpeedPenalty => _weightSpeedPenalty;
        public float OverweightSpeedFraction => _overweightSpeedFraction;
        public float BulkyVolumeThreshold => _bulkyVolumeThreshold;
        public float BulkyClearViewRadius => _bulkyClearViewRadius;
        public float ShakeFillSeconds => _shakeFillSeconds;
        public float ShakeDrainPerSecond => _shakeDrainPerSecond;
        public float InteractionRadius => _interactionRadius;

        private void OnValidate()
        {
            if (_sprintSpeed < _walkSpeed)
            {
                Debug.LogWarning("Sprint speed should not be lower than walk speed.", this);
            }
        }
    }
}
