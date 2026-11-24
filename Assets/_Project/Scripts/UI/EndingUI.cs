using Guaxinim.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Guaxinim.UI
{
    /// <summary>
    /// Tela de fim de jogo (GDD §13), depois da última fase: total de estrelas e soma dos melhores tempos, com
    /// Recomeçar (volta à primeira fase mantendo o save), Menu inicial e Créditos.
    /// </summary>
    public class EndingUI : MonoBehaviour
    {
        [SerializeField] private LevelCatalog _catalog;
        [SerializeField] private Text _summary;
        [SerializeField] private Button _restartButton;

        private void Awake()
        {
            if (_catalog == null || _summary == null || _restartButton == null)
            {
                Debug.LogError("EndingUI is missing references.", this);
                enabled = false;
            }
        }

        private void Start()
        {
            SaveData save = SaveSystem.Data;
            int maxStars = _catalog.PlayableCount * LevelResult.MaxStars;
            _summary.text = $"Estrelas  {_catalog.SumBestStars(save)} de {maxStars}\n" +
                $"Tempo total  {TimeFormat.MinutesAndSeconds(_catalog.SumBestTimeSeconds(save))}";
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(_restartButton.gameObject);
            }
        }

        /// <summary>Ligado ao botão Recomeçar no Inspector. O progresso salvo continua.</summary>
        public void Restart() => SceneLoader.LoadLevel(_catalog.FirstPlayable);

        /// <summary>Ligado ao botão Menu inicial no Inspector.</summary>
        public void GoToMainMenu() => SceneLoader.LoadMainMenu();

        /// <summary>Ligado ao botão Créditos no Inspector.</summary>
        public void ShowCredits() => SceneLoader.LoadMainMenuCredits();
    }
}
