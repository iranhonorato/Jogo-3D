using Guaxinim.Core;
using UnityEditor;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Cria as dicas do tutorial da Fase 1 (GDD §11) e as liga ao <see cref="LevelData"/> dela. Os textos e os gatilhos
    /// vêm do código (<see cref="TutorialTipsData"/>): rodar de novo regrava o asset, para uma mudança nos textos (como
    /// os botões do controle) chegar ao jogo. Ajuste as dicas no código, não no Inspector.
    /// </summary>
    public static class TutorialTipsFactory
    {
        private const string DataFolder = "Assets/_Project/Data/Levels";
        private const string Level01TipsPath = DataFolder + "/TutorialTips_Level01.asset";

        /// <summary>Liga as dicas da Fase 1 ao LevelData, criando ou regravando o asset com os textos do código.</summary>
        public static void AssignToLevel01(LevelData level)
        {
            var tips = EditorAssets.LoadOrCreateFromCode<TutorialTipsData>(Level01TipsPath);
            EditorAssets.SetReference(level, "_tips", tips);
            EditorUtility.SetDirty(level);
        }
    }
}
