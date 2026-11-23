using System.Collections.Generic;

namespace Guaxinim.Items
{
    /// <summary>
    /// Progresso da lista de compras (GDD §7.1 e §7.7): cada entrega marca uma unidade ainda pendente
    /// da lista; o que não está na lista (ou já foi entregue) conta como bônus.
    /// </summary>
    public class ShoppingListProgress
    {
        private readonly List<ItemData> _required;
        private readonly bool[] _isDelivered;

        public ShoppingListProgress(IReadOnlyList<ItemData> required)
        {
            _required = new List<ItemData>(required);
            _isDelivered = new bool[_required.Count];
        }

        public int RequiredCount => _required.Count;
        public int DeliveredCount { get; private set; }
        public int BonusCount { get; private set; }
        public bool IsComplete => DeliveredCount >= RequiredCount;

        public ItemData GetRequired(int index) => _required[index];

        public bool IsDelivered(int index) => _isDelivered[index];

        /// <summary>Entrega um item. Devolve true se ele marcou um item da lista; false se foi bônus.</summary>
        public bool Deliver(ItemData item)
        {
            int index = FindPending(item);
            if (index < 0)
            {
                BonusCount++;
                return false;
            }

            _isDelivered[index] = true;
            DeliveredCount++;
            return true;
        }

        /// <summary>O item ainda falta na lista? (usado no contorno dos itens por perto)</summary>
        public bool IsPending(ItemData item) => FindPending(item) >= 0;

        private int FindPending(ItemData item)
        {
            for (int i = 0; i < _required.Count; i++)
            {
                if (!_isDelivered[i] && _required[i] == item)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
