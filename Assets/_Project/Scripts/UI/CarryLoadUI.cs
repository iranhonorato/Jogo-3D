using Guaxinim.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Guaxinim.UI
{
    /// <summary>
    /// Barras de peso e volume na HUD (GDD §13). O volume fica laranja quando a carga tapa a visão
    /// (volume 4 ou mais) e o peso fica vermelho com excesso de peso.
    /// </summary>
    public class CarryLoadUI : MonoBehaviour
    {
        [Tooltip("Inventário e efeitos do guaxinim (ligados na montagem da fase)")]
        [SerializeField] private Inventory _inventory;
        [SerializeField] private RaccoonEffects _effects;
        [SerializeField] private RaccoonBulkyLoad _bulkyLoad;

        [Header("Peso")]
        [SerializeField] private RectTransform _weightFill;
        [SerializeField] private Image _weightFillImage;
        [SerializeField] private Text _weightLabel;

        [Header("Volume")]
        [SerializeField] private RectTransform _volumeFill;
        [SerializeField] private Image _volumeFillImage;
        [SerializeField] private Text _volumeLabel;

        [Header("Cores")]
        [SerializeField] private Color _normalColor = new Color(1f, 0.82f, 0.35f);
        [SerializeField] private Color _warningColor = new Color(1f, 0.5f, 0.15f);
        [SerializeField] private Color _overloadColor = new Color(0.95f, 0.15f, 0.1f);

        private void Awake()
        {
            if (_inventory == null || _effects == null || _bulkyLoad == null || _weightFill == null ||
                _volumeFill == null || _weightLabel == null || _volumeLabel == null)
            {
                Debug.LogError("CarryLoadUI is missing references.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (_inventory != null && _effects != null && _bulkyLoad != null)
            {
                _inventory.OnChanged += Refresh;
                _effects.OnChanged += Refresh; // a proteína muda o peso máximo
                _bulkyLoad.OnBulkyChanged += HandleBulkyChanged; // a ordem dos ouvintes não é garantida
            }
        }

        private void Start() => Refresh();

        private void OnDisable()
        {
            if (_inventory != null && _effects != null && _bulkyLoad != null)
            {
                _inventory.OnChanged -= Refresh;
                _effects.OnChanged -= Refresh;
                _bulkyLoad.OnBulkyChanged -= HandleBulkyChanged;
            }
        }

        private void HandleBulkyChanged(bool isBulky) => Refresh();

        private void Refresh()
        {
            SetFill(_weightFill, _inventory.CurrentWeight / _inventory.MaxWeight);
            _weightFillImage.color = _inventory.IsOverweight ? _overloadColor : _normalColor;
            _weightLabel.text = $"Peso {_inventory.CurrentWeight:0}/{_inventory.MaxWeight:0}";

            SetFill(_volumeFill, _inventory.CurrentVolume / _inventory.MaxVolume);
            _volumeFillImage.color = _bulkyLoad.IsBulky ? _warningColor : _normalColor;
            _volumeLabel.text = $"Volume {_inventory.CurrentVolume:0}/{_inventory.MaxVolume:0}";
        }

        private static void SetFill(RectTransform fill, float fraction) =>
            fill.anchorMax = new Vector2(Mathf.Clamp01(fraction), 1f);
    }
}
