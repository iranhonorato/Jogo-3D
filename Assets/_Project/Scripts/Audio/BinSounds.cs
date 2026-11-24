using Guaxinim.Hazards;
using UnityEngine;

namespace Guaxinim.Audio
{
    /// <summary>Barulho de lata da lixeira quando o guaxinim entra ou sai (GDD §9 e §15.6).</summary>
    [RequireComponent(typeof(AudioSource), typeof(HidingSpot))]
    public class BinSounds : MonoBehaviour
    {
        [SerializeField] private SoundData _sounds;

        private AudioSource _source;
        private HidingSpot _hidingSpot;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            _hidingSpot = GetComponent<HidingSpot>();
            if (_sounds == null)
            {
                Debug.LogError("BinSounds needs the Sound Data assigned.", this);
                enabled = false;
            }
        }

        private void OnEnable() => _hidingSpot.OnUsed += HandleUsed;

        private void OnDisable() => _hidingSpot.OnUsed -= HandleUsed;

        private void HandleUsed() => _sounds.BinUse.PlayOn(_source);
    }
}
