using System;
using UnityEngine;

namespace Guaxinim.Audio
{
    /// <summary>Um som do jogo: o clipe, o volume e quanto o tom pode variar a cada vez (para não cansar).</summary>
    [Serializable]
    public struct SoundCue
    {
        [SerializeField] private AudioClip _clip;
        [SerializeField, Range(0f, 1f)] private float _volume;

        [Tooltip("Variação aleatória do tom, para mais ou para menos (0 = sempre igual)")]
        [SerializeField, Range(0f, 0.5f)] private float _pitchJitter;

        public SoundCue(AudioClip clip, float volume, float pitchJitter)
        {
            _clip = clip;
            _volume = volume;
            _pitchJitter = pitchJitter;
        }

        /// <summary>Toca o som em <paramref name="source"/>, sem cortar o que já está tocando nela.</summary>
        public void PlayOn(AudioSource source)
        {
            if (_clip == null)
            {
                return;
            }

            source.pitch = 1f + UnityEngine.Random.Range(-_pitchJitter, _pitchJitter);
            source.PlayOneShot(_clip, _volume);
        }
    }
}
