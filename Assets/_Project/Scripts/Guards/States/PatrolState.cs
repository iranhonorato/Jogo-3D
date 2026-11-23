using UnityEngine;

namespace Guaxinim.Guards.States
{
    /// <summary>
    /// Patrulha (GDD §6.2): segue os waypoints a 2 m/s e, em cada um, para e olha em volta.
    /// Sem waypoints, fica no ponto inicial olhando em volta.
    /// </summary>
    public class PatrolState : IGuardState
    {
        private readonly GuardAI _guard;
        private readonly PatrolRoute _route;
        private readonly LookAroundPause _pause;

        public PatrolState(GuardAI guard)
        {
            _guard = guard;
            _route = new PatrolRoute(guard.Waypoints.Count);
            _pause = new LookAroundPause(guard);
        }

        /// <summary>Para onde o Retorno leva: o waypoint atual, ou o ponto inicial sem waypoints.</summary>
        public Vector3 CurrentWaypointPosition =>
            _route.IsEmpty ? _guard.HomePosition : _guard.Waypoints[_route.CurrentIndex].position;

        public void Enter()
        {
            _guard.Agent.speed = _guard.Data.PatrolSpeed;
            _guard.MoveTo(CurrentWaypointPosition);
        }

        public void Tick(float deltaTime)
        {
            if (!_pause.IsActive)
            {
                if (_guard.HasArrived())
                {
                    _pause.Begin(_guard.Data.WaypointPauseSeconds);
                }

                return;
            }

            if (!_pause.Tick(deltaTime))
            {
                return;
            }

            _pause.End();
            _route.Advance();
            _guard.MoveTo(CurrentWaypointPosition);
        }

        public void Exit() => _pause.End();
    }
}
