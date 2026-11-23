using System.Collections.Generic;
using Guaxinim.Shared;
using UnityEngine;

namespace Guaxinim.Guards
{
    /// <summary>
    /// Visão (GDD §6.1): a cada frame, procura um <see cref="IDetectable"/> visível dentro do cone e
    /// sem obstáculo (layer Obstacle) no caminho. O resultado fica em <see cref="VisibleTarget"/>.
    /// </summary>
    public class VisionCone : MonoBehaviour
    {
        [SerializeField] private VisionData _data;

        [Tooltip("Layers que bloqueiam a visão (Obstacle)")]
        [SerializeField] private LayerMask _obstacleMask;

        public VisionData Data => _data;
        public LayerMask ObstacleMask => _obstacleMask;

        /// <summary>O alvo visto neste frame, ou null.</summary>
        public IDetectable VisibleTarget { get; private set; }

        public bool HasVisibleTarget => VisibleTarget != null;

        /// <summary>Distância no chão até o alvo visto, em metros.</summary>
        public float DistanceToTarget { get; private set; }

        public Vector3 EyePosition => transform.position + Vector3.up * _data.EyeHeight;

        private void Awake()
        {
            if (_data == null)
            {
                Debug.LogError("VisionCone needs Vision Data assigned.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            VisibleTarget = null;
            IReadOnlyList<IDetectable> detectables = DetectableRegistry.All;
            for (int i = 0; i < detectables.Count; i++) // for em vez de foreach: sem alocação por frame
            {
                if (CanSee(detectables[i]))
                {
                    VisibleTarget = detectables[i];
                    DistanceToTarget = VisionMath.FlatDistance(transform.position, VisibleTarget.DetectionPoint);
                    return;
                }
            }
        }

        public bool CanSee(IDetectable target)
        {
            if (!target.IsVisible)
            {
                return false;
            }

            if (!VisionMath.IsInCone(transform.position, transform.forward, target.DetectionPoint,
                    _data.RangeMeters, _data.AngleDegrees))
            {
                return false;
            }

            return !Physics.Linecast(EyePosition, target.DetectionPoint, _obstacleMask, QueryTriggerInteraction.Ignore);
        }
    }
}
