using System.Collections.Generic;
using UnityEngine;

namespace Guaxinim.Core
{
    /// <summary>
    /// As fases do jogo em ordem (GDD §11 e §12): o mapa da cidade lista este catálogo, e a tela de resultado
    /// acha nele a próxima fase. Uma fase sem cena ainda aparece como "Em breve". Somente leitura durante o jogo.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelCatalog", menuName = "Guaxinim/Level Catalog")]
    public class LevelCatalog : ScriptableObject
    {
        [Tooltip("Fases na ordem do mapa. Cada uma só libera depois que a anterior for completa")]
        [SerializeField] private LevelData[] _levels = new LevelData[0];

        public IReadOnlyList<LevelData> Levels => _levels;

        /// <summary>Fases que já têm cena; as outras aparecem como "Em breve".</summary>
        public int PlayableCount
        {
            get
            {
                int count = 0;
                foreach (LevelData level in _levels)
                {
                    count += level != null && level.IsPlayable ? 1 : 0;
                }

                return count;
            }
        }

        /// <summary>A primeira fase jogável (para recomeçar a campanha), ou null se não há nenhuma.</summary>
        public LevelData FirstPlayable
        {
            get
            {
                foreach (LevelData level in _levels)
                {
                    if (level != null && level.IsPlayable)
                    {
                        return level;
                    }
                }

                return null;
            }
        }

        /// <summary>Última fase jogável do catálogo: vencê-la termina a campanha (as "Em breve" não contam).</summary>
        public bool IsLastPlayable(LevelData level)
        {
            int index = IndexOf(level);
            if (index < 0 || !level.IsPlayable)
            {
                return false;
            }

            for (int i = index + 1; i < _levels.Length; i++)
            {
                if (_levels[i] != null && _levels[i].IsPlayable)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Soma das melhores estrelas das fases jogáveis.</summary>
        public int SumBestStars(SaveData save)
        {
            int total = 0;
            foreach (LevelData level in _levels)
            {
                LevelRecord record = level != null && level.IsPlayable ? save.Find(level.Id) : null;
                total += record != null ? record.BestStars : 0;
            }

            return total;
        }

        /// <summary>Soma dos melhores tempos das fases jogáveis já completas.</summary>
        public float SumBestTimeSeconds(SaveData save)
        {
            float total = 0f;
            foreach (LevelData level in _levels)
            {
                LevelRecord record = level != null && level.IsPlayable ? save.Find(level.Id) : null;
                total += record != null && record.IsCompleted ? record.BestTimeSeconds : 0f;
            }

            return total;
        }

        /// <summary>Posição da fase no catálogo, ou -1 se ela não está (a Sandbox, por exemplo).</summary>
        public int IndexOf(LevelData level)
        {
            if (level == null)
            {
                return -1;
            }

            for (int i = 0; i < _levels.Length; i++)
            {
                if (_levels[i] == level)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>Jogável: tem cena e a anterior foi completa (a primeira fase está sempre liberada).</summary>
        public bool IsUnlocked(int index, SaveData save)
        {
            if (index < 0 || index >= _levels.Length || _levels[index] == null || !_levels[index].IsPlayable)
            {
                return false;
            }

            bool hasMissingPrevious = index > 0 && _levels[index - 1] == null; // catálogo mal montado: não libera
            if (hasMissingPrevious)
            {
                return false;
            }

            return save.IsUnlocked(_levels[index].Id, index == 0 ? null : _levels[index - 1].Id);
        }

        /// <summary>A fase seguinte à atual, se ela existir, tiver cena e já estiver liberada.</summary>
        public bool TryGetNext(LevelData current, SaveData save, out LevelData next)
        {
            int nextIndex = IndexOf(current) + 1;
            bool isInCatalog = nextIndex > 0;
            if (isInCatalog && IsUnlocked(nextIndex, save))
            {
                next = _levels[nextIndex];
                return true;
            }

            next = null;
            return false;
        }
    }
}
