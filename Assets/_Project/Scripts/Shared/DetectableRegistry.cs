using System.Collections.Generic;
using UnityEngine;

namespace Guaxinim.Shared
{
    /// <summary>
    /// Lista dos <see cref="IDetectable"/> ativos na cena. Quem pode ser visto se registra no OnEnable;
    /// os detectores percorrem a lista, sem buscar objetos na cena durante o jogo.
    /// </summary>
    public static class DetectableRegistry
    {
        private static readonly List<IDetectable> Detectables = new List<IDetectable>();

        public static IReadOnlyList<IDetectable> All => Detectables;

        public static void Register(IDetectable detectable)
        {
            if (!Detectables.Contains(detectable))
            {
                Detectables.Add(detectable);
            }
        }

        public static void Unregister(IDetectable detectable) => Detectables.Remove(detectable);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Detectables.Clear();
    }
}
