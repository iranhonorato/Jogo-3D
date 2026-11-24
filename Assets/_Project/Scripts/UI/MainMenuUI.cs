using Guaxinim.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Guaxinim.UI
{
    /// <summary>
    /// Menu principal (GDD §13): Jogar, Como jogar, Créditos e Sair. Só um painel fica aberto por vez, e o
    /// primeiro botão dele é selecionado para o teclado e o controle. "Sair" some no WebGL, onde não há o que fechar.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private GameObject _howToPlayPanel;
        [SerializeField] private GameObject _creditsPanel;

        [SerializeField] private Button _playButton;
        [SerializeField] private Button _howToPlayBackButton;
        [SerializeField] private Button _creditsBackButton;

        [Tooltip("Escondido no WebGL")]
        [SerializeField] private GameObject _quitButton;

        private void Awake()
        {
            if (_mainPanel == null || _howToPlayPanel == null || _creditsPanel == null || _playButton == null ||
                _howToPlayBackButton == null || _creditsBackButton == null || _quitButton == null)
            {
                Debug.LogError("MainMenuUI is missing references.", this);
                enabled = false;
                return;
            }

            _quitButton.SetActive(Application.platform != RuntimePlatform.WebGLPlayer);
        }

        private void Start() => ShowMain();

        /// <summary>Ligado ao botão Jogar no Inspector.</summary>
        public void Play() => SceneLoader.LoadCityMap();

        public void ShowMain() => Show(_mainPanel, _playButton);

        public void ShowHowToPlay() => Show(_howToPlayPanel, _howToPlayBackButton);

        public void ShowCredits() => Show(_creditsPanel, _creditsBackButton);

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void Show(GameObject panel, Button firstSelected)
        {
            _mainPanel.SetActive(panel == _mainPanel);
            _howToPlayPanel.SetActive(panel == _howToPlayPanel);
            _creditsPanel.SetActive(panel == _creditsPanel);
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(firstSelected.gameObject);
            }
        }
    }
}
