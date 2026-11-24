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
            if (string.IsNullOrEmpty(levelId))
            {
                return; // fase sem id (a Sandbox) não tem progresso
            }

            Data.RecordResult(levelId, stars, timeSeconds);
            Persist();
        }

        /// <summary>Apaga todo o progresso (menu do Editor, para testar o desbloqueio das fases do começo).</summary>
        public static void Clear()
        {
            _data = new SaveData();
            Persist();
        }

        private static void Persist()
        {
            PlayerPrefs.SetString(SaveKey, Data.ToJson());
            PlayerPrefs.Save(); // no WebGL, grava no IndexedDB do navegador
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => _data = null;
    }
}
