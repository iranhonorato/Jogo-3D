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
    /// a lista e os itens bônus, com o botão "Jogar de novo". "Próxima fase" e "Mapa" entram no passo 3.8.
    /// </summary>
    public class LevelResultUI : MonoBehaviour
    {
        private const int SecondsPerMinute = 60;

        [SerializeField] private LevelFlow _levelFlow;
        [SerializeField] private ShoppingList _shoppingList;

        [Tooltip("Painel da tela; começa escondido")]
        [SerializeField] private GameObject _panel;

        [Tooltip("As três estrelas, na ordem: lista, tempo, sem alerta")]
        [SerializeField] private Image[] _stars = new Image[0];

        [SerializeField] private Text _details;
        [SerializeField] private Button _playAgainButton;

        [SerializeField] private Color _earnedStarColor = new Color(1f, 0.82f, 0.2f);
        [SerializeField] private Color _missedStarColor = new Color(0.3f, 0.28f, 0.35f);

        private void Awake()
        {
            if (_levelFlow == null || _shoppingList == null || _panel == null || _details == null ||
                _playAgainButton == null || _stars.Length != 3)
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

        private void HandleLevelWon()
        {
            LevelResult result = _levelFlow.Result;
            SetStar(0, result.IsListComplete);
            SetStar(1, result.BeatTargetTime);
            SetStar(2, !result.HadAlert);
            _details.text = BuildDetails(result, _shoppingList.Progress);
            _panel.SetActive(true);
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(_playAgainButton.gameObject); // controle e teclado
            }
        }

        private void SetStar(int index, bool isEarned) =>
            _stars[index].color = isEarned ? _earnedStarColor : _missedStarColor;

        private static string BuildDetails(LevelResult result, ShoppingListProgress progress)
        {
            var text = new StringBuilder();
            text.AppendLine($"{Mark(result.IsListComplete)} Lista completa");
            text.AppendLine($"{Mark(result.BeatTargetTime)} Tempo {FormatTime(result.ElapsedSeconds)} " +
                $"(meta {FormatTime(result.TargetSeconds)})");
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

        private static string FormatTime(float seconds)
        {
            int wholeSeconds = Mathf.FloorToInt(seconds);
            return $"{wholeSeconds / SecondsPerMinute:00}:{wholeSeconds % SecondsPerMinute:00}";
        }
    }
}
