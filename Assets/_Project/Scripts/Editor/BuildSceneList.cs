using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Mantém o Build Profile na ordem do jogo (GDD §13): menu principal, mapa da cidade, as fases por nome e a tela
    /// de fim de jogo. A Sandbox de teste fica de fora do build. Só entra o que existe, então vale rodar depois de
    /// cada cena nova.
    /// </summary>
    public static class BuildSceneList
    {
        public const string MainMenuPath = "Assets/_Project/Scenes/MainMenu.unity";
        public const string CityMapPath = "Assets/_Project/Scenes/CityMap.unity";
        public const string EndingPath = "Assets/_Project/Scenes/Ending.unity";

        private const string LevelsFolder = "Assets/_Project/Scenes/Levels";

        public static void Refresh()
        {
            var paths = new List<string> { MainMenuPath, CityMapPath };
            if (Directory.Exists(LevelsFolder))
            {
                paths.AddRange(Directory.GetFiles(LevelsFolder, "*.unity").Select(NormalizePath).OrderBy(path => path));
            }

            paths.Add(EndingPath);
            EditorBuildSettings.scenes = paths
                .Where(File.Exists)
                .Select(path => new EditorBuildSettingsScene(path, true))
                .ToArray();
        }

        private static string NormalizePath(string path) => path.Replace('\\', '/');
    }
}
