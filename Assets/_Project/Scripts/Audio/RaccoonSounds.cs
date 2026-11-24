using Guaxinim.Items;
using Guaxinim.Player;
using UnityEngine;

namespace Guaxinim.Audio
{
    /// <summary>
    /// Sons do guaxinim (GDD §15.6): pisada ao correr, assobio, pegar, largar e vidro quebrando. Só escuta os
    /// eventos dos componentes do próprio guaxinim.
    /// </summary>
    [RequireComponent(typeof(AudioSource), typeof(Inventory), typeof(RaccoonNoiseMaker))]
    [RequireComponent(typeof(RaccoonDropper), typeof(FragileShake))]
    public class RaccoonSounds : MonoBehaviour
    {
        [SerializeField] private SoundData _sounds;

        private AudioSource _source;
        private Inventory _inventory;
        private RaccoonNoiseMaker _noiseMaker;
        private RaccoonDropper _dropper;
        private FragileShake _fragileShake;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            _inventory = GetComponent<Inventory>();
            _noiseMaker = GetComponent<RaccoonNoiseMaker>();
            _dropper = GetComponent<RaccoonDropper>();
            _fragileShake = GetComponent<FragileShake>();
            if (_sounds == null)
            {
                Debug.LogError("RaccoonSounds needs the Sound Data assigned.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            _noiseMaker.OnRunStep += HandleRunStep;
            _noiseMaker.OnWhistled += HandleWhistled;
            _inventory.OnItemPickedUp += HandleItemPickedUp;
            _dropper.OnDropped += HandleDropped;
            _fragileShake.OnItemBroken += HandleItemBroken;
        }

        private void OnDisable()
        {
            _noiseMaker.OnRunStep -= HandleRunStep;
            _noiseMaker.OnWhistled -= HandleWhistled;
            _inventory.OnItemPickedUp -= HandleItemPickedUp;
            _dropper.OnDropped -= HandleDropped;
            _fragileShake.OnItemBroken -= HandleItemBroken;
        }

        private void HandleRunStep() => _sounds.RunStep.PlayOn(_source);

        private void HandleWhistled() => _sounds.Whistle.PlayOn(_source);

        private void HandleItemPickedUp(PickupItem item) => _sounds.Pickup.PlayOn(_source);

        private void HandleDropped() => _sounds.Drop.PlayOn(_source);

        private void HandleItemBroken() => _sounds.GlassBreak.PlayOn(_source);
    }
}
