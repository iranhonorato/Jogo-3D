using UnityEngine;

namespace Guaxinim.Guards
{
    /// <summary>Geometria do cone de visão no plano do chão (GDD §6.1), sem física.</summary>
    public static class VisionMath
    {
        private const float SamePointSqrDistance = 0.0001f;

        /// <summary>O alvo está dentro do alcance e da abertura do cone? A altura é ignorada.</summary>
        public static bool IsInCone(Vector3 origin, Vector3 forward, Vector3 target, float rangeMeters,
            float angleDegrees)
        {
            Vector3 toTarget = Flatten(target - origin);
            float sqrDistance = toTarget.sqrMagnitude;
            if (sqrDistance > rangeMeters * rangeMeters)
            {
                return false;
            }

            if (sqrDistance < SamePointSqrDistance)
            {
                return true;
            }

            return Vector3.Angle(Flatten(forward), toTarget) <= angleDegrees / 2f;
        }

        public static float FlatDistance(Vector3 from, Vector3 to) => Flatten(to - from).magnitude;

        private static Vector3 Flatten(Vector3 vector) => new Vector3(vector.x, 0f, vector.z);
    }
}
