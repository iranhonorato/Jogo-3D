using Guaxinim.Core;
using UnityEditor;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Cria as dicas do tutorial da Fase 1 (GDD §11) e as liga ao <see cref="LevelData"/> dela. Os textos e os gatilhos
    /// ficam no asset, editáveis no Inspector; rodar de novo mantém o que foi ajustado.
    /// </summary>
    public static class TutorialTipsFactory
    {
        private const string DataFolder = "Assets/_Project/Data/Levels";
        private const string Level01TipsPath = DataFolder + "/TutorialTips_Level01.asset";

        /// <summary>Liga as dicas da Fase 1 ao LevelData, criando o asset se ainda não existe.</summary>
        public static void AssignToLevel01(LevelData level)
        {
            var tips = EditorAssets.LoadOrCreate<TutorialTipsData>(Level01TipsPath);
            EditorAssets.SetReference(level, "_tips", tips);
            EditorUtility.SetDirty(level);
        }
    }
}
