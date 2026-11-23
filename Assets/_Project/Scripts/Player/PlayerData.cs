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

        public float WalkSpeed => _walkSpeed;
        public float SprintSpeed => _sprintSpeed;
        public float Acceleration => _acceleration;
        public float Deceleration => _deceleration;
        public float TurnSpeedDegrees => _turnSpeedDegrees;
        public float Gravity => _gravity;
        public float GroundedFallSpeed => _groundedFallSpeed;

        private void OnValidate()
        {
            if (_sprintSpeed < _walkSpeed)
            {
                Debug.LogWarning("Sprint speed should not be lower than walk speed.", this);
            }
        }
    }
}
