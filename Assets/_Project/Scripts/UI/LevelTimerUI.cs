using Guaxinim.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Guaxinim.UI
{
    /// <summary>Cronômetro da fase na HUD (GDD §13), em minutos e segundos; o texto muda uma vez por segundo.</summary>
    public class LevelTimerUI : MonoBehaviour
    {
        [SerializeField] private LevelFlow _levelFlow;
        [SerializeField] private Text _text;

        private int _lastShownSeconds = -1;

        private void Awake()
        {
            if (_levelFlow == null || _text == null)
            {
                Debug.LogError("LevelTimerUI is missing references.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            int seconds = Mathf.FloorToInt(_levelFlow.ElapsedSeconds);
            if (seconds == _lastShownSeconds)
            {
                return;
            }

            _lastShownSeconds = seconds;
            _text.text = TimeFormat.MinutesAndSeconds(seconds);
        }
    }
}
