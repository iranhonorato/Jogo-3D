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
    /// Escolher uma fase liberada abre o início da fase (nome, lista de compras e botão Começar). Vindo de uma vitória
    /// que liberou a próxima fase, toca antes a viagem do guaxinim pela estrada (Etapa 4) e abre o início dela.
    /// </summary>
    public class CityMapUI : MonoBehaviour
    {
        [SerializeField] private LevelCatalog _catalog;

        [Tooltip("Uma fachada por fase do catálogo, na mesma ordem")]
        [SerializeField] private LevelButtonUI[] _levelButtons = new LevelButtonUI[0];

        [SerializeField] private GameObject _mapPanel;

        [Tooltip("Do painel do mapa: desligado durante a viagem, para um clique não abrir outra fase")]
        [SerializeField] private CanvasGroup _mapGroup;

        [SerializeField] private Button _menuButton;
        [SerializeField] private MapTravelAnimator _travel;

        [Header("Início da fase")]
        [SerializeField] private GameObject _briefingPanel;
        [SerializeField] private Text _briefingTitle;
        [SerializeField] private Text _briefingBody;
        [SerializeField] private Button _startButton;

        private LevelData _selectedLevel;
        private int _travelDestination = -1;

        private void Awake()
        {
            if (_catalog == null || _mapPanel == null || _mapGroup == null || _menuButton == null || _travel == null ||
                _briefingPanel == null || _briefingTitle == null || _briefingBody == null || _startButton == null)
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

            _travel.OnArrived += HandleTravelArrived;
            _travel.OnFinished += HandleTravelFinished;
        }

        private void Start()
        {
            RefreshLevelButtons();
            if (TryStartTravel())
            {
                return;
            }

            _travel.PlaceAt(FindLastUnlockedIndex());
            ShowMap();
        }

        private void OnDisable()
        {
            foreach (LevelButtonUI levelButton in _levelButtons)
            {
                levelButton.OnSelected -= HandleLevelSelected;
            }

            _travel.OnArrived -= HandleTravelArrived;
            _travel.OnFinished -= HandleTravelFinished;
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

        /// <summary>Viagem pedida pelo resultado da fase: o destino aparece bloqueado até o marcador chegar.</summary>
        private bool TryStartTravel()
        {
            if (!MapTravelRequest.TryConsume(out LevelData from, out LevelData to))
            {
                return false;
            }

            int fromIndex = _catalog.IndexOf(from);
            int toIndex = _catalog.IndexOf(to);
            if (fromIndex < 0 || toIndex <= fromIndex)
            {
                return false;
            }

            _travelDestination = toIndex;
            _levelButtons[toIndex].Bind(to, SaveSystem.Data.Find(to.Id), false);
            _briefingPanel.SetActive(false);
            _mapPanel.SetActive(true);
            SetMapInteractable(false);
            Deselect();
            _travel.Play(fromIndex, toIndex, _levelButtons[toIndex].transform);
            return true;
        }

        private void HandleTravelArrived() => RefreshLevelButtons();

        private void HandleTravelFinished()
        {
            SetMapInteractable(true);
            HandleLevelSelected(_catalog.Levels[_travelDestination]);
        }

        private void SetMapInteractable(bool isInteractable)
        {
            _mapGroup.interactable = isInteractable;
            _mapGroup.blocksRaycasts = isInteractable;
        }

        private int FindLastUnlockedIndex()
        {
            SaveData save = SaveSystem.Data;
            int last = 0;
            for (int i = 0; i < _catalog.Levels.Count; i++)
            {
                if (_catalog.Levels[i].IsPlayable && _catalog.IsUnlocked(i, save))
                {
                    last = i;
                }
            }

            return last;
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

        private static void Deselect()
        {
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
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
