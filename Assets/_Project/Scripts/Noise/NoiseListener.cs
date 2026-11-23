using System;
using UnityEngine;

namespace Guaxinim.Noise
{
    /// <summary>Ouvido de quem reage a barulhos (o segurança). Repassa o que ouviu em <see cref="OnNoiseHeard"/>.</summary>
    public class NoiseListener : MonoBehaviour, INoiseListener
    {
        public event Action<NoiseEvent> OnNoiseHeard;

        public Vector3 ListenerPosition => transform.position;

        private void OnEnable() => NoiseSystem.Register(this);

        private void OnDisable() => NoiseSystem.Unregister(this);

        public void Hear(NoiseEvent noise) => OnNoiseHeard?.Invoke(noise);
    }
}
