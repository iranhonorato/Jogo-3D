using UnityEditor;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Regera tudo de uma vez: os prefabs, todas as fases descritas, o menu, o mapa e o fim de jogo (os menus Montar
    /// Todas as Fases e Montar Menu e Mapa, nessa ordem). Também roda por linha de comando, com o Editor fechado:
    /// Unity.exe -batchmode -quit -projectPath . -executeMethod Guaxinim.EditorTools.BatchBuild.RebuildAll
    /// </summary>
    public static class BatchBuild
    {
        [MenuItem("Tools/Guaxinim/Regerar Tudo")]
        public static void RebuildAll()
        {
            LevelSceneBuilder.BuildAllLevels();
            MenuScenesBuilder.BuildMenuScenes();
            AssetDatabase.SaveAssets();
        }
    }
}
