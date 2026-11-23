using UnityEngine;

namespace Guaxinim.Noise
{
    /// <summary>Um barulho no mundo: onde aconteceu e até onde se ouve (GDD §5).</summary>
    public readonly struct NoiseEvent
    {
        public NoiseEvent(Vector3 position, float radius)
        {
            Position = position;
            Radius = radius;
        }

        public Vector3 Position { get; }
        public float Radius { get; }
    }
}
