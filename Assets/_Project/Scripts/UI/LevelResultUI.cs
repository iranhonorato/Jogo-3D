using System.Text;
using Guaxinim.Core;
using Guaxinim.Items;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Guaxinim.UI
{
    /// <summary>
    /// Tela de resultado (GDD §12 e §13): ao vencer, mostra as três estrelas e o que cada uma pede, o tempo,
    /// a lista e os itens bônus, com os botões Próxima fase (só quando já está liberada), Jogar de novo e Mapa.
    /// </summary>
    public class LevelResultUI : MonoBehaviour
    {
        [SerializeField] private LevelFlow _levelFlow;
        [SerializeField] private ShoppingList _shoppingList;

        [Tooltip("Para achar a próxima fase")]
        [SerializeField] private LevelCatalog _catalog;

        [Tooltip("Painel da tela; começa escondido")]
        [SerializeField] private GameObject _panel;

        [Tooltip("As três estrelas, na ordem: lista, tempo, sem alerta")]
        [SerializeField] private Image[] _stars = new Image[0];

        [SerializeField] private Text _details;
        [SerializeField] private Button _nextLevelButton;
        [SerializeField] private Button _playAgainButton;

        [SerializeField] private Color _earnedStarColor = new Color(1f, 0.82f, 0.2f);
        [SerializeField] private Color _missedStarColor = new Color(0.3f, 0.28f, 0.35f);

        private LevelData _nextLevel;

        private void Awake()
        {
            if (_levelFlow == null || _shoppingList == null || _catalog == null || _panel == null ||
                _details == null || _nextLevelButton == null || _playAgainButton == null || _stars.Length != 3)
            {
                Debug.LogError("LevelResultUI is missing references (it needs 3 stars).", this);
                enabled = false;
                return;
            }

            _panel.SetActive(false);
        }

        private void OnEnable()
        {
            if (_levelFlow != null)
            {
                _levelFlow.OnLevelWon += HandleLevelWon;
            }
        }

        private void OnDisable()
        {
            if (_levelFlow != null)
            {
                _levelFlow.OnLevelWon -= HandleLevelWon;
            }
        }

        /// <summary>Ligado ao botão "Jogar de novo" no Inspector.</summary>
        public void PlayAgain() => SceneLoader.ReloadActiveScene();

        /// <summary>Ligado ao botão "Próxima fase" no Inspector.</summary>
        public void PlayNextLevel() => SceneLoader.LoadLevel(_nextLevel);

        /// <summary>Ligado ao botão "Mapa" no Inspector.</summary>
        public void GoToCityMap() => SceneLoader.LoadCityMap();

        private void HandleLevelWon()
        {
            LevelResult result = _levelFlow.Result;
            SetStar(0, result.IsListComplete);
            SetStar(1, result.BeatTargetTime);
            SetStar(2, !result.HadAlert);
            _details.text = BuildDetails(result, _shoppingList.Progress);

            // O LevelFlow já gravou o resultado, então a próxima fase aparece liberada aqui.
            bool hasNextLevel = _catalog.TryGetNext(_levelFlow.Level, SaveSystem.Data, out _nextLevel);
            _nextLevelButton.gameObject.SetActive(hasNextLevel);
            _panel.SetActive(true);
            if (EventSystem.current != null)
            {
                Button firstChoice = hasNextLevel ? _nextLevelButton : _playAgainButton;
                EventSystem.current.SetSelectedGameObject(firstChoice.gameObject); // controle e teclado
            }
        }

        private void SetStar(int index, bool isEarned) =>
            _stars[index].color = isEarned ? _earnedStarColor : _missedStarColor;

        private static string BuildDetails(LevelResult result, ShoppingListProgress progress)
        {
            var text = new StringBuilder();
            text.AppendLine($"{Mark(result.IsListComplete)} Lista completa");
            text.AppendLine($"{Mark(result.BeatTargetTime)} Tempo {TimeFormat.MinutesAndSeconds(result.ElapsedSeconds)} " +
                $"(meta {TimeFormat.MinutesAndSeconds(result.TargetSeconds)})");
            text.AppendLine($"{Mark(!result.HadAlert)} Sem alerta geral");
            text.AppendLine();
            for (int i = 0; i < progress.RequiredCount; i++)
            {
                text.AppendLine($"{Mark(progress.IsDelivered(i))} {progress.GetRequired(i).DisplayName}");
            }

            text.AppendLine();
            text.Append($"Itens bônus: {result.BonusCount}  (+{result.BonusPoints} pontos)");
            return text.ToString();
        }

        private static string Mark(bool isDone) => isDone ? "[x]" : "[ ]";
    }
}
