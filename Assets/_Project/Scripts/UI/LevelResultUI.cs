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
    /// a lista e os itens bônus, e o aviso "Novo recorde!" quando o melhor tempo melhora. Os botões são Mapa,
    /// Jogar de novo e Próxima fase (só quando já está liberada); na última fase, o botão vira "Concluir" e abre a
    /// tela de fim de jogo.
    /// </summary>
    public class LevelResultUI : MonoBehaviour
    {
        private const string NextLevelText = "Próxima fase";
        private const string FinishText = "Concluir";

        [SerializeField] private LevelFlow _levelFlow;
        [SerializeField] private ShoppingList _shoppingList;

        [Tooltip("Para achar a próxima fase e saber se esta é a última")]
        [SerializeField] private LevelCatalog _catalog;

        [Tooltip("Painel da tela; começa escondido")]
        [SerializeField] private GameObject _panel;

        [Tooltip("As três estrelas, na ordem: lista, tempo, sem ser perseguido")]
        [SerializeField] private Image[] _stars = new Image[0];

        [SerializeField] private Text _details;
        [SerializeField] private Text _recordLabel;

        [Tooltip("Próxima fase; na última fase vira Concluir")]
        [SerializeField] private Button _nextLevelButton;

        [SerializeField] private Text _nextLevelLabel;
        [SerializeField] private Button _playAgainButton;

        [SerializeField] private Color _earnedStarColor = new Color32(0xFF, 0xC8, 0x4D, 0xFF);
        [SerializeField] private Color _missedStarColor = new Color(0.16f, 0.12f, 0.24f, 0.35f);

        private LevelData _nextLevel;
        private bool _isLastLevel;

        private void Awake()
        {
            if (_levelFlow == null || _shoppingList == null || _catalog == null || _panel == null ||
                _details == null || _recordLabel == null || _nextLevelButton == null || _nextLevelLabel == null ||
                _playAgainButton == null || _stars.Length != LevelResult.MaxStars)
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

        /// <summary>Ligado ao botão "Próxima fase" (ou "Concluir") no Inspector.</summary>
        public void ContinueCampaign()
        {
            if (_isLastLevel)
            {
                SceneLoader.LoadEnding();
                return;
            }

            SceneLoader.LoadLevel(_nextLevel);
        }

        /// <summary>Ligado ao botão "Mapa" no Inspector.</summary>
        public void GoToCityMap() => SceneLoader.LoadCityMap();

        private void HandleLevelWon()
        {
            LevelResult result = _levelFlow.Result;
            SetStar(0, result.IsListComplete);
            SetStar(1, result.BeatTargetTime);
            SetStar(2, !result.WasChased);
            _details.text = BuildDetails(result, _shoppingList.Progress);
            _recordLabel.gameObject.SetActive(_levelFlow.IsNewRecord);

            // O LevelFlow já gravou o resultado, então a próxima fase aparece liberada aqui.
            bool hasNextLevel = _catalog.TryGetNext(_levelFlow.Level, SaveSystem.Data, out _nextLevel);
            _isLastLevel = _catalog.IsLastPlayable(_levelFlow.Level);
            bool canContinue = hasNextLevel || _isLastLevel;
            _nextLevelButton.gameObject.SetActive(canContinue);
            _nextLevelLabel.text = _isLastLevel ? FinishText : NextLevelText;
            _panel.SetActive(true);
            if (EventSystem.current != null)
            {
                Button firstChoice = canContinue ? _nextLevelButton : _playAgainButton;
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
            text.AppendLine($"{Mark(!result.WasChased)} Sem ser perseguido");
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
