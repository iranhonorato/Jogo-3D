using System;
using System.Collections;
using Guaxinim.Player;
using UnityEngine;

namespace Guaxinim.Core
{
    /// <summary>
    /// Ciclo da fase (GDD §6.5 e §13): quando o guaxinim é pego, avisa a UI e reinicia a fase depois de
    /// 1,5 s. Vive na cena (no LevelSystems), então o estado da fase morre com ela a cada reinício.
    /// </summary>
    public class LevelFlow : MonoBehaviour
    {
        [SerializeField] private LevelFlowData _data;
        [SerializeField] private PlayerState _player;

        public event Action OnPlayerCaught;

        public bool IsLevelOver { get; private set; }

        private void Awake()
        {
            if (_data == null || _player == null)
            {
                Debug.LogError("LevelFlow needs Data and Player assigned.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (_player != null)
            {
                _player.OnCaught += HandlePlayerCaught;
            }
        }

        private void OnDisable()
        {
            if (_player != null)
            {
                _player.OnCaught -= HandlePlayerCaught;
            }
        }

        private void HandlePlayerCaught()
        {
            if (IsLevelOver)
            {
                return;
            }

            IsLevelOver = true;
            OnPlayerCaught?.Invoke();
            StartCoroutine(RestartAfterDelay());
        }

        private IEnumerator RestartAfterDelay()
        {
            yield return new WaitForSeconds(_data.CaughtRestartDelaySeconds);
            SceneLoader.ReloadActiveScene();
        }
    }
}
