using System.Collections;
using Guaxinim.Animation;
using Guaxinim.Shared;
using UnityEngine;

namespace Guaxinim.Items
{
    /// <summary>
    /// Item no mundo (GDD §7.2). Com E, vai para quem o pegou (um <see cref="IItemCarrier"/>): voa em arco até a
    /// bolsa, encolhendo, e some. Ao ser pego, o objeto é escondido, e não destruído; largar o mostra de novo.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PickupItem : MonoBehaviour, IInteractable
    {
        [SerializeField] private ItemData _data;

        [Tooltip("Contorno mostrado quando o item é da lista e o guaxinim está perto (GDD §7.1)")]
        [SerializeField] private GameObject _highlight;

        [Header("Voo até a bolsa")]
        [Tooltip("Segundos do voo")]
        [SerializeField, Min(0.01f)] private float _flightSeconds = 0.3f;

        [Tooltip("Metros que o arco sobe no meio do voo")]
        [SerializeField, Min(0f)] private float _flightArcHeight = 0.5f;

        [Tooltip("Metros acima dos pés de quem pegou: onde fica a bolsa")]
        [SerializeField, Min(0f)] private float _bagHeight = 0.35f;

        [Tooltip("Fração do tamanho ao chegar na bolsa")]
        [SerializeField, Range(0.01f, 1f)] private float _endScale = 0.2f;

        private Collider _collider;
        private Vector3 _baseScale;

        public ItemData Data => _data;
        public Vector3 InteractionPoint => transform.position;
        public string PromptText => "Pegar";

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _baseScale = transform.localScale;
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

        /// <summary>Sai do mundo: voa até a bolsa de <paramref name="carrier"/> e fica guardado (escondido).</summary>
        public void StowInto(Transform carrier) => StartFlight(carrier, destroyAtEnd: false);

        /// <summary>Consumível: voa até quem pegou e é usado (destruído no fim do voo).</summary>
        public void ConsumeInto(Transform carrier) => StartFlight(carrier, destroyAtEnd: true);

        /// <summary>Volta ao mundo em <paramref name="position"/> (largado), no tamanho normal.</summary>
        public void PlaceAt(Vector3 position, Quaternion rotation)
        {
            StopAllCoroutines(); // largado no meio do voo
            transform.SetPositionAndRotation(position, rotation);
            transform.localScale = _baseScale;
            gameObject.SetActive(true);
            _collider.enabled = true;
        }

        private void StartFlight(Transform carrier, bool destroyAtEnd)
        {
            _collider.enabled = false; // ninguém pega de novo no meio do voo
            SetHighlighted(false);
            StartCoroutine(FlyTo(carrier, destroyAtEnd));
        }

        private IEnumerator FlyTo(Transform carrier, bool destroyAtEnd)
        {
            Vector3 start = transform.position;
            float elapsed = 0f;
            while (elapsed < _flightSeconds && carrier != null)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / _flightSeconds;
                Vector3 bag = carrier.position + Vector3.up * _bagHeight;
                transform.position = ArcPath.Evaluate(start, bag, _flightArcHeight, progress);
                transform.localScale = _baseScale * Mathf.Lerp(1f, _endScale, progress);
                yield return null;
            }

            if (destroyAtEnd)
            {
                Destroy(gameObject);
                yield break;
            }

            transform.localScale = _baseScale;
            gameObject.SetActive(false);
        }
    }
}
