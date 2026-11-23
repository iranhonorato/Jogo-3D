namespace Guaxinim.Shared
{
    /// <summary>Algo que enche de 0 a 1 e pode ser mostrado numa barra (medidor de detecção, chacoalhar).</summary>
    public interface IMeterValue
    {
        float Value { get; }
    }
}
