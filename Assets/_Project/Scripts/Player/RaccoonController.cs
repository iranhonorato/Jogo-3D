using Guaxinim.Core;
using UnityEngine;

namespace Guaxinim.Player
{
    /// <summary>
    /// Move o guaxinim com o CharacterController (GDD §4): anda e corre relativo à câmera
    /// isométrica, fica mais lento com o peso e mais rápido com o energético (GDD §7.3 e §8.1), gira para a direção do movimento
    /// e cai com gravidade.
    /// </summary>
    [RequireComponent(typeof(CharacterController), typeof(PlayerState), typeof(Inventory))]
    [RequireComponent(typeof(RaccoonEffects))]
    public class RaccoonController : MonoBehaviour
    {
        // Um quadro longo (o primeiro depois de carregar uma fase grande) não pode virar um passo enorme, que
        // atravessaria o piso.
        private const float MaxStepSeconds = 0.05f;

        [SerializeField] private PlayerData _data;
        [SerializeField] private InputReader _input;

        [Tooltip("Câmera do jogo: só a rotação em Y é usada, para o W andar para o fundo da tela")]
        [SerializeField] private Transform _cameraTransform;

        private CharacterController _controller;
        private PlayerState _state;
        private Inventory _inventory;
        private RaccoonEffects _effects;
        private Vector3 _horizontalVelocity;
        private float _verticalVelocity;
        private Vector3 _spawnPosition;

        /// <summary>Correndo de verdade: Shift segurado e com input de movimento (GDD §5: faz barulho).</summary>
        public bool IsRunning { get; private set; }

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _state = GetComponent<PlayerState>();
            _spawnPosition = transform.position;
            _inventory = GetComponent<Inventory>();
            _effects = GetComponent<RaccoonEffects>();
            if (_data == null || _input == null || _cameraTransform == null)
            {
                Debug.LogError("RaccoonController needs Data, Input and Camera Transform assigned.", this);
                enabled = false;
            }
        }

        private void OnEnable() => _state.OnCaught += HandleCaught;

        private void Update()
        {
            if (!_state.CanMove)
            {
                StandStill(); // na lixeira ou entrando nela
                return;
            }

            float deltaTime = Mathf.Min(Time.deltaTime, MaxStepSeconds);
            Vector3 direction = RaccoonLocomotion.GetMoveDirection(_input.Move, _cameraTransform.eulerAngles.y);
            IsRunning = _input.IsSprinting && direction.sqrMagnitude > 0f;

            UpdateHorizontalVelocity(direction, deltaTime);
            _verticalVelocity = RaccoonLocomotion.StepVerticalVelocity(_verticalVelocity, _controller.isGrounded,
                _data.Gravity, _data.GroundedFallSpeed, deltaTime);

            _controller.Move((_horizontalVelocity + Vector3.up * _verticalVelocity) * deltaTime);
            ReturnToSpawnIfFellOff();
            TurnTowards(direction, deltaTime);
        }

        private void OnDisable()
        {
            IsRunning = false; // parado não faz barulho
            _state.OnCaught -= HandleCaught;
        }

        private void HandleCaught() => enabled = false;

        /// <summary>Rede de segurança: se cair abaixo do piso, volta ao ponto de início em vez de cair para sempre.</summary>
        private void ReturnToSpawnIfFellOff()
        {
            if (transform.position.y > _data.FallResetHeight)
            {
                return;
            }

            _controller.enabled = false; // o CharacterController ignora mudanças de posição enquanto está ligado
            transform.position = _spawnPosition;
            _controller.enabled = true;
            _horizontalVelocity = Vector3.zero;
            _verticalVelocity = 0f;
        }

        private void StandStill()
        {
            IsRunning = false;
            _horizontalVelocity = Vector3.zero;
            _verticalVelocity = 0f;
        } // GDD §6.5: pego, para de se mexer até reiniciar

        private void UpdateHorizontalVelocity(Vector3 direction, float deltaTime)
        {
            float baseSpeed = RaccoonLocomotion.GetTargetSpeed(direction.magnitude, _input.IsSprinting,
                _data.WalkSpeed, _data.SprintSpeed);
            float speed = GetLoadedSpeed(baseSpeed) * _effects.SpeedMultiplier;
            Vector3 target = direction.sqrMagnitude > 0f ? direction.normalized * speed : Vector3.zero;
            _horizontalVelocity = RaccoonLocomotion.StepHorizontalVelocity(_horizontalVelocity, target,
                _data.Acceleration, _data.Deceleration, deltaTime);
        }

        /// <summary>GDD §7.3 e §8.1: mais lento com peso; com excesso de peso, velocidade mínima.</summary>
        private float GetLoadedSpeed(float baseSpeed)
        {
            if (_inventory.IsOverweight)
            {
                return baseSpeed * _data.OverweightSpeedFraction;
            }

            float penalty = _data.WeightSpeedPenalty * _effects.WeightPenaltyScale;
            return CarrySpeed.Calculate(baseSpeed, _inventory.CurrentWeight, _inventory.MaxWeight, penalty);
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
