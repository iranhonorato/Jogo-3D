using System;
using System.Collections.Generic;
using Guaxinim.Items;
using UnityEngine;

namespace Guaxinim.Player
{
    /// <summary>
    /// Itens carregados pelo guaxinim (GDD §7). Avisa qualquer mudança em <see cref="OnChanged"/>.
    /// </summary>
    public class Inventory : MonoBehaviour, IItemCarrier
    {
        private readonly List<PickupItem> _items = new List<PickupItem>();

        public event Action OnChanged;

        public IReadOnlyList<PickupItem> Items => _items;

        /// <summary>Pega o item: ele sai do mundo e entra na lista.</summary>
        public bool TryPickUp(PickupItem item)
        {
            if (item == null || _items.Contains(item))
            {
                return false;
            }

            _items.Add(item);
            item.Stow();
            OnChanged?.Invoke();
            return true;
        }
    }
}
