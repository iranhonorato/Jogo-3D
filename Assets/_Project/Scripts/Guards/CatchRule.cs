namespace Guaxinim.Guards
{
    /// <summary>GDD §6.5: em Perseguição, a menos de 1 m, o guaxinim é pego (a não ser que esteja intocável).</summary>
    public static class CatchRule
    {
        public static bool CanCatch(float distanceMeters, float catchDistanceMeters, bool isTargetUntouchable) =>
            !isTargetUntouchable && distanceMeters <= catchDistanceMeters;
    }
}
