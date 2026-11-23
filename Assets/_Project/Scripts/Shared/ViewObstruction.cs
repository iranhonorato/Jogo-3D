using UnityEngine;

namespace Guaxinim.Shared
{
    /// <summary>
    /// Visão do jogador tapada pela carga volumosa (GDD §7.4): enquanto ativa, o que estiver a mais de
    /// <c>clearRadius</c> metros do guaxinim não é desenhado (os cones dos seguranças consultam aqui).
    /// Quem carrega liga e desliga; os desenhos só perguntam, sem conhecer o guaxinim.
    /// </summary>
    public static class ViewObstruction
    {
        private static Transform _viewer;
        private static float _clearRadius;

        public static bool IsActive => _viewer != null;

        public static void Begin(Transform viewer, float clearRadius)
        {
            _viewer = viewer;
            _clearRadius = clearRadius;
        }

        public static void End(Transform viewer)
        {
            if (_viewer == viewer)
            {
                _viewer = null;
            }
        }

        /// <summary>Verdadeiro se <paramref name="position"/> está longe demais para o jogador ver agora.</summary>
        public static bool IsHidden(Vector3 position)
        {
            if (_viewer == null)
            {
                return false;
            }

            Vector3 offset = position - _viewer.position;
            offset.y = 0f;
            return offset.sqrMagnitude > _clearRadius * _clearRadius;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => _viewer = null;
    }
}
