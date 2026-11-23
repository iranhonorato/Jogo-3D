using UnityEngine;

namespace Guaxinim.Guards.States
{
    /// <summary>
    /// Perseguição (GDD §6.2): corre atrás do guaxinim a 5,5 m/s. Sem vê-lo por um instante, vai até a
    /// última posição conhecida e passa para a Busca.
    /// </summary>
    public class ChaseState : IGuardState
    {
        private readonly GuardAI _guard;
        private readonly LostSightTimer _lostSight;
        private Vector3 _lastKnownPosition;
        private float _secondsUntilRepath;

        public ChaseState(GuardAI guard)
        {
            _guard = guard;
            _lostSight = new LostSightTimer(guard.Data.LostSightGraceSeconds);
        }

        public void Enter()
        {
            _guard.Agent.speed = _guard.Data.ChaseSpeed;
            _lostSight.Reset();
            _secondsUntilRepath = 0f;
            _lastKnownPosition = _guard.Vision.HasVisibleTarget
                ? _guard.Vision.VisibleTarget.DetectionPoint
                : _guard.transform.position;
            // TODO(passo 3.3): disparar o alerta geral (GDD §6.4).
        }

        public void Tick(float deltaTime)
        {
            bool isTargetSeen = _guard.Vision.HasVisibleTarget;
            if (isTargetSeen)
            {
                _lastKnownPosition = _guard.Vision.VisibleTarget.DetectionPoint;
            }

            if (_lostSight.Tick(isTargetSeen, deltaTime))
            {
                _guard.Search.SetPoint(_lastKnownPosition);
                _guard.ChangeState(_guard.Search);
                return;
            }

            _secondsUntilRepath -= deltaTime;
            if (_secondsUntilRepath <= 0f)
            {
                _secondsUntilRepath = _guard.Data.ChaseRepathSeconds;
                _guard.MoveTo(_lastKnownPosition);
            }
        }

        public void Exit()
        {
        }
    }
}
