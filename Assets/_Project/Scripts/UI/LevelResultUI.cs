using System.Text;
using Guaxinim.Core;
using Guaxinim.Items;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Guaxinim.UI
{
    /// <summary>
    /// Tela de resultado provisória (GDD §7.7 e §13): ao vencer, mostra "Fase completa!", o que foi
    /// entregue e o botão "Jogar de novo". As estrelas e a volta ao mapa entram no passo 3.6.
    /// </summary>
    public class LevelResultUI : MonoBehaviour
    {
        [SerializeField] private LevelFlow _levelFlow;
        [SerializeField] private ShoppingList _shoppingList;

        [Tooltip("Painel da tela; começa escondido")]
        [SerializeField] private GameObject _panel;

        [SerializeField] private Text _details;
        [SerializeField] private Button _playAgainButton;

        private void Awake()
        {
            if (_levelFlow == null || _shoppingList == null || _panel == null || _details == null ||
                _playAgainButton == null)
            {
                Debug.LogError("LevelResultUI is missing references.", this);
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
            _details.text = BuildDetails(_shoppingList.Progress);
            _panel.SetActive(true);
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(_playAgainButton.gameObject); // controle e teclado
            }
        }

        private static string BuildDetails(ShoppingListProgress progress)
        {
            var text = new StringBuilder();
            for (int i = 0; i < progress.RequiredCount; i++)
            {
                string mark = progress.IsDelivered(i) ? "[x]" : "[ ]";
                text.AppendLine($"{mark} {progress.GetRequired(i).DisplayName}");
            }

            text.AppendLine();
            text.Append($"Itens bônus: {progress.BonusCount}");
            return text.ToString();
        }
    }
}
