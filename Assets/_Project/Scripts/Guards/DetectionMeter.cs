using System;
using Guaxinim.Shared;
using UnityEngine;

namespace Guaxinim.Guards
{
    /// <summary>
    /// Medidor de detecção (GDD §6.3): enche enquanto o <see cref="VisionCone"/> vê o guaxinim e
    /// avisa em <see cref="OnFilled"/>. O mesmo componente serve ao segurança (entra em Perseguição)
    /// e à câmera de segurança (dispara o alerta); muda só quem escuta.
    /// </summary>
    public class DetectionMeter : MonoBehaviour, IMeterValue
    {
        [SerializeField] private VisionCone _visionCone;
        [SerializeField] private DetectionData _data;

        private DetectionLevel _level;

        public event Action OnFilled;

        /// <summary>De 0 a 1.</summary>
        public float Value => _level != null ? _level.Value : 0f;
        public bool IsFull => _level != null && _level.IsFull;

        private void Awake()
        {
            if (_visionCone == null || _data == null)
            {
                Debug.LogError("DetectionMeter needs Vision Cone and Detection Data assigned.", this);
                enabled = false;
                return;
            }

            _level = new DetectionLevel(_data.FillSecondsAtMaxRange, _data.CloseRangeMultiplier, _data.DrainPerSecond);
        }

        private void Update()
        {
            bool becameFull = _level.Tick(_visionCone.HasVisibleTarget, _visionCone.DistanceToTarget,
                _visionCone.Data.RangeMeters, Time.deltaTime);
            if (becameFull)
            {
                OnFilled?.Invoke();
            }
        }

        /// <summary>Zera o medidor (ao voltar à patrulha, por exemplo).</summary>
        public void ResetLevel() => _level?.Reset();
    }
}
