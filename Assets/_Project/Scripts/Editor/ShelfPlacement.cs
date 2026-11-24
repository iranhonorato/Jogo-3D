namespace Guaxinim.EditorTools
{
    /// <summary>Gôndola de 2 m ou de 4 m.</summary>
    public readonly struct ShelfPlacement
    {
        public ShelfPlacement(bool isLong, Placement placement)
        {
            IsLong = isLong;
            Placement = placement;
        }

        public bool IsLong { get; }
        public Placement Placement { get; }
    }
}
