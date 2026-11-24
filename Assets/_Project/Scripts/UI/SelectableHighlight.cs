using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Guaxinim.UI
{
    /// <summary>
    /// Destaque do botão selecionado (GDD §13). No estilo Botão, a seleção troca a cor do fundo e do texto, engorda
    /// o contorno, aumenta o botão e mostra uma seta; no estilo Cartão (fachadas do mapa), só o contorno e o
    /// tamanho mudam, e as cores ficam com o próprio cartão. Passar o mouse seleciona o botão, para só existir
    /// um destaque por vez, venha ele do mouse, do teclado ou do controle.
    /// </summary>
    public class SelectableHighlight : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler,
        IPointerDownHandler, IPointerUpHandler
    {
        public enum HighlightStyle
        {
            Button,
            Card,
        }

        [SerializeField] private UITheme _theme;
        [SerializeField] private HighlightStyle _style;
        [SerializeField] private Button _button;
        [SerializeField] private Image _background;

        [Tooltip("Texto do botão (só no estilo Botão)")]
        [SerializeField] private Text _label;

        [SerializeField] private Outline _outline;

        [Tooltip("Seta mostrada só enquanto o botão está selecionado (só no estilo Botão)")]
        [SerializeField] private Text _arrow;

        private bool _isSelected;
        private bool _isPressed;

        private void Awake()
        {
            bool isButtonStyleComplete = _style == HighlightStyle.Card || (_background != null && _label != null);
            if (_theme == null || _button == null || _outline == null || !isButtonStyleComplete)
            {
                Debug.LogError("SelectableHighlight is missing references.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            _isPressed = false;
            _isSelected = EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject;
            Refresh();
        }

        private void OnDisable() => transform.localScale = Vector3.one;

        public void OnSelect(BaseEventData eventData)
        {
            _isSelected = true;
            Refresh();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            _isSelected = false;
            Refresh();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_button.interactable && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(gameObject);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPressed = true;
            Refresh();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;
            Refresh();
        }

        /// <summary>Reaplica o visual; chamar depois de mudar o <c>interactable</c> do botão.</summary>
        public void Refresh()
        {
            if (!enabled)
            {
                return;
            }

            bool isInteractable = _button.interactable;
            bool isHighlighted = isInteractable && _isSelected;
            ApplyOutline(isHighlighted);
            transform.localScale = Vector3.one * (isHighlighted && !_isPressed ? _theme.SelectedScale : 1f);
            if (_style == HighlightStyle.Button)
            {
                ApplyButtonColors(isInteractable);
            }
        }

        private void ApplyOutline(bool isHighlighted)
        {
            _outline.effectColor = isHighlighted ? _theme.SelectedOutline : _theme.NormalOutline;
            float width = isHighlighted ? _theme.ThickOutline : _theme.ThinOutline;
            _outline.effectDistance = new Vector2(width, -width);
        }

        private void ApplyButtonColors(bool isInteractable)
        {
            if (!isInteractable)
            {
                SetColors(_theme.DisabledBackground, _theme.DisabledText, false);
                return;
            }

            if (_isPressed)
            {
                SetColors(_theme.PressedBackground, _theme.SelectedText, false);
                return;
            }

            if (_isSelected)
            {
                SetColors(_theme.SelectedBackground, _theme.SelectedText, true);
                return;
            }

            SetColors(_theme.NormalBackground, _theme.NormalText, false);
        }

        private void SetColors(Color background, Color text, bool isArrowVisible)
        {
            _background.color = background;
            _label.color = text;
            if (_arrow != null)
            {
                _arrow.text = _theme.SelectionArrow;
                _arrow.color = text;
                _arrow.enabled = isArrowVisible;
            }
        }
    }
}
