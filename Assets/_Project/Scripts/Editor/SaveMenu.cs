using Guaxinim.Core;
using UnityEditor;

namespace Guaxinim.EditorTools
{
    /// <summary>Atalhos do Editor para testar o desbloqueio das fases: do zero, ou com todas liberadas.</summary>
    public static class SaveMenu
    {
        private const int TestStars = 1;

        [MenuItem("Tools/Guaxinim/Apagar Save")]
        private static void ClearSave() => SaveSystem.Clear();

        /// <summary>
        /// Marca todas as fases menos a última como completas (1 estrela, no tempo-alvo), para jogar qualquer uma pelo
        /// mapa. Só para teste: use Apagar Save para voltar ao começo.
        /// </summary>
        [MenuItem("Tools/Guaxinim/Liberar Todas as Fases (teste)")]
        private static void UnlockAllLevels()
        {
            LevelCatalog catalog = LevelCatalogFactory.LoadOrCreateCatalog();
            for (int i = 0; i < catalog.Levels.Count - 1; i++)
            {
                LevelData level = catalog.Levels[i];
                SaveSystem.RecordLevelResult(level.Id, TestStars, level.TargetTimeSeconds);
            }
        }
    }
}
