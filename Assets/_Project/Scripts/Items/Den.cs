using System.Collections.Generic;
using UnityEngine;

namespace Guaxinim.Items
{
    /// <summary>
    /// A toca (GDD §7.7): ao entrar na área, todos os itens carregados são entregues automaticamente à
    /// <see cref="ShoppingList"/>. Com a lista completa, a lista avisa e a fase termina.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Den : MonoBehaviour
    {
        [SerializeField] private ShoppingList _shoppingList;

        private readonly List<PickupItem> _delivered = new List<PickupItem>();

        private void Awake()
        {
            if (_shoppingList == null)
            {
                Debug.LogError("Den needs the level Shopping List assigned.", this);
                enabled = false;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!enabled || !other.TryGetComponent(out IItemCarrier carrier))
            {
                return;
            }

            carrier.HandOverAll(_delivered);
            foreach (PickupItem item in _delivered)
            {
                _shoppingList.Deliver(item.Data);
                Destroy(item.gameObject); // entregue: sai da fase de vez
            }

            _delivered.Clear();
        }
    }
}
