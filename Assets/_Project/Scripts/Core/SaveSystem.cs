using UnityEngine;

namespace Guaxinim.Core
{
    /// <summary>
    /// Salva o progresso num JSON numa chave do PlayerPrefs (GDD §12), que funciona no WebGL. Classe
    /// estática: não precisa de Inspector nem de Update. Carrega uma vez e mantém em memória.
    /// </summary>
    public static class SaveSystem
    {
        private const string SaveKey = "guaxinim_save_v1";

        private static SaveData _data;

        public static SaveData Data => _data ??= SaveData.FromJson(PlayerPrefs.GetString(SaveKey, string.Empty));

        public static void RecordLevelResult(string levelId, int stars, float timeSeconds)
        {
            Data.RecordResult(levelId, stars, timeSeconds);
            PlayerPrefs.SetString(SaveKey, Data.ToJson());
            PlayerPrefs.Save(); // no WebGL, grava no IndexedDB do navegador
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => _data = null;
    }
}
