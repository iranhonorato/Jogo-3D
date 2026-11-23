using Guaxinim.Core;
using UnityEngine;

namespace Guaxinim.Player
{
    /// <summary>
    /// Move o guaxinim com o CharacterController (GDD §4): anda e corre relativo à câmera
    /// isométrica, gira para a direção do movimento e cai com gravidade.
    /// </summary>
    [RequireComponent(typeof(CharacterController), typeof(PlayerState))]
    public class RaccoonController : MonoBehaviour
    {
        [SerializeField] private PlayerData _data;
        [SerializeField] private InputReader _input;

        [Tooltip("Câmera do jogo: só a rotação em Y é usada, para o W andar para o fundo da tela")]
        [SerializeField] private Transform _cameraTransform;

        private CharacterController _controller;
        private PlayerState _state;
        private Vector3 _horizontalVelocity;
        private float _verticalVelocity;

        /// <summary>Correndo de verdade: Shift segurado e com input de movimento (GDD §5: faz barulho).</summary>
        public bool IsRunning { get; private set; }

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _state = GetComponent<PlayerState>();
            if (_data == null || _input == null || _cameraTransform == null)
            {
                Debug.LogError("RaccoonController needs Data, Input and Camera Transform assigned.", this);
                enabled = false;
            }
        }

        private void OnEnable() => _state.OnCaught += HandleCaught;

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            Vector3 direction = RaccoonLocomotion.GetMoveDirection(_input.Move, _cameraTransform.eulerAngles.y);
            IsRunning = _input.IsSprinting && direction.sqrMagnitude > 0f;

            UpdateHorizontalVelocity(direction, deltaTime);
            _verticalVelocity = RaccoonLocomotion.StepVerticalVelocity(_verticalVelocity, _controller.isGrounded,
                _data.Gravity, _data.GroundedFallSpeed, deltaTime);

            _controller.Move((_horizontalVelocity + Vector3.up * _verticalVelocity) * deltaTime);
            TurnTowards(direction, deltaTime);
        }

        private void OnDisable()
        {
            IsRunning = false; // parado não faz barulho
            _state.OnCaught -= HandleCaught;
        }

        private void HandleCaught() => enabled = false; // GDD §6.5: pego, para de se mexer até reiniciar

        private void UpdateHorizontalVelocity(Vector3 direction, float deltaTime)
        {
            float speed = RaccoonLocomotion.GetTargetSpeed(direction.magnitude, _input.IsSprinting,
                _data.WalkSpeed, _data.SprintSpeed);
            Vector3 target = direction.sqrMagnitude > 0f ? direction.normalized * speed : Vector3.zero;
            _horizontalVelocity = RaccoonLocomotion.StepHorizontalVelocity(_horizontalVelocity, target,
                _data.Acceleration, _data.Deceleration, deltaTime);
        }

        private void TurnTowards(Vector3 direction, float deltaTime)
        {
            if (direction.sqrMagnitude <= 0f)
            {
                return;
            }

            Quaternion target = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target,
                _data.TurnSpeedDegrees * deltaTime);
        }
    }
}
