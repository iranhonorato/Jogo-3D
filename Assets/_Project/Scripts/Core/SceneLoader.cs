using UnityEngine;
using UnityEngine.SceneManagement;

namespace Guaxinim.Core
{
    /// <summary>
    /// Troca de cenas (GDD §13): menu principal, mapa da cidade, uma fase e o reinício da fase atual. Toda
    /// troca devolve o tempo ao normal, porque a pausa e a tela de resultado o congelam.
    /// </summary>
    public static class SceneLoader
    {
        public const string MainMenuSceneName = "MainMenu";
        public const string CityMapSceneName = "CityMap";

        /// <summary>Recarrega a cena ativa: todo o estado da fase volta ao começo.</summary>
        public static void ReloadActiveScene()
        {
            Scene active = SceneManager.GetActiveScene();
            if (active.buildIndex < 0)
            {
                Debug.LogError($"Scene '{active.name}' is not in the Build Profile scene list; it cannot be reloaded.");
                return;
            }

            Time.timeScale = 1f; // a tela de resultado (e a pausa) congela o tempo
            SceneManager.LoadScene(active.buildIndex);
        }

        public static void LoadMainMenu() => Load(MainMenuSceneName);

        public static void LoadCityMap() => Load(CityMapSceneName);

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

            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
        }
    }
}
