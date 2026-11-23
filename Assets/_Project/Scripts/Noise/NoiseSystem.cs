using System;
using System.Collections.Generic;
using UnityEngine;

namespace Guaxinim.Noise
{
    /// <summary>
    /// Mediador dos barulhos (GDD §5): quem faz barulho chama <see cref="Emit"/> sem saber quem ouve,
    /// e cada <see cref="INoiseListener"/> no raio é avisado. O anel no chão escuta <see cref="OnNoiseEmitted"/>.
    /// </summary>
    public static class NoiseSystem
    {
        private static readonly List<INoiseListener> Listeners = new List<INoiseListener>();

        public static event Action<NoiseEvent> OnNoiseEmitted;

        public static void Register(INoiseListener listener)
        {
            if (!Listeners.Contains(listener))
            {
                Listeners.Add(listener);
            }
        }

        public static void Unregister(INoiseListener listener) => Listeners.Remove(listener);

        /// <summary>Faz um barulho em <paramref name="position"/>, ouvido até <paramref name="radius"/> metros.</summary>
        public static void Emit(Vector3 position, float radius)
        {
            if (radius <= 0f)
            {
                return; // GDD §5: andar é silencioso
            }

            var noise = new NoiseEvent(position, radius);
            for (int i = Listeners.Count - 1; i >= 0; i--) // de trás para frente: um ouvinte pode sair ao ouvir
            {
                INoiseListener listener = Listeners[i];
                if (CanHear(listener.ListenerPosition, noise))
                {
                    listener.Hear(noise);
                }
            }

            OnNoiseEmitted?.Invoke(noise);
        }

        // TODO(passo 1.10): paredes externas bloqueiam o som; prateleiras não (GDD §5). Hoje todo barulho é dentro do mercado.
        public static bool CanHear(Vector3 listenerPosition, NoiseEvent noise) =>
            (listenerPosition - noise.Position).sqrMagnitude <= noise.Radius * noise.Radius;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Listeners.Clear();
            OnNoiseEmitted = null;
        }
    }
}
