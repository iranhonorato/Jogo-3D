using UnityEngine;

namespace Guaxinim.EditorTools
{
    /// <summary>Posição no chão e giro em Y de uma peça da fase.</summary>
    public readonly struct Placement
    {
        public Placement(float x, float z, float yawDegrees = 0f, float y = 0f)
        {
            Position = new Vector3(x, y, z);
            YawDegrees = yawDegrees;
        }

        public Vector3 Position { get; }
        public float YawDegrees { get; }
    }
}
