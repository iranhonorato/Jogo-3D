using System.Collections.Generic;
using UnityEngine;

namespace Guaxinim.Items
{
    /// <summary>
    /// Contorno sutil nos itens da lista que ainda faltam, quando o guaxinim está perto (GDD §7.1).
    /// Os itens da fase são listados uma vez, no Start; depois só se percorre a lista.
    /// </summary>
    public class ShoppingListHighlighter : MonoBehaviour
    {
        [SerializeField] private ShoppingList _shoppingList;

        [Tooltip("O guaxinim (ligado na montagem da fase)")]
        [SerializeField] private Transform _player;

        [Tooltip("Metros: itens da lista mais perto que isto ganham o contorno")]
        [SerializeField, Min(0f)] private float _radius = 3f;

        private readonly List<PickupItem> _items = new List<PickupItem>();

        private void Awake()
        {
            if (_shoppingList == null || _player == null)
            {
                Debug.LogError("ShoppingListHighlighter needs Shopping List and Player assigned.", this);
                enabled = false;
            }
        }

        private void Start()
        {
            // Só no setup: os itens da fase não mudam (largar reaproveita o mesmo objeto).
            _items.AddRange(FindObjectsByType<PickupItem>(FindObjectsInactive.Include, FindObjectsSortMode.None));
        }

        private void Update()
        {
            Vector3 playerPosition = _player.position;
            float sqrRadius = _radius * _radius;
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                PickupItem item = _items[i];
                if (item == null)
                {
                    _items.RemoveAt(i); // entregue ou quebrado
                    continue;
                }

                bool isNear = (item.transform.position - playerPosition).sqrMagnitude <= sqrRadius;
                item.SetHighlighted(isNear && _shoppingList.Progress.IsPending(item.Data));
            }
        }
    }
}
