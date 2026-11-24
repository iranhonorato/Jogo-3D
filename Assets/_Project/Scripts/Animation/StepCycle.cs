namespace Guaxinim.Animation
{
    /// <summary>
    /// Fase do ciclo de passos (de 0 a 2), pela distância andada: quem anda mais rápido dá passos mais rápidos, e
    /// parado o ciclo não avança. Assim o passo nunca escorrega em relação ao chão.
    /// </summary>
    public class StepCycle
    {
        private const float StepsPerCycle = 2f;

        public float Phase { get; private set; }

        public void Advance(float distanceMeters, float stepLengthMeters)
        {
            if (stepLengthMeters <= 0f || distanceMeters <= 0f)
            {
                return;
            }

            Phase = (Phase + distanceMeters / stepLengthMeters) % StepsPerCycle;
        }
    }
}
