using UnityEngine;

namespace Guaxinim.Audio
{
    /// <summary>Os efeitos sonoros do jogo (GDD §15.6), com volume e variação de tom ajustáveis.</summary>
    [CreateAssetMenu(fileName = "SoundData", menuName = "Guaxinim/Sound Data")]
    public class SoundData : ScriptableObject
    {
        [Header("Guaxinim")]
        [SerializeField] private SoundCue _runStep;
        [SerializeField] private SoundCue _whistle;
        [SerializeField] private SoundCue _pickup;
        [SerializeField] private SoundCue _drop;
        [SerializeField] private SoundCue _glassBreak;
        [SerializeField] private SoundCue _binUse;

        [Header("Fase")]
        [SerializeField] private SoundCue _deliver;
        [SerializeField] private SoundCue _spotted;
        [SerializeField] private SoundCue _caught;
        [SerializeField] private SoundCue _win;

        public SoundCue RunStep => _runStep;
        public SoundCue Whistle => _whistle;
        public SoundCue Pickup => _pickup;
        public SoundCue Drop => _drop;
        public SoundCue GlassBreak => _glassBreak;
        public SoundCue BinUse => _binUse;
        public SoundCue Deliver => _deliver;
        public SoundCue Spotted => _spotted;
        public SoundCue Caught => _caught;
        public SoundCue Win => _win;
    }
}
