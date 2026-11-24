using System;
using System.Collections.Generic;
using UnityEngine;

namespace Guaxinim.Core
{
    /// <summary>
    /// Progresso salvo (GDD §12): estrelas e melhor tempo de cada fase. A primeira fase está sempre
    /// liberada; as outras, quando a anterior foi completa. Vira JSON com o JsonUtility.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        [SerializeField] private List<LevelRecord> _levels = new List<LevelRecord>();

        public static SaveData FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return new SaveData();
            }

            try
            {
                return JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
            }
            catch (ArgumentException exception)
            {
                Debug.LogError($"Corrupted save, starting over: {exception.Message}");
                return new SaveData();
            }
        }

        public string ToJson() => JsonUtility.ToJson(this);

        /// <summary>
        /// Guarda o resultado da fase. Fase sem id (a Sandbox) não entra no save. Devolve true quando o
        /// tempo bateu o recorde anterior.
        /// </summary>
        public bool RecordResult(string levelId, int stars, float timeSeconds)
        {
            if (string.IsNullOrEmpty(levelId))
            {
                return false;
            }

            return GetOrCreate(levelId).Record(stars, timeSeconds);
        }

        /// <summary>Resultado salvo da fase, ou null se nunca foi completa.</summary>
        public LevelRecord Find(string levelId)
        {
            foreach (LevelRecord record in _levels)
            {
                if (record.LevelId == levelId)
                {
                    return record;
                }
            }

            return null;
        }

        public bool IsCompleted(string levelId)
        {
            LevelRecord record = Find(levelId);
            return record != null && record.IsCompleted;
        }

        /// <summary>A fase está liberada se é a primeira ou se a anterior foi completa.</summary>
        public bool IsUnlocked(string levelId, string previousLevelId) =>
            string.IsNullOrEmpty(previousLevelId) || IsCompleted(previousLevelId) || IsCompleted(levelId);

        private LevelRecord GetOrCreate(string levelId)
        {
            LevelRecord record = Find(levelId);
            if (record != null)
            {
                return record;
            }

            record = new LevelRecord(levelId);
            _levels.Add(record);
            return record;
        }
    }
}
