using UnityEngine;

namespace Guaxinim.EditorTools
{
    /// <summary>Inimigo com a ronda: começa no waypoint <see cref="StartWaypoint"/>, virado para <see cref="YawDegrees"/>.</summary>
    public readonly struct EnemyPlacement
    {
        public EnemyPlacement(EnemyType type, Vector3[] waypoints, int startWaypoint, float yawDegrees)
        {
            Type = type;
            Waypoints = waypoints;
            StartWaypoint = startWaypoint;
            YawDegrees = yawDegrees;
        }

        public EnemyType Type { get; }
        public Vector3[] Waypoints { get; }
        public int StartWaypoint { get; }
        public float YawDegrees { get; }
    }
}
