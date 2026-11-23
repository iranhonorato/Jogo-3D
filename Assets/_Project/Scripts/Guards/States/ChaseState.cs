using Guaxinim.Shared;
using UnityEngine;

namespace Guaxinim.Guards.States
{
    /// <summary>
    /// Perseguição (GDD §6.2 e §6.5): corre atrás do guaxinim a 5,5 m/s e o pega a menos de 1 m.
    /// Sem vê-lo por um instante, vai até a última posição conhecida e passa para a Busca.
    /// </summary>
    public class ChaseState : IGuardState
    {
        private readonly GuardAI _guard;
        private readonly LostSightTimer _lostSight;
        private IDetectable _target;
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
            _target = _guard.Vision.VisibleTarget;
            _lastKnownPosition = _target != null ? _target.DetectionPoint : _guard.transform.position;
            // TODO(passo 3.3): disparar o alerta geral (GDD §6.4).
        }

        public void Tick(float deltaTime)
        {
            bool isTargetSeen = _guard.Vision.HasVisibleTarget;
            if (isTargetSeen)
            {
                _target = _guard.Vision.VisibleTarget;
                _lastKnownPosition = _target.DetectionPoint;
            }

            if (TryCatchTarget())
            {
                return;
            }

            if (_lostSight.Tick(isTargetSeen, deltaTime))
            {
                _guard.Search.SetPoint(_lastKnownPosition);
                _guard.ChangeState(_guard.Search);
                return;
            }

            RepathIfDue(deltaTime);
        }

        public void Exit() => _target = null;

        /// <summary>Pega pela distância, mesmo fora do cone: colado de lado também conta.</summary>
        private bool TryCatchTarget()
        {
            if (_target == null)
            {
                return false;
            }

            float distance = VisionMath.FlatDistance(_guard.transform.position, _target.DetectionPoint);
            if (!CatchRule.CanCatch(distance, _guard.Data.CatchDistance, _target.IsUntouchable))
            {
                return false;
            }

            _target.Catch();
            _guard.Agent.isStopped = true;
            return true;
        }

        private void RepathIfDue(float deltaTime)
        {
            _secondsUntilRepath -= deltaTime;
            if (_secondsUntilRepath > 0f)
            {
                return;
            }

            _secondsUntilRepath = _guard.Data.ChaseRepathSeconds;
            _guard.MoveTo(_lastKnownPosition);
        }
    }
}
