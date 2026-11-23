namespace Guaxinim.Guards.States
{
    /// <summary>Busca (GDD §6.2): perdeu o guaxinim de vista; vai até a última posição conhecida e procura por 6 s.</summary>
    public class SearchState : InvestigateState
    {
        public SearchState(GuardAI guard) : base(guard)
        {
        }

        protected override float MoveSpeed => Guard.Data.SearchSpeed;
        protected override float LookAroundSeconds => Guard.Data.SearchLookSeconds;
    }
}
