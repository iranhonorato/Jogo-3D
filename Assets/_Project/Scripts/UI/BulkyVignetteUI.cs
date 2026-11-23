using Guaxinim.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Guaxinim.UI
{
    /// <summary>
    /// Vinheta que escurece as bordas da tela com carga volumosa (GDD §7.4). A textura radial é gerada
    /// uma vez, por código, e a vinheta aparece e some com um fade curto.
    /// </summary>
    public class BulkyVignetteUI : MonoBehaviour
    {
        private const int TextureSize = 128;
        private const float ClearCenterFraction = 0.45f; // centro da tela fica limpo

        [SerializeField] private RaccoonBulkyLoad _bulkyLoad;
        [SerializeField] private RawImage _image;

        [Tooltip("Opacidade máxima das bordas, de 0 a 1")]
        [SerializeField, Range(0f, 1f)] private float _maxAlpha = 0.85f;

        [Tooltip("Segundos para a vinheta aparecer ou sumir")]
        [SerializeField, Min(0.01f)] private float _fadeSeconds = 0.35f;

        [SerializeField] private Color _color = new Color(0.04f, 0.02f, 0.08f);

        private Texture2D _texture;
        private float _targetAlpha;
        private float _alpha;

        private void Awake()
        {
            if (_bulkyLoad == null || _image == null)
            {
                Debug.LogError("BulkyVignetteUI needs Bulky Load and Image assigned.", this);
                enabled = false;
                return;
            }

            _texture = CreateRadialTexture();
            _image.texture = _texture;
            ApplyAlpha(0f);
        }

        private void OnEnable()
        {
            if (_bulkyLoad != null)
            {
                _bulkyLoad.OnBulkyChanged += HandleBulkyChanged;
            }
        }

        private void Update()
        {
            if (Mathf.Approximately(_alpha, _targetAlpha))
            {
                return;
            }

            float step = _maxAlpha * Time.deltaTime / _fadeSeconds;
            ApplyAlpha(Mathf.MoveTowards(_alpha, _targetAlpha, step));
        }

        private void OnDisable()
        {
            if (_bulkyLoad != null)
            {
                _bulkyLoad.OnBulkyChanged -= HandleBulkyChanged;
            }
        }

        private void OnDestroy()
        {
            if (_texture != null)
            {
                Destroy(_texture);
            }
        }

        private void HandleBulkyChanged(bool isBulky) => _targetAlpha = isBulky ? _maxAlpha : 0f;

        private void ApplyAlpha(float alpha)
        {
            _alpha = alpha;
            _image.enabled = alpha > 0f;
            Color color = _color;
            color.a = alpha;
            _image.color = color;
        }

        private static Texture2D CreateRadialTexture()
        {
            var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
            };
            var pixels = new Color32[TextureSize * TextureSize];
            float half = (TextureSize - 1) / 2f;
            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    float distance = new Vector2((x - half) / half, (y - half) / half).magnitude;
                    float edge = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(ClearCenterFraction, 1.2f, distance));
                    pixels[y * TextureSize + x] = new Color32(255, 255, 255, (byte)(edge * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }
    }
}
