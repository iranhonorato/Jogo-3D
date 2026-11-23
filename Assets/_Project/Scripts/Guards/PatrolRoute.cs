namespace Guaxinim.Guards
{
    /// <summary>Ordem dos waypoints da patrulha: vai do primeiro ao último e recomeça.</summary>
    public class PatrolRoute
    {
        private readonly int _waypointCount;

        public PatrolRoute(int waypointCount) => _waypointCount = waypointCount;

        public int CurrentIndex { get; private set; }
        public bool IsEmpty => _waypointCount <= 0;

        public void Advance()
        {
            if (IsEmpty)
            {
                return;
            }

            CurrentIndex = (CurrentIndex + 1) % _waypointCount;
        }
    }
}
