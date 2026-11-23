using UnityEngine;

namespace Guaxinim.Noise
{
    /// <summary>Quem ouve barulhos: o NoiseSystem avisa quando um barulho alcança <see cref="ListenerPosition"/>.</summary>
    public interface INoiseListener
    {
        Vector3 ListenerPosition { get; }

        void Hear(NoiseEvent noise);
    }
}
