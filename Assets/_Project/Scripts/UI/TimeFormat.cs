using UnityEngine;

namespace Guaxinim.UI
{
    /// <summary>Tempo em minutos e segundos ("01:30"), igual no cronômetro, no resultado e no mapa.</summary>
    public static class TimeFormat
    {
        private const int SecondsPerMinute = 60;

        public static string MinutesAndSeconds(float seconds)
        {
            int wholeSeconds = Mathf.FloorToInt(Mathf.Max(0f, seconds));
            return $"{wholeSeconds / SecondsPerMinute:00}:{wholeSeconds % SecondsPerMinute:00}";
        }
    }
}
