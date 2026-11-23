using UnityEngine;

namespace Guaxinim.Guards.States
{
    /// <summary>
    /// Base do Desconfiado e da Busca (GDD §6.2): vai até um ponto e olha em volta por um tempo;
    /// depois passa para o Retorno. Cada subclasse define só a velocidade e o tempo de procura.
    /// Um ponto novo (outro som, outro relance) faz o segurança voltar a andar até ele.
    /// </summary>
    public abstract class InvestigateState : IGuardState
    {
        private readonly LookAroundPause _pause;
        private Vector3 _point;
        private bool _isActive;

        protected InvestigateState(GuardAI guard)
        {
            Guard = guard;
            _pause = new LookAroundPause(guard);
        }

        protected GuardAI Guard { get; }
        protected abstract float MoveSpeed { get; }
        protected abstract float LookAroundSeconds { get; }

        public void SetPoint(Vector3 point)
        {
            _point = point;
            if (_isActive)
            {
                StartMoving();
            }
        }

        public void Enter()
        {
            _isActive = true;
            Guard.Agent.speed = MoveSpeed;
            StartMoving();
        }

        public void Tick(float deltaTime)
        {
            if (!_pause.IsActive)
            {
                if (Guard.HasArrived())
                {
                    _pause.Begin(LookAroundSeconds);
                }

                return;
            }

            if (_pause.Tick(deltaTime))
            {
                Guard.ChangeState(Guard.Return);
            }
        }

        public void Exit()
        {
            _isActive = false;
            _pause.End();
        }

        private void StartMoving()
        {
            _pause.End();
            Guard.MoveTo(_point);
        }
    }
}
