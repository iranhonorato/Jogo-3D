using UnityEngine;

namespace Guaxinim.Guards
{
    /// <summary>Balanço da cabeça ao olhar em volta: começa de frente, vai a um lado, ao outro e volta.</summary>
    public static class LookAround
    {
        public static float GetYawOffset(float elapsedSeconds, float amplitudeDegrees, float periodSeconds)
        {
            if (periodSeconds <= 0f)
            {
                return 0f;
            }

            return amplitudeDegrees * Mathf.Sin(2f * Mathf.PI * elapsedSeconds / periodSeconds);
        }
    }
}
