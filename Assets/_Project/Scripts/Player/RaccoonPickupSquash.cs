using Guaxinim.Animation;
using Guaxinim.Items;
using UnityEngine;

namespace Guaxinim.Player
{
    /// <summary>O guaxinim achata um pouco ao pegar um item ou consumível (resposta visual à coleta).</summary>
    [RequireComponent(typeof(Inventory))]
    public class RaccoonPickupSquash : MonoBehaviour
    {
        [SerializeField] private ProceduralMotion _motion;

        private Inventory _inventory;

        private void Awake()
        {
            _inventory = GetComponent<Inventory>();
            if (_motion == null)
            {
                Debug.LogError("RaccoonPickupSquash needs Motion assigned.", this);
                enabled = false;
            }
        }

        private void OnEnable() => _inventory.OnItemPickedUp += HandleItemPickedUp;

        private void OnDisable() => _inventory.OnItemPickedUp -= HandleItemPickedUp;

        private void HandleItemPickedUp(PickupItem item) => _motion.Squash();
    }
}
