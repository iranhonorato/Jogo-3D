using Guaxinim.Guards;
using UnityEngine;

namespace Guaxinim.UI
{
    /// <summary>
    /// Barra sobre a cabeça do segurança que mostra o <see cref="DetectionMeter"/> (GDD §6.3): some
    /// quando vazia, cresce da esquerda para a direita e vai do amarelo ao vermelho. Sempre de frente
    /// para a câmera.
    /// </summary>
    public class DetectionMeterIcon : MonoBehaviour
    {
        private const float FillDepthOffset = -0.01f; // na frente do fundo, do lado da câmera

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private DetectionMeter _meter;

        [Tooltip("Objeto com o fundo e a barra; fica escondido com o medidor vazio")]
        [SerializeField] private GameObject _visualRoot;

        [SerializeField] private Transform _fill;
        [SerializeField] private Renderer _fillRenderer;

        [Tooltip("Metros de largura da barra cheia")]
        [SerializeField, Min(0.01f)] private float _width = 0.8f;

        [Tooltip("Metros de altura da barra")]
        [SerializeField, Min(0.01f)] private float _height = 0.12f;

        [SerializeField] private Color _lowColor = new Color(1f, 0.85f, 0.2f);
        [SerializeField] private Color _fullColor = new Color(0.95f, 0.15f, 0.1f);

        private MaterialPropertyBlock _propertyBlock;
        private Transform _cameraTransform;

        private void Awake()
        {
            if (_meter == null || _visualRoot == null || _fill == null || _fillRenderer == null)
            {
                Debug.LogError("DetectionMeterIcon is missing references.", this);
                enabled = false;
                return;
            }

            _propertyBlock = new MaterialPropertyBlock();
        }

        private void Start()
        {
            Camera mainCamera = Camera.main; // só no setup: nunca dentro do LateUpdate
            if (mainCamera != null)
            {
                _cameraTransform = mainCamera.transform;
            }
        }

        private void LateUpdate()
        {
            float value = _meter.Value;
            bool isVisible = value > 0f;
            if (_visualRoot.activeSelf != isVisible)
            {
                _visualRoot.SetActive(isVisible);
            }

            if (!isVisible)
            {
                return;
            }

            _fill.localScale = new Vector3(_width * value, _height, 1f);
            _fill.localPosition = new Vector3(-_width * (1f - value) / 2f, 0f, FillDepthOffset);
            _propertyBlock.SetColor(BaseColorId, Color.Lerp(_lowColor, _fullColor, value));
            _fillRenderer.SetPropertyBlock(_propertyBlock);

            if (_cameraTransform != null)
            {
                transform.rotation = _cameraTransform.rotation;
            }
        }
    }
}
