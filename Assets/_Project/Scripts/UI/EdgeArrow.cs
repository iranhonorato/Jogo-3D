using UnityEngine;

namespace Guaxinim.UI
{
    /// <summary>
    /// Conta da seta que aponta para algo fora da tela: decide se o ponto está visível e, se não, onde a seta fica
    /// na borda (na linha entre o centro da tela e o ponto). Tudo em pixels da tela.
    /// </summary>
    public static class EdgeArrow
    {
        private const float MinOffset = 0.0001f;

        /// <summary>O ponto está na frente da câmera e a pelo menos <paramref name="margin"/> pixels das bordas.</summary>
        public static bool IsOnScreen(Vector3 screenPoint, Vector2 screenSize, float margin) =>
            screenPoint.z > 0f &&
            screenPoint.x >= margin && screenPoint.x <= screenSize.x - margin &&
            screenPoint.y >= margin && screenPoint.y <= screenSize.y - margin;

        /// <summary>Onde a seta fica: na borda recuada <paramref name="margin"/> pixels, na direção do ponto.</summary>
        public static Vector2 ClampToEdge(Vector3 screenPoint, Vector2 screenSize, float margin)
        {
            Vector2 center = screenSize * 0.5f;
            Vector2 offset = new Vector2(screenPoint.x, screenPoint.y) - center;
            if (screenPoint.z < 0f)
            {
                offset = -offset; // atrás da câmera a projeção sai invertida
            }

            Vector2 halfArea = center - Vector2.one * margin;
            float scaleX = halfArea.x / Mathf.Max(Mathf.Abs(offset.x), MinOffset);
            float scaleY = halfArea.y / Mathf.Max(Mathf.Abs(offset.y), MinOffset);
            return center + offset * Mathf.Min(1f, Mathf.Min(scaleX, scaleY));
        }

        /// <summary>Ângulo em graus (0 = direita, 90 = cima) para girar a seta de <paramref name="from"/> a <paramref name="to"/>.</summary>
        public static float AngleDegrees(Vector2 from, Vector2 to) =>
            Mathf.Atan2(to.y - from.y, to.x - from.x) * Mathf.Rad2Deg;
    }
}
