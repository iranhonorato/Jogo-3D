using System.Collections.Generic;
using Guaxinim.Core;
using Guaxinim.Items;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Guaxinim.UI
{
    /// <summary>
    /// Tela "Você foi pego!" (GDD §13): com o jogo congelado, mostra o tempo, quantos itens da lista já tinham sido
    /// entregues e uma dica sorteada, e deixa o jogador escolher entre Tentar de novo (já selecionado), Mapa e
    /// Menu inicial. A fase não reinicia sozinha.
    /// </summary>
    public class CaughtScreenUI : MonoBehaviour
    {
        [SerializeField] private LevelFlow _levelFlow;
        [SerializeField] private ShoppingList _shoppingList;
        [SerializeField] private CaughtTipsData _tips;

        [Tooltip("Painel da tela; começa escondido")]
        [SerializeField] private GameObject _panel;

        [SerializeField] private Text _summary;
        [SerializeField] private Text _tip;
        [SerializeField] private Button _tryAgainButton;

        private void Awake()
        {
            if (_levelFlow == null || _shoppingList == null || _tips == null || _panel == null || _summary == null ||
                _tip == null || _tryAgainButton == null)
            {
                Debug.LogError("CaughtScreenUI is missing references.", this);
                enabled = false;
                return;
            }

            _panel.SetActive(false);
        }

        private void OnEnable()
        {
            if (_levelFlow != null)
            {
                _levelFlow.OnCaughtPanelDue += HandleCaughtPanelDue;
            }
        }

        private void OnDisable()
        {
            if (_levelFlow != null)
            {
                _levelFlow.OnCaughtPanelDue -= HandleCaughtPanelDue;
            }
        }

        /// <summary>Ligado ao botão "Tentar de novo" no Inspector.</summary>
        public void TryAgain() => SceneLoader.ReloadActiveScene();

        /// <summary>Ligado ao botão "Mapa" no Inspector.</summary>
        public void GoToCityMap() => SceneLoader.LoadCityMap();

        /// <summary>Ligado ao botão "Menu inicial" no Inspector.</summary>
        public void GoToMainMenu() => SceneLoader.LoadMainMenu();

        private void HandleCaughtPanelDue()
        {
            ShoppingListProgress progress = _shoppingList.Progress;
            _summary.text = $"Tempo {TimeFormat.MinutesAndSeconds(_levelFlow.ElapsedSeconds)}" +
                $"     Itens entregues {progress.DeliveredCount}/{progress.RequiredCount}";
            _tip.text = PickTip();
            _panel.SetActive(true);
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(_tryAgainButton.gameObject); // Enter ou A recomeça
            }
        }

        private string PickTip()
        {
            IReadOnlyList<string> tips = _tips.Tips;
            return tips.Count == 0 ? string.Empty : $"Dica: {tips[Random.Range(0, tips.Count)]}";
        }
    }
}
