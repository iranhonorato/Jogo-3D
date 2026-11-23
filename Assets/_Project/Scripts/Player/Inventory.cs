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
        public bool HasFragile { get; private set; }
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

        /// <summary>Larga o último item pego (GDD §7.6): ele volta ao mundo e pode ser pego de novo.</summary>
        public bool TryDropLast(Vector3 position, Quaternion rotation)
        {
            if (_items.Count == 0)
            {
                return false;
            }

            PickupItem item = _items[_items.Count - 1];
            _items.RemoveAt(_items.Count - 1);
            item.PlaceAt(position, rotation);
            Recalculate();
            return true;
        }

        /// <summary>Quebra o último item frágil pego (GDD §7.5): ele some de vez.</summary>
        public bool TryBreakLastFragile()
        {
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                if (!_items[i].Data.IsFragile)
                {
                    continue;
                }

                PickupItem broken = _items[i];
                _items.RemoveAt(i);
                Destroy(broken.gameObject);
                Recalculate();
                return true;
            }

            return false;
        }

        public bool CanCarry(ItemData item) =>
            CarryCapacity.CanCarry(CurrentWeight, CurrentVolume, item.Weight, item.Volume, MaxWeight, MaxVolume);

        private void Recalculate()
        {
            CurrentWeight = 0f;
            CurrentVolume = 0f;
            HasFragile = false;
            for (int i = 0; i < _items.Count; i++)
            {
                CurrentWeight += _items[i].Data.Weight;
                CurrentVolume += _items[i].Data.Volume;
                HasFragile |= _items[i].Data.IsFragile;
            }

            OnChanged?.Invoke();
        }
    }
}
