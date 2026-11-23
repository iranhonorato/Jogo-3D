using System.Collections.Generic;

namespace Guaxinim.Items
{
    /// <summary>Quem pode carregar itens (o inventário do guaxinim).</summary>
    public interface IItemCarrier
    {
        /// <summary>Pega o item, ou recusa se não couber.</summary>
        bool TryPickUp(PickupItem item);

        /// <summary>Entrega tudo o que carrega (na toca): os itens vão para <paramref name="into"/>.</summary>
        void HandOverAll(List<PickupItem> into);
    }
}
