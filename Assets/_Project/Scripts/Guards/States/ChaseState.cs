using Guaxinim.Shared;
using UnityEngine;

namespace Guaxinim.Guards.States
{
    /// <summary>
    /// Perseguição (GDD §6.2, §6.5 e §9): corre atrás do guaxinim a 5,5 m/s e o pega a menos de 1 m.
    /// Se o viu entrar numa lixeira, vai até ela e o pega lá dentro. Sem vê-lo por um instante (e sem
    /// tê-lo visto se esconder), vai até a última posição conhecida e passa para a Busca.
    /// </summary>
    public class ChaseState : IGuardState
    {
        private readonly GuardAI _guard;
        private readonly LostSightTimer _lostSight;
        private IDetectable _target;
        private Vector3 _lastKnownPosition;
        private float _secondsUntilRepath;
        private bool _wasSeenLastFrame;
        private bool _sawTargetHide;

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
            _sawTargetHide = false;
            _target = _guard.Vision.VisibleTarget;
            _wasSeenLastFrame = _target != null;
            _lastKnownPosition = _target != null ? _target.DetectionPoint : _guard.transform.position;
            // TODO(passo 3.3): disparar o alerta geral (GDD §6.4).
        }

        public void Tick(float deltaTime)
        {
            bool isTargetSeen = UpdateSight();
            if (TryCatchTarget())
            {
                return;
            }

            if (!_sawTargetHide && _lostSight.Tick(isTargetSeen, deltaTime))
            {
                _guard.Search.SetPoint(_lastKnownPosition);
                _guard.ChangeState(_guard.Search);
                return;
            }

            RepathIfDue(deltaTime);
        }

        public void Exit() => _target = null;

        private bool UpdateSight()
        {
            bool isTargetSeen = _guard.Vision.HasVisibleTarget;
            if (isTargetSeen)
            {
                _target = _guard.Vision.VisibleTarget;
                _lastKnownPosition = _target.DetectionPoint;
            }
            else if (_target != null && HidingWitness.SawTargetHide(_wasSeenLastFrame, false, _target.IsVisible))
            {
                _sawTargetHide = true; // vai até a lixeira, sem desistir
            }

            _wasSeenLastFrame = isTargetSeen;
            return isTargetSeen;
        }

        /// <summary>Pega pela distância, mesmo fora do cone: colado de lado (ou na lixeira) também conta.</summary>
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

            if (!_target.IsVisible && !_sawTargetHide)
            {
                return false; // escondido sem ter sido visto entrando: o segurança não sabe que está ali
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
