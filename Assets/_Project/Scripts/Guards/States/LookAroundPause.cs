using UnityEngine;

namespace Guaxinim.Guards.States
{
    /// <summary>
    /// "Parar e olhar em volta", usado na Patrulha, no Desconfiado e na Busca: o segurança fica parado
    /// e balança a cabeça de um lado para o outro até o tempo acabar.
    /// </summary>
    public class LookAroundPause
    {
        private readonly GuardAI _guard;
        private float _remainingSeconds;
        private float _elapsedSeconds;
        private float _baseYawDegrees;

        public LookAroundPause(GuardAI guard) => _guard = guard;

        public bool IsActive { get; private set; }

        public void Begin(float durationSeconds)
        {
            IsActive = true;
            _remainingSeconds = durationSeconds;
            _elapsedSeconds = 0f;
            _baseYawDegrees = _guard.transform.eulerAngles.y;
            _guard.Agent.isStopped = true;
            _guard.Agent.updateRotation = false; // durante a pausa, quem gira é a olhada
        }

        /// <summary>Devolve true quando o tempo da pausa acabou.</summary>
        public bool Tick(float deltaTime)
        {
            _elapsedSeconds += deltaTime;
            float yawOffset = LookAround.GetYawOffset(_elapsedSeconds, _guard.Data.LookAroundAngleDegrees,
                _guard.Data.LookAroundPeriodSeconds);
            _guard.transform.rotation = Quaternion.Euler(0f, _baseYawDegrees + yawOffset, 0f);

            _remainingSeconds -= deltaTime;
            return _remainingSeconds <= 0f;
        }

        public void End()
        {
            IsActive = false;
            _guard.Agent.isStopped = false;
            _guard.Agent.updateRotation = true;
        }
    }
}
