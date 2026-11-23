using System;
using UnityEngine;

namespace Guaxinim.Items
{
    /// <summary>
    /// Lista de compras da fase (GDD §7.1). Recebe as entregas da toca e avisa em
    /// <see cref="OnItemDelivered"/> e, quando tudo foi entregue, em <see cref="OnCompleted"/> (uma vez só).
    /// </summary>
    public class ShoppingList : MonoBehaviour
    {
        [Tooltip("Itens obrigatórios desta fase (copiados do LevelData na montagem da fase)")]
        [SerializeField] private ItemData[] _required = new ItemData[0];

        private bool _hasCompleted;

        /// <summary>O item entregue e se ele era da lista (false = bônus).</summary>
        public event Action<ItemData, bool> OnItemDelivered;

        public event Action OnCompleted;

        public ShoppingListProgress Progress { get; private set; }

        private void Awake()
        {
            if (_required.Length == 0)
            {
                Debug.LogError("ShoppingList is empty: the level could never be won.", this);
            }

            Progress = new ShoppingListProgress(_required);
        }

        public void Deliver(ItemData item)
        {
            bool wasOnList = Progress.Deliver(item);
            OnItemDelivered?.Invoke(item, wasOnList);
            if (_hasCompleted || !Progress.IsComplete)
            {
                return;
            }

            _hasCompleted = true;
            OnCompleted?.Invoke();
        }
    }
}
