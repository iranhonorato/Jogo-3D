namespace Guaxinim.Guards
{
    /// <summary>
    /// Conta quanto tempo o alvo está fora de vista. Uma tolerância curta evita que a Perseguição
    /// vire Busca só porque o guaxinim passou um instante atrás de uma quina.
    /// </summary>
    public class LostSightTimer
    {
        private readonly float _graceSeconds;
        private float _unseenSeconds;

        public LostSightTimer(float graceSeconds) => _graceSeconds = graceSeconds;

        /// <summary>Devolve true quando o alvo está sumido há pelo menos a tolerância.</summary>
        public bool Tick(bool isTargetSeen, float deltaTime)
        {
            if (isTargetSeen)
            {
                _unseenSeconds = 0f;
                return false;
            }

            _unseenSeconds += deltaTime;
            return _unseenSeconds >= _graceSeconds;
        }

        public void Reset() => _unseenSeconds = 0f;
    }
}
