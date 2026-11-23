namespace Guaxinim.Guards
{
    /// <summary>Um estado do segurança (GDD §6.2). Cada estado decide quando passar para outro.</summary>
    public interface IGuardState
    {
        void Enter();

        void Tick(float deltaTime);

        void Exit();
    }
}
