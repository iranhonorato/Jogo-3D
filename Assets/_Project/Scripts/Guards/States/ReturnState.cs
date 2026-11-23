namespace Guaxinim.Guards.States
{
    /// <summary>Retorno (GDD §6.2): volta, na velocidade de patrulha, ao waypoint de onde saiu e retoma a Patrulha.</summary>
    public class ReturnState : IGuardState
    {
        private readonly GuardAI _guard;

        public ReturnState(GuardAI guard) => _guard = guard;

        public void Enter()
        {
            _guard.Agent.speed = _guard.Data.PatrolSpeed;
            _guard.MoveTo(_guard.Patrol.CurrentWaypointPosition);
        }

        public void Tick(float deltaTime)
        {
            if (_guard.HasArrived())
            {
                _guard.ChangeState(_guard.Patrol);
            }
        }

        public void Exit()
        {
        }
    }
}
