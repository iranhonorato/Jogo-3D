using System;
using System.Collections.Generic;
using Guaxinim.Items;
using UnityEngine;

namespace Guaxinim.Player
{
    /// <summary>
    /// Itens carregados pelo guaxinim (GDD §7): soma de peso e volume, com os limites do
    /// <see cref="PlayerData"/>. Avisa qualquer mudança em <see cref="OnChanged"/>.
    /// </summary>
    public class Inventory : MonoBehaviour, IItemCarrier
    {
        [SerializeField] private PlayerData _data;

        private readonly List<PickupItem> _items = new List<PickupItem>();

        public event Action OnChanged;

        public IReadOnlyList<PickupItem> Items => _items;
        public float CurrentWeight { get; private set; }
        public float CurrentVolume { get; private set; }
        public float MaxWeight => _data.MaxWeight;
        public float MaxVolume => _data.MaxVolume;

        private void Awake()
        {
            if (_data == null)
            {
                Debug.LogError("Inventory needs Player Data assigned.", this);
                enabled = false;
            }
        }

        /// <summary>Pega o item se couber: ele sai do mundo e entra na lista.</summary>
        public bool TryPickUp(PickupItem item)
        {
            if (item == null || _items.Contains(item) || !CanCarry(item.Data))
            {
                return false;
            }

            _items.Add(item);
            item.Stow();
            Recalculate();
            return true;
        }

        public bool CanCarry(ItemData item) =>
            CarryCapacity.CanCarry(CurrentWeight, CurrentVolume, item.Weight, item.Volume, MaxWeight, MaxVolume);

        private void Recalculate()
        {
            CurrentWeight = 0f;
            CurrentVolume = 0f;
            for (int i = 0; i < _items.Count; i++)
            {
                CurrentWeight += _items[i].Data.Weight;
                CurrentVolume += _items[i].Data.Volume;
            }

            OnChanged?.Invoke();
        }
    }
}
