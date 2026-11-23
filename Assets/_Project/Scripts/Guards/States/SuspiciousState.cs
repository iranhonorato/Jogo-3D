namespace Guaxinim.Guards.States
{
    /// <summary>Desconfiado (GDD §6.2): ouviu um som ou viu de relance; vai até o ponto a 3 m/s e olha em volta por 4 s.</summary>
    public class SuspiciousState : InvestigateState
    {
        public SuspiciousState(GuardAI guard) : base(guard)
        {
        }

        protected override float MoveSpeed => Guard.Data.SuspiciousSpeed;
        protected override float LookAroundSeconds => Guard.Data.SuspiciousLookSeconds;
    }
}
