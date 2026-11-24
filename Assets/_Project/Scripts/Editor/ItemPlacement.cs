namespace Guaxinim.EditorTools
{
    /// <summary>Item no mundo, pelo nome da tabela do GDD §7.8 (ex.: "Bread").</summary>
    public readonly struct ItemPlacement
    {
        public ItemPlacement(string itemName, Placement placement)
        {
            ItemName = itemName;
            Placement = placement;
        }

        public string ItemName { get; }
        public Placement Placement { get; }
    }
}
