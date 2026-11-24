using System;
using Guaxinim.Core;
using Guaxinim.Shared;
using UnityEngine;

namespace Guaxinim.Player
{
    /// <summary>
    /// Interação com E (GDD §3.2): procura os <see cref="IInteractable"/> por perto, na layer Interactable,
    /// e usa o mais próximo. Avisa em <see cref="OnTargetChanged"/> quando o mais próximo muda, para a HUD mostrar
    /// a indicação "E Pegar" sobre ele.
    /// </summary>
    [RequireComponent(typeof(PlayerState))]
    public class RaccoonInteractor : MonoBehaviour
    {
        private const int MaxNearbyColliders = 16;

        [SerializeField] private PlayerData _data;
        [SerializeField] private InputReader _input;

        [Tooltip("Layers com coisas que respondem ao E (Interactable)")]
        [SerializeField] private LayerMask _interactableMask;

        private readonly Collider[] _nearby = new Collider[MaxNearbyColliders];
        private PlayerState _state;

        /// <summary>Muda para o mais próximo ao alcance do E, ou para null quando não há nada ao alcance.</summary>
        public event Action<IInteractable> OnTargetChanged;

        /// <summary>O que o E usaria agora, ou null.</summary>
        public IInteractable CurrentTarget { get; private set; }

        private void Awake()
        {
            _state = GetComponent<PlayerState>();
            if (_data == null || _input == null)
            {
                Debug.LogError("RaccoonInteractor needs Data and Input assigned.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (_input != null)
            {
                _input.OnInteractPressed += HandleInteractPressed;
            }
        }

        private void Update()
        {
            IInteractable target = null;
            if (!_state.IsCaught)
            {
                TryGetClosestInteractable(out target);
            }

            if (!ReferenceEquals(target, CurrentTarget))
            {
                CurrentTarget = target;
                OnTargetChanged?.Invoke(target);
            }
        }

        private void OnDisable()
        {
            if (_input != null)
            {
                _input.OnInteractPressed -= HandleInteractPressed;
            }
        }

        public bool TryGetClosestInteractable(out IInteractable closest)
        {
            closest = null;
            Vector3 center = _state.DetectionPoint;
            int count = Physics.OverlapSphereNonAlloc(center, _data.InteractionRadius, _nearby, _interactableMask,
                QueryTriggerInteraction.Collide);

            float closestSqrDistance = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                if (!_nearby[i].TryGetComponent(out IInteractable candidate))
                {
                    continue;
                }

                float sqrDistance = (candidate.InteractionPoint - center).sqrMagnitude;
                if (sqrDistance < closestSqrDistance)
                {
                    closestSqrDistance = sqrDistance;
                    closest = candidate;
                }
            }

            return closest != null;
        }

        private void HandleInteractPressed()
        {
            if (_state.IsCaught || !TryGetClosestInteractable(out IInteractable target))
            {
                return;
            }

            target.Interact(gameObject);
        }
    }
}
