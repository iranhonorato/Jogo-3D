using UnityEngine;

namespace Guaxinim.Guards.States
{
    /// <summary>
    /// Patrulha (GDD §6.2): segue os waypoints a 2 m/s e, em cada um, para e olha em volta.
    /// Sem waypoints, fica parado olhando em volta.
    /// </summary>
    public class PatrolState : IGuardState
    {
        private readonly GuardAI _guard;
        private readonly PatrolRoute _route;
        private bool _isPausing;
        private float _pauseRemainingSeconds;
        private float _lookElapsedSeconds;
        private float _baseYawDegrees;

        public PatrolState(GuardAI guard)
        {
            _guard = guard;
            _route = new PatrolRoute(guard.Waypoints.Count);
        }

        public void Enter()
        {
            _guard.Agent.speed = _guard.Data.PatrolSpeed;
            if (_route.IsEmpty)
            {
                StartPause();
                return;
            }

            MoveToCurrentWaypoint();
        }

        public void Tick(float deltaTime)
        {
            if (_isPausing)
            {
                TickPause(deltaTime);
                return;
            }

            if (HasArrived())
            {
                StartPause();
            }
        }

        public void Exit()
        {
            _isPausing = false;
            _guard.Agent.isStopped = false;
            _guard.Agent.updateRotation = true;
        }

        private void MoveToCurrentWaypoint()
        {
            _isPausing = false;
            _guard.Agent.isStopped = false;
            _guard.Agent.updateRotation = true;
            _guard.Agent.SetDestination(_guard.Waypoints[_route.CurrentIndex].position);
        }

        private bool HasArrived() =>
            !_guard.Agent.pathPending && _guard.Agent.remainingDistance <= _guard.Data.ArrivalDistance;

        private void StartPause()
        {
            _isPausing = true;
            _guard.Agent.isStopped = true;
            _guard.Agent.updateRotation = false; // durante a pausa, quem gira é a olhada em volta
            _pauseRemainingSeconds = _guard.Data.WaypointPauseSeconds;
            _lookElapsedSeconds = 0f;
            _baseYawDegrees = _guard.transform.eulerAngles.y;
        }

        private void TickPause(float deltaTime)
        {
            _lookElapsedSeconds += deltaTime;
            float yawOffset = LookAround.GetYawOffset(_lookElapsedSeconds, _guard.Data.LookAroundAngleDegrees,
                _guard.Data.LookAroundPeriodSeconds);
            _guard.transform.rotation = Quaternion.Euler(0f, _baseYawDegrees + yawOffset, 0f);

            _pauseRemainingSeconds -= deltaTime;
            if (_pauseRemainingSeconds > 0f)
            {
                return;
            }

            if (_route.IsEmpty)
            {
                StartPause();
                return;
            }

            _route.Advance();
            MoveToCurrentWaypoint();
        }
    }
}
