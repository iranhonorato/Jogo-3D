using System;
using Guaxinim.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Guaxinim.UI
{
    /// <summary>
    /// Fachada de uma fase no mapa da cidade (GDD §13): nome, estrelas e melhor tempo. Fase sem cena aparece como
    /// "Em breve", e fase que ainda não liberou aparece cinza e como "Bloqueada"; as duas não podem ser escolhidas.
    /// </summary>
    public class LevelButtonUI : MonoBehaviour
    {
        private const string ComingSoonText = "Em breve";
        private const string LockedText = "Bloqueada";
        private const string AvailableText = "Disponível";

        [SerializeField] private Button _button;

        [Tooltip("Destaque da seleção (contorno e tamanho); precisa ser avisado quando a fase libera ou bloqueia")]
        [SerializeField] private SelectableHighlight _highlight;

        [SerializeField] private Image _background;
        [SerializeField] private Text _nameLabel;
        [SerializeField] private Text _statusLabel;

        [Tooltip("As três estrelas da fase, na ordem: lista, tempo, sem alerta")]
        [SerializeField] private Image[] _stars = new Image[0];

        [SerializeField] private Color _unlockedColor = new Color(1f, 0.78f, 0.3f);
        [SerializeField] private Color _lockedColor = new Color(0.35f, 0.33f, 0.4f);
        [SerializeField] private Color _unlockedTextColor = new Color(0.12f, 0.07f, 0.15f);
        [SerializeField] private Color _lockedTextColor = new Color(0.7f, 0.68f, 0.75f);
        [SerializeField] private Color _earnedStarColor = new Color(0.55f, 0.3f, 0.05f);
        [SerializeField] private Color _missedStarColor = new Color(0.12f, 0.07f, 0.15f, 0.25f);

        private LevelData _level;

        public event Action<LevelData> OnSelected;

        public Button Button => _button;

        public bool IsSelectable => _button.interactable;

        private void Awake()
        {
            if (_button == null || _highlight == null || _background == null || _nameLabel == null ||
                _statusLabel == null || _stars.Length != 3)
            {
                Debug.LogError("LevelButtonUI is missing references (it needs 3 stars).", this);
                enabled = false;
            }
        }

        /// <summary>Mostra a fase. O registro é null se ela nunca foi completa.</summary>
        public void Bind(LevelData level, LevelRecord record, bool isUnlocked)
        {
            _level = level;
            _nameLabel.text = level.DisplayName;
            _button.interactable = isUnlocked;
            _highlight.Refresh();
            _background.color = isUnlocked ? _unlockedColor : _lockedColor;
            _nameLabel.color = isUnlocked ? _unlockedTextColor : _lockedTextColor;
            _statusLabel.color = _nameLabel.color;
            _statusLabel.text = BuildStatus(level, record, isUnlocked);

            int earnedStars = record != null ? record.BestStars : 0;
            for (int i = 0; i < _stars.Length; i++)
            {
                _stars[i].color = i < earnedStars ? _earnedStarColor : _missedStarColor;
            }
        }

        /// <summary>Ligado ao botão no Inspector.</summary>
        public void Select() => OnSelected?.Invoke(_level);

        private static string BuildStatus(LevelData level, LevelRecord record, bool isUnlocked)
        {
            if (!level.IsPlayable)
            {
                return ComingSoonText;
            }

            if (!isUnlocked)
            {
                return LockedText;
            }

            bool wasCompleted = record != null && record.IsCompleted;
            return wasCompleted ? $"Melhor {TimeFormat.MinutesAndSeconds(record.BestTimeSeconds)}" : AvailableText;
        }
    }
}
