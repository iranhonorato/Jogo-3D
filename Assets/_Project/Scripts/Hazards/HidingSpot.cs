using Guaxinim.Shared;
using UnityEngine;

namespace Guaxinim.Hazards
{
    /// <summary>
    /// Lixeira (GDD §9): E entra e E sai. Dentro, o guaxinim fica invisível. Uma lixeira só guarda um
    /// por vez; quem não pode se esconder agora (carga volumosa, por exemplo) é recusado.
    /// </summary>
    public class HidingSpot : MonoBehaviour, IInteractable
    {
        [Tooltip("Onde o guaxinim reaparece ao sair")]
        [SerializeField] private Transform _exitPoint;

        private IHider _occupant;

        public Vector3 InteractionPoint => transform.position;

        private void Awake()
        {
            if (_exitPoint == null)
            {
                Debug.LogError("HidingSpot needs an Exit Point assigned.", this);
                enabled = false;
            }
        }

        public void Interact(GameObject interactor)
        {
            if (!enabled || !interactor.TryGetComponent(out IHider hider) || hider.IsInTransition)
            {
                return;
            }

            if (_occupant == hider)
            {
                _occupant = null;
                hider.ExitHiding(_exitPoint.position);
                return;
            }

            // TODO(passo 3.5): avisar na HUD quando não cabe (carga volumosa).
            if (_occupant != null || !hider.CanHide)
            {
                return;
            }

            _occupant = hider;
            hider.EnterHiding(transform.position);
        }
    }
}
