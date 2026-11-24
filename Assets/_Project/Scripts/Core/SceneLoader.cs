using UnityEngine;
using UnityEngine.SceneManagement;

namespace Guaxinim.Core
{
    /// <summary>
    /// Troca de cenas (GDD §13): menu principal, mapa da cidade, uma fase, a tela de fim de jogo e o reinício da
    /// fase atual. Toda troca escurece a tela (<see cref="ScreenFader"/>), devolve o tempo ao normal quando a cena
    /// muda (a pausa e as telas de resultado o congelam) e ignora pedidos enquanto outra troca está em andamento.
    /// </summary>
    public static class SceneLoader
    {
        public const string MainMenuSceneName = "MainMenu";
        public const string CityMapSceneName = "CityMap";
        public const string EndingSceneName = "Ending";

        /// <summary>Recarrega a cena ativa: todo o estado da fase volta ao começo.</summary>
        public static void ReloadActiveScene()
        {
            Scene active = SceneManager.GetActiveScene();
            int buildIndex = active.buildIndex;
            if (buildIndex < 0)
            {
                Debug.LogError($"Scene '{active.name}' is not in the Build Profile scene list; it cannot be reloaded.");
                return;
            }

            if (ScreenFader.IsBusy)
            {
                return;
            }

            ScreenFader.FadeOutThen(() =>
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(buildIndex);
            });
        }

        public static void LoadMainMenu() => Load(MainMenuSceneName);

        /// <summary>Vai ao menu principal, que abre direto nos Créditos.</summary>
        public static void LoadMainMenuCredits()
        {
            MenuRequest.RequestCredits();
            Load(MainMenuSceneName);
        }

        public static void LoadCityMap() => Load(CityMapSceneName);

        public static void LoadEnding() => Load(EndingSceneName);

        public static void LoadLevel(LevelData level)
        {
            if (level == null || !level.IsPlayable)
            {
                Debug.LogError("Cannot load a level that is missing or has no scene yet.");
                return;
            }

            Load(level.SceneName);
        }

        private static void Load(string sceneName)
        {
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError($"Scene '{sceneName}' is not in the Build Profile scene list; it cannot be loaded.");
                return;
            }

            if (ScreenFader.IsBusy)
            {
                return;
            }

            ScreenFader.FadeOutThen(() =>
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(sceneName);
            });
        }
    }
}
