using System;
using UnityEngine;

namespace Guaxinim.Core
{
    /// <summary>Melhor resultado salvo de uma fase (GDD §12). Serializável pelo JsonUtility.</summary>
    [Serializable]
    public class LevelRecord
    {
        [SerializeField] private string _levelId;
        [SerializeField] private int _bestStars;
        [SerializeField] private float _bestTimeSeconds;

        public LevelRecord(string levelId) => _levelId = levelId;

        private LevelRecord()
        {
            // O JsonUtility cria a instância por aqui ao carregar o save.
        }

        public string LevelId => _levelId;
        public bool IsCompleted => _bestStars > 0;
        public int BestStars => _bestStars;
        public float BestTimeSeconds => _bestTimeSeconds;

        /// <summary>
        /// Guarda as melhores estrelas e o melhor tempo, cada um separadamente. Devolve true quando o tempo
        /// melhorou o recorde anterior (a primeira vez que a fase é completa não conta como recorde).
        /// </summary>
        public bool Record(int stars, float timeSeconds)
        {
            bool isFirstCompletion = !IsCompleted;
            bool isNewBestTime = !isFirstCompletion && timeSeconds < _bestTimeSeconds;
            _bestStars = Mathf.Max(_bestStars, stars);
            _bestTimeSeconds = isFirstCompletion ? timeSeconds : Mathf.Min(_bestTimeSeconds, timeSeconds);
            return isNewBestTime;
        }
    }
}
