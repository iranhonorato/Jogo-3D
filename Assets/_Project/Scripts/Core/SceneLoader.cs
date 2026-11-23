using UnityEngine;
using UnityEngine.SceneManagement;

namespace Guaxinim.Core
{
    /// <summary>Troca de cenas (GDD §13). Por enquanto só reinicia a fase atual.</summary>
    public static class SceneLoader
    {
        /// <summary>Recarrega a cena ativa: todo o estado da fase volta ao começo.</summary>
        public static void ReloadActiveScene()
        {
            Scene active = SceneManager.GetActiveScene();
            if (active.buildIndex < 0)
            {
                Debug.LogError($"Scene '{active.name}' is not in the Build Profile scene list; it cannot be reloaded.");
                return;
            }

            SceneManager.LoadScene(active.buildIndex);
        }
    }
}
