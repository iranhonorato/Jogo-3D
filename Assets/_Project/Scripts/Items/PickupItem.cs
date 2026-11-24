using Guaxinim.Shared;
using UnityEngine;

namespace Guaxinim.Items
{
    /// <summary>
    /// Item no mundo (GDD §7.2). Com E, vai para quem o pegou (um <see cref="IItemCarrier"/>). Ao ser pego,
    /// o objeto é escondido, e não destruído; largar o mostra de novo em outro lugar.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PickupItem : MonoBehaviour, IInteractable
    {
        [SerializeField] private ItemData _data;

        [Tooltip("Contorno mostrado quando o item é da lista e o guaxinim está perto (GDD §7.1)")]
        [SerializeField] private GameObject _highlight;

        public ItemData Data => _data;
        public Vector3 InteractionPoint => transform.position;
        public string PromptText => "Pegar";

        private void Awake()
        {
            if (_data == null)
            {
                Debug.LogError("PickupItem needs Item Data assigned.", this);
            }
        }

        public void Interact(GameObject interactor)
        {
            if (interactor.TryGetComponent(out IItemCarrier carrier))
            {
                carrier.TryPickUp(this);
            }
        }

        public void SetHighlighted(bool isHighlighted)
        {
            if (_highlight != null && _highlight.activeSelf != isHighlighted)
            {
                _highlight.SetActive(isHighlighted);
            }
        }

        /// <summary>Sai do mundo: está sendo carregado.</summary>
        public void Stow() => gameObject.SetActive(false);

        /// <summary>Volta ao mundo em <paramref name="position"/> (largado).</summary>
        public void PlaceAt(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
            gameObject.SetActive(true);
        }
    }
}
