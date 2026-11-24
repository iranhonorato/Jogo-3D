using Guaxinim.Animation;
using UnityEngine;

namespace Guaxinim.Guards
{
    /// <summary>"Ei!": o inimigo dá um pulinho de susto quando o medidor de detecção enche (GDD §6.3).</summary>
    public class DetectionStartle : MonoBehaviour
    {
        [SerializeField] private DetectionMeter _meter;
        [SerializeField] private ProceduralMotion _motion;

        private void Awake()
        {
            if (_meter == null || _motion == null)
            {
                Debug.LogError("DetectionStartle needs Meter and Motion assigned.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (_meter != null)
            {
                _meter.OnFilled += HandleFilled;
            }
        }

        private void OnDisable()
        {
            if (_meter != null)
            {
                _meter.OnFilled -= HandleFilled;
            }
        }

        private void HandleFilled() => _motion.Hop();
    }
}
