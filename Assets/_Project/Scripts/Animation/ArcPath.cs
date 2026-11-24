using UnityEngine;

namespace Guaxinim.Animation
{
    /// <summary>Trajetória em arco (parábola) entre dois pontos, para o item voando até a bolsa do guaxinim.</summary>
    public static class ArcPath
    {
        /// <summary>Ponto no arco em <paramref name="progress"/> (0 a 1), subindo até <paramref name="heightMeters"/> no meio.</summary>
        public static Vector3 Evaluate(Vector3 start, Vector3 end, float heightMeters, float progress)
        {
            float t = Mathf.Clamp01(progress);
            return Vector3.Lerp(start, end, t) + Vector3.up * (4f * heightMeters * t * (1f - t));
        }
    }
}
