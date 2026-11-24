using System;
using System.Collections;
using Guaxinim.Items;
using Guaxinim.Player;
using Guaxinim.Shared;
using UnityEngine;

namespace Guaxinim.Core
{
    /// <summary>
    /// Ciclo da fase (GDD §6.5, §7.7 e §13): ao ser pego, congela o jogo e, depois de uma pausa curta, avisa a UI
    /// para mostrar a tela "Você foi pego!" (quem decide o que fazer é o jogador); lista completa na toca, vence,
    /// congela o jogo e avisa a tela de resultado. Só um fim por fase. Vive na cena (no LevelSystems), então o
    /// estado da fase morre com ela a cada reinício.
    /// </summary>
    public class LevelFlow : MonoBehaviour
    {
        [SerializeField] private LevelFlowData _data;
        [SerializeField] private LevelData _level;
        [SerializeField] private PlayerState _player;
        [SerializeField] private ShoppingList _shoppingList;

        /// <summary>Na hora em que o guaxinim é pego: o jogo já está congelado.</summary>
        public event Action OnPlayerCaught;

        /// <summary>Alguns instantes depois de ser pego: é a hora de mostrar a tela com as opções.</summary>
        public event Action OnCaughtPanelDue;

        public event Action OnLevelWon;

        public bool IsLevelOver { get; private set; }

        /// <summary>Segundos de jogo desde o início da fase; para no fim (cronômetro e tempo-alvo).</summary>
        public float ElapsedSeconds { get; private set; }

        /// <summary>Preenchido ao vencer (estrelas, tempo e bônus).</summary>
        public LevelResult Result { get; private set; }

        /// <summary>O tempo desta vitória bateu o melhor tempo salvo antes (a primeira vitória não conta).</summary>
        public bool IsNewRecord { get; private set; }

        public LevelData Level => _level;

        /// <summary>Algum inimigo perseguiu o guaxinim nesta tentativa (perde a estrela "sem ser perseguido").</summary>
        public bool WasChased { get; private set; }

        private void Awake()
        {
            if (_data == null || _level == null || _player == null || _shoppingList == null)
            {
                Debug.LogError("LevelFlow needs Data, Level, Player and Shopping List assigned.", this);
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

            ChaseReports.OnChaseStarted += HandleChaseStarted;
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

            ChaseReports.OnChaseStarted -= HandleChaseStarted;
        }

        private void HandleChaseStarted() => WasChased = true;

        private void HandlePlayerCaught()
        {
            if (IsLevelOver)
            {
                return;
            }

            IsLevelOver = true;
            Time.timeScale = 0f; // o mundo para; o SceneLoader volta a 1 ao sair da fase
            OnPlayerCaught?.Invoke();
            StartCoroutine(RaiseCaughtPanelAfterDelay());
        }

        private void HandleListCompleted()
        {
            if (IsLevelOver)
            {
                return; // pego e entregando no mesmo frame: vale o que aconteceu primeiro
            }

            IsLevelOver = true;
            Result = LevelResult.Create(ElapsedSeconds, _level.TargetTimeSeconds, _shoppingList.Progress.IsComplete,
                WasChased, _shoppingList.Progress.BonusCount, _data.PointsPerBonusItem);
            IsNewRecord = SaveSystem.RecordLevelResult(_level.Id, Result.Stars, Result.ElapsedSeconds); // libera a próxima fase
            Time.timeScale = 0f; // o mundo para atrás da tela de resultado; o SceneLoader volta a 1
            OnLevelWon?.Invoke();
        }

        private IEnumerator RaiseCaughtPanelAfterDelay()
        {
            yield return new WaitForSecondsRealtime(_data.CaughtPanelDelaySeconds); // o tempo do jogo está parado
            OnCaughtPanelDue?.Invoke();
        }
    }
}
