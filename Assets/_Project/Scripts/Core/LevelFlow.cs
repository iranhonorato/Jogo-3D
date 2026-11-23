using System;
using System.Collections;
using Guaxinim.Items;
using Guaxinim.Player;
using UnityEngine;

namespace Guaxinim.Core
{
    /// <summary>
    /// Ciclo da fase (GDD §6.5, §7.7 e §13): pego, avisa a UI e reinicia a fase depois de 1,5 s; lista
    /// completa na toca, vence, congela o jogo e avisa a tela de resultado. Só um fim por fase. Vive na cena
    /// (no LevelSystems), então o estado da fase morre com ela a cada reinício.
    /// </summary>
    public class LevelFlow : MonoBehaviour
    {
        [SerializeField] private LevelFlowData _data;
        [SerializeField] private PlayerState _player;
        [SerializeField] private ShoppingList _shoppingList;

        public event Action OnPlayerCaught;
        public event Action OnLevelWon;

        public bool IsLevelOver { get; private set; }

        /// <summary>Segundos de jogo desde o início da fase; para no fim (cronômetro e tempo-alvo).</summary>
        public float ElapsedSeconds { get; private set; }

        private void Awake()
        {
            if (_data == null || _player == null || _shoppingList == null)
            {
                Debug.LogError("LevelFlow needs Data, Player and Shopping List assigned.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (_player != null)
            {
                _player.OnCaught += HandlePlayerCaught;
            }

            if (_shoppingList != null)
            {
                _shoppingList.OnCompleted += HandleListCompleted;
            }
        }

        private void Update()
        {
            if (!IsLevelOver)
            {
                ElapsedSeconds += Time.deltaTime;
            }
        }

        private void OnDisable()
        {
            if (_player != null)
            {
                _player.OnCaught -= HandlePlayerCaught;
            }

            if (_shoppingList != null)
            {
                _shoppingList.OnCompleted -= HandleListCompleted;
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

        private void HandleListCompleted()
        {
            if (IsLevelOver)
            {
                return; // pego e entregando no mesmo frame: vale o que aconteceu primeiro
            }

            IsLevelOver = true;
            Time.timeScale = 0f; // o mundo para atrás da tela de resultado; o SceneLoader volta a 1
            OnLevelWon?.Invoke();
        }

        private IEnumerator RestartAfterDelay()
        {
            yield return new WaitForSeconds(_data.CaughtRestartDelaySeconds);
            SceneLoader.ReloadActiveScene();
        }
    }
}
