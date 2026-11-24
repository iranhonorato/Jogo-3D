using System;
using UnityEngine;

namespace Guaxinim.Core
{
    /// <summary>
    /// Pausa da fase (GDD §13): Esc (Start no controle) congela o tempo e passa o input para o mapa UI;
    /// Esc de novo, ou o botão Continuar, volta. Quando a fase acaba (vitória ou pego), o input do guaxinim
    /// também é travado, para nada acontecer atrás das telas.
    /// </summary>
    public class PauseController : MonoBehaviour
    {
        [SerializeField] private LevelFlow _levelFlow;

        [Tooltip("Input do guaxinim (ligado na montagem da fase)")]
        [SerializeField] private InputReader _input;

        public event Action OnPaused;
        public event Action OnResumed;

        public bool IsPaused { get; private set; }

        private void Awake()
        {
            if (_levelFlow == null || _input == null)
            {
                Debug.LogError("PauseController needs Level Flow and Input assigned.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (_levelFlow == null || _input == null)
            {
                return;
            }

            _input.OnPausePressed += Pause;
            _input.OnResumePressed += Resume;
            _levelFlow.OnLevelWon += LockPlayerInput;
            _levelFlow.OnPlayerCaught += LockPlayerInput;
        }

        private void OnDisable()
        {
            if (_levelFlow == null || _input == null)
            {
                return;
            }

            _input.OnPausePressed -= Pause;
            _input.OnResumePressed -= Resume;
            _levelFlow.OnLevelWon -= LockPlayerInput;
            _levelFlow.OnPlayerCaught -= LockPlayerInput;
        }

        public void Pause()
        {
            if (IsPaused || _levelFlow.IsLevelOver)
            {
                return;
            }

            IsPaused = true;
            Time.timeScale = 0f;
            _input.EnableUIInput();
            OnPaused?.Invoke();
        }

        /// <summary>Ligado ao botão Continuar no Inspector e ao Esc no mapa UI.</summary>
        public void Resume()
        {
            if (!IsPaused)
            {
                return;
            }

            IsPaused = false;
            Time.timeScale = 1f;
            _input.EnablePlayerInput();
            OnResumed?.Invoke();
        }

        /// <summary>Ligado ao botão Reiniciar fase no Inspector.</summary>
        public void RestartLevel() => SceneLoader.ReloadActiveScene();

        /// <summary>Ligado ao botão Mapa no Inspector.</summary>
        public void GoToCityMap() => SceneLoader.LoadCityMap();

        private void LockPlayerInput() => _input.EnableUIInput();
    }
}
