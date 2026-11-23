namespace Guaxinim.Guards
{
    /// <summary>
    /// GDD §9: o segurança em Perseguição que via o guaxinim no frame anterior e o perdeu porque ele se
    /// escondeu (ficou invisível, não saiu do cone) viu o guaxinim entrar: vai abrir a lixeira.
    /// </summary>
    public static class HidingWitness
    {
        public static bool SawTargetHide(bool wasSeenLastFrame, bool isSeenNow, bool isTargetVisible) =>
            wasSeenLastFrame && !isSeenNow && !isTargetVisible;
    }
}
