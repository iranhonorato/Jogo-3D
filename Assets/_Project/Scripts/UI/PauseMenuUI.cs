using Guaxinim.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Guaxinim.UI
{
    /// <summary>Tela de pausa (GDD §13): mostra o painel ao pausar e seleciona Continuar (teclado e controle).</summary>
    public class PauseMenuUI : MonoBehaviour
    {
        [SerializeField] private PauseController _pause;
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _resumeButton;

        private void Awake()
        {
            if (_pause == null || _panel == null || _resumeButton == null)
            {
                Debug.LogError("PauseMenuUI is missing references.", this);
                enabled = false;
                return;
            }

            _panel.SetActive(false);
        }

        private void OnEnable()
        {
            if (_pause != null)
            {
                _pause.OnPaused += HandlePaused;
                _pause.OnResumed += HandleResumed;
            }
        }

        private void OnDisable()
        {
            if (_pause != null)
            {
                _pause.OnPaused -= HandlePaused;
                _pause.OnResumed -= HandleResumed;
            }
        }

        private void HandlePaused()
        {
            _panel.SetActive(true);
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(_resumeButton.gameObject);
            }
        }

        private void HandleResumed() => _panel.SetActive(false);
    }
}
