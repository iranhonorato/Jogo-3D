using System;
using Guaxinim.Shared;
using UnityEngine;

namespace Guaxinim.Player
{
    /// <summary>
    /// Efeitos da carga volumosa (GDD §7.4), ligados e desligados conforme o <see cref="Inventory"/> muda:
    /// a visão fica tapada (<see cref="ViewObstruction"/>) e o guaxinim deixa de atravessar a layer NarrowGap.
    /// A UI escuta <see cref="OnBulkyChanged"/> para a vinheta.
    /// </summary>
    [RequireComponent(typeof(Inventory))]
    public class RaccoonBulkyLoad : MonoBehaviour
    {
        [SerializeField] private PlayerData _data;

        private Inventory _inventory;
        private int _playerLayer;
        private int _narrowGapLayer;

        public event Action<bool> OnBulkyChanged;

        public bool IsBulky { get; private set; }

        private void Awake()
        {
            _inventory = GetComponent<Inventory>();
            _playerLayer = LayerMask.NameToLayer(Layers.Player);
            _narrowGapLayer = LayerMask.NameToLayer(Layers.NarrowGap);
            if (_data == null)
            {
                Debug.LogError("RaccoonBulkyLoad needs Player Data assigned.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            _inventory.OnChanged += HandleInventoryChanged;
            Apply(false); // a fase sempre começa sem carga
        }

        private void OnDisable()
        {
            _inventory.OnChanged -= HandleInventoryChanged;
            Apply(false); // não deixa a regra global ligada ao reiniciar a fase
        }

        private void HandleInventoryChanged()
        {
            bool isBulky = CarryCapacity.IsBulky(_inventory.CurrentVolume, _data.BulkyVolumeThreshold);
            if (isBulky != IsBulky)
            {
                Apply(isBulky);
                OnBulkyChanged?.Invoke(isBulky);
            }
        }

        private void Apply(bool isBulky)
        {
            IsBulky = isBulky;
            Physics.IgnoreLayerCollision(_playerLayer, _narrowGapLayer, !isBulky);
            if (isBulky)
            {
                ViewObstruction.Begin(transform, _data.BulkyClearViewRadius);
            }
            else
            {
                ViewObstruction.End(transform);
            }
        }
    }
}
