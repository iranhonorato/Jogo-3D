using UnityEngine;

namespace Guaxinim.UI
{
    /// <summary>
    /// Cores e medidas dos botões (GDD §13): normal, selecionado, pressionado e desativado. Os montadores de tela
    /// e o componente de destaque leem daqui, para mudar o visual em um lugar só. Somente leitura durante o jogo.
    /// </summary>
    [CreateAssetMenu(fileName = "UITheme", menuName = "Guaxinim/UI Theme")]
    public class UITheme : ScriptableObject
    {
        [Header("Botão normal")]
        [SerializeField] private Color _normalBackground = new Color32(0x2A, 0x1F, 0x3D, 0xFF);
        [SerializeField] private Color _normalText = new Color32(0xFF, 0xE0, 0xB3, 0xFF);
        [SerializeField] private Color _normalOutline = new Color32(0x8A, 0x74, 0xB0, 0xFF);

        [Header("Botão selecionado (teclado, controle ou mouse)")]
        [SerializeField] private Color _selectedBackground = new Color32(0xFF, 0xC8, 0x4D, 0xFF);
        [SerializeField] private Color _selectedText = new Color32(0x1F, 0x12, 0x26, 0xFF);
        [SerializeField] private Color _selectedOutline = new Color32(0xFF, 0xF1, 0xD6, 0xFF);

        [Tooltip("Tamanho do botão selecionado em relação ao normal")]
        [SerializeField, Min(1f)] private float _selectedScale = 1.08f;

        [Header("Botão pressionado")]
        [SerializeField] private Color _pressedBackground = new Color32(0xD9, 0x9A, 0x1E, 0xFF);

        [Header("Botão desativado")]
        [SerializeField] private Color _disabledBackground = new Color(0.5f, 0.5f, 0.55f, 0.4f);
        [SerializeField] private Color _disabledText = new Color(0.78f, 0.76f, 0.82f, 1f);

        [Header("Contorno")]
        [Tooltip("Pixels de contorno do botão normal")]
        [SerializeField, Min(0f)] private float _thinOutline = 1.5f;

        [Tooltip("Pixels de contorno do botão selecionado")]
        [SerializeField, Min(0f)] private float _thickOutline = 4f;

        [Header("Seta ao lado do botão selecionado")]
        [SerializeField] private string _selectionArrow = "►";

        public Color NormalBackground => _normalBackground;
        public Color NormalText => _normalText;
        public Color NormalOutline => _normalOutline;
        public Color SelectedBackground => _selectedBackground;
        public Color SelectedText => _selectedText;
        public Color SelectedOutline => _selectedOutline;
        public float SelectedScale => _selectedScale;
        public Color PressedBackground => _pressedBackground;
        public Color DisabledBackground => _disabledBackground;
        public Color DisabledText => _disabledText;
        public float ThinOutline => _thinOutline;
        public float ThickOutline => _thickOutline;
        public string SelectionArrow => _selectionArrow;
    }
}
