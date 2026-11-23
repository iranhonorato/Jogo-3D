namespace Guaxinim.Items
{
    /// <summary>Quem pode carregar itens (o inventário do guaxinim). Recusa se não couber.</summary>
    public interface IItemCarrier
    {
        bool TryPickUp(PickupItem item);
    }
}
