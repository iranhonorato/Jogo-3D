using UnityEditor;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Regera tudo por linha de comando, com o Editor fechado: os prefabs, a Fase 1, o menu, o mapa e o fim de jogo
    /// (os mesmos menus Montar Fase 1 e Montar Menu e Mapa, nessa ordem).
    /// Unity.exe -batchmode -quit -projectPath . -executeMethod Guaxinim.EditorTools.BatchBuild.RebuildAll
    /// </summary>
    public static class BatchBuild
    {
        [MenuItem("Tools/Guaxinim/Regerar Tudo")]
        public static void RebuildAll()
        {
            Level01Builder.BuildLevel();
            MenuScenesBuilder.BuildMenuScenes();
            AssetDatabase.SaveAssets();
        }
    }
}
