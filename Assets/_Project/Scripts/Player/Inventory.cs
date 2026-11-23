using System;
using System.Collections.Generic;
using Guaxinim.Items;
using UnityEngine;

namespace Guaxinim.Player
{
    /// <summary>
    /// Itens carregados pelo guaxinim (GDD §7): soma de peso e volume, com os limites do
    /// <see cref="PlayerData"/> e o bônus da barra de proteína. Consumíveis viram efeito na hora (GDD §8.1).
    /// Avisa qualquer mudança em <see cref="OnChanged"/>.
    /// </summary>
    [RequireComponent(typeof(RaccoonEffects))]
    public class Inventory : MonoBehaviour, IItemCarrier
    {
        [SerializeField] private PlayerData _data;

        private readonly List<PickupItem> _items = new List<PickupItem>();
        private RaccoonEffects _effects;

        public event Action OnChanged;

        public IReadOnlyList<PickupItem> Items => _items;
        public float CurrentWeight { get; private set; }
        public float CurrentVolume { get; private set; }
        public bool HasFragile { get; private set; }
        public float MaxWeight => _data.MaxWeight + _effects.MaxWeightBonus;

        /// <summary>GDD §8.1: a proteína acabou com mais peso que o máximo; fica lento e não pega nada.</summary>
        public bool IsOverweight => CurrentWeight > MaxWeight;
        public float MaxVolume => _data.MaxVolume;

        private void Awake()
        {
            _effects = GetComponent<RaccoonEffects>();
            if (_data == null)
            {
                Debug.LogError("Inventory needs Player Data assigned.", this);
                enabled = false;
            }
        }

        /// <summary>Pega o item se couber: ele sai do mundo e entra na lista. Consumível é usado na hora.</summary>
        public bool TryPickUp(PickupItem item)
        {
            if (item == null || _items.Contains(item))
            {
                return false;
            }

            if (item.Data.IsConsumable)
            {
                Consume(item);
                return true;
            }

            if (!CanCarry(item.Data))
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

        /// <summary>Entrega tudo na toca (GDD §7.7): os itens saem do inventário e vão para a lista.</summary>
        public void HandOverAll(List<PickupItem> into)
        {
            if (_items.Count == 0)
            {
                return;
            }

            into.AddRange(_items);
            _items.Clear();
            Recalculate();
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

        private void Consume(PickupItem item)
        {
            if (item.Data.Effect != null)
            {
                _effects.Apply(item.Data.Effect.CreateEffect());
            }
            else
            {
                Debug.LogError($"Consumable {item.Data.name} has no Effect assigned.", item);
            }

            Destroy(item.gameObject);
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
