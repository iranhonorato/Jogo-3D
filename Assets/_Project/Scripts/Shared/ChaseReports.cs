using System;
using UnityEngine;

namespace Guaxinim.Shared
{
    /// <summary>
    /// Mediador das perseguições: o inimigo que começa a perseguir avisa aqui, sem saber quem escuta. O ciclo da fase
    /// escuta para a terceira estrela ("sem ser perseguido", GDD §12), no lugar do alerta geral, que ficou fora do escopo final.
    /// </summary>
    public static class ChaseReports
    {
        public static event Action OnChaseStarted;

        public static void ReportChaseStarted() => OnChaseStarted?.Invoke();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => OnChaseStarted = null;
    }
}
