using Guaxinim.Core;
using Guaxinim.Noise;
using UnityEngine;

namespace Guaxinim.Player
{
    /// <summary>
    /// Q larga o último item pego, um pouco à frente do guaxinim, com um barulho de 3 m (GDD §7.6).
    /// Se houver parede ou prateleira logo à frente, larga aos pés.
    /// </summary>
    [RequireComponent(typeof(Inventory), typeof(PlayerState))]
    public class RaccoonDropper : MonoBehaviour
    {
        private const float DropDistance = 0.45f;

        [SerializeField] private InputReader _input;
        [SerializeField] private NoiseData _noiseData;

        [Tooltip("Layers que impedem largar à frente (Obstacle)")]
        [SerializeField] private LayerMask _obstacleMask;

        private Inventory _inventory;
        private PlayerState _state;

        private void Awake()
        {
            _inventory = GetComponent<Inventory>();
            _state = GetComponent<PlayerState>();
            if (_input == null || _noiseData == null)
            {
                Debug.LogError("RaccoonDropper needs Input and Noise Data assigned.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (_input != null)
            {
                _input.OnDropPressed += HandleDropPressed;
            }
        }

        private void OnDisable()
        {
            if (_input != null)
            {
                _input.OnDropPressed -= HandleDropPressed;
            }
        }

        private void HandleDropPressed()
        {
            if (_state.IsCaught)
            {
                return;
            }

            Vector3 dropPosition = GetDropPosition();
            if (_inventory.TryDropLast(dropPosition, transform.rotation))
            {
                NoiseSystem.Emit(dropPosition, _noiseData.DropRadius);
            }
        }

        private Vector3 GetDropPosition()
        {
            bool isBlocked = Physics.Raycast(_state.DetectionPoint, transform.forward, DropDistance, _obstacleMask,
                QueryTriggerInteraction.Ignore);
            return isBlocked ? transform.position : transform.position + transform.forward * DropDistance;
        }
    }
}
