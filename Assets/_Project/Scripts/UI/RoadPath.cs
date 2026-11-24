using UnityEngine;

namespace Guaxinim.UI
{
    /// <summary>
    /// Posição do marcador do guaxinim na estrada do mapa, pelo progresso da viagem (0 a 1): anda em linha reta de
    /// uma parada à outra, em pulinhos, e acende os pontos da estrada por onde já passou.
    /// </summary>
    public static class RoadPath
    {
        /// <summary>Ponto da estrada em <paramref name="progress"/>, com a altura do pulinho somada em Y.</summary>
        public static Vector2 Evaluate(Vector2 start, Vector2 end, float progress, float hopHeight, int hopCount)
        {
            float clamped = Mathf.Clamp01(progress);
            float hop = Mathf.Abs(Mathf.Sin(clamped * hopCount * Mathf.PI)) * hopHeight;
            return Vector2.Lerp(start, end, clamped) + Vector2.up * hop;
        }

        /// <summary>Quantos dos <paramref name="dotCount"/> pontos da estrada o marcador já passou.</summary>
        public static int LitDotCount(int dotCount, float progress)
        {
            // Os pontos ficam igualmente espaçados entre as paradas, sem ficar em cima delas.
            float passed = Mathf.Clamp01(progress) * (dotCount + 1);
            return Mathf.Clamp(Mathf.FloorToInt(passed), 0, dotCount);
        }
    }
}
