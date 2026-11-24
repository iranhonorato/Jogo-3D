using System.Text;
using Guaxinim.Core;
using Guaxinim.Items;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Guaxinim.UI
{
    /// <summary>
    /// Mapa da cidade (GDD §12 e §13): uma fachada por fase do catálogo, com estrelas e melhor tempo do save.
    /// Escolher uma fase liberada abre o início da fase (nome, lista de compras e botão Começar).
    /// </summary>
    public class CityMapUI : MonoBehaviour
    {
        [SerializeField] private LevelCatalog _catalog;

        [Tooltip("Uma fachada por fase do catálogo, na mesma ordem")]
        [SerializeField] private LevelButtonUI[] _levelButtons = new LevelButtonUI[0];

        [SerializeField] private GameObject _mapPanel;
        [SerializeField] private Button _menuButton;

        [Header("Início da fase")]
        [SerializeField] private GameObject _briefingPanel;
        [SerializeField] private Text _briefingTitle;
        [SerializeField] private Text _briefingBody;
        [SerializeField] private Button _startButton;

        private LevelData _selectedLevel;

        private void Awake()
        {
            if (_catalog == null || _mapPanel == null || _menuButton == null || _briefingPanel == null ||
                _briefingTitle == null || _briefingBody == null || _startButton == null)
            {
                Debug.LogError("CityMapUI is missing references.", this);
                enabled = false;
                return;
            }

            if (_levelButtons.Length != _catalog.Levels.Count)
            {
                Debug.LogError($"CityMapUI has {_levelButtons.Length} level buttons for {_catalog.Levels.Count} levels.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            foreach (LevelButtonUI levelButton in _levelButtons)
            {
                levelButton.OnSelected += HandleLevelSelected;
            }
        }

        private void Start()
        {
            RefreshLevelButtons();
            ShowMap();
        }

        private void OnDisable()
        {
            foreach (LevelButtonUI levelButton in _levelButtons)
            {
                levelButton.OnSelected -= HandleLevelSelected;
            }
        }

        /// <summary>Ligado ao botão Começar no Inspector.</summary>
        public void StartSelectedLevel() => SceneLoader.LoadLevel(_selectedLevel);

        /// <summary>Ligado ao botão Voltar do início da fase no Inspector.</summary>
        public void CloseBriefing() => ShowMap();

        /// <summary>Ligado ao botão Menu no Inspector.</summary>
        public void GoToMainMenu() => SceneLoader.LoadMainMenu();

        private void RefreshLevelButtons()
        {
            SaveData save = SaveSystem.Data;
            for (int i = 0; i < _levelButtons.Length; i++)
            {
                LevelData level = _catalog.Levels[i];
                _levelButtons[i].Bind(level, save.Find(level.Id), _catalog.IsUnlocked(i, save));
            }
        }

        private void HandleLevelSelected(LevelData level)
        {
            _selectedLevel = level;
            _briefingTitle.text = level.DisplayName;
            _briefingBody.text = BuildBriefing(level, SaveSystem.Data.Find(level.Id));
            _mapPanel.SetActive(false);
            _briefingPanel.SetActive(true);
            Select(_startButton);
        }

        private void ShowMap()
        {
            _briefingPanel.SetActive(false);
            _mapPanel.SetActive(true);
            Select(FindFirstChoice());
        }

        /// <summary>A fase que o jogador acabou de ver; senão a primeira liberada; senão o botão do menu.</summary>
        private Button FindFirstChoice()
        {
            int lastIndex = _catalog.IndexOf(_selectedLevel);
            if (lastIndex >= 0)
            {
                return _levelButtons[lastIndex].Button;
            }

            foreach (LevelButtonUI levelButton in _levelButtons)
            {
                if (levelButton.IsSelectable)
                {
                    return levelButton.Button;
                }
            }

            return _menuButton;
        }

        private static void Select(Button button)
        {
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(button.gameObject); // teclado e controle
            }
        }

        private static string BuildBriefing(LevelData level, LevelRecord record)
        {
            var text = new StringBuilder();
            text.AppendLine("Lista de compras");
            foreach (ItemData item in level.ShoppingList)
            {
                text.AppendLine($"  - {item.DisplayName}");
            }

            text.AppendLine();
            text.AppendLine($"Tempo-alvo: {TimeFormat.MinutesAndSeconds(level.TargetTimeSeconds)}");
            if (record != null && record.IsCompleted)
            {
                text.Append($"Seu melhor: {record.BestStars} de 3 estrelas, em " +
                    $"{TimeFormat.MinutesAndSeconds(record.BestTimeSeconds)}");
            }

            return text.ToString();
        }
    }
}
