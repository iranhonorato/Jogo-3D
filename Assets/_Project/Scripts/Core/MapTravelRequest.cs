using UnityEngine;

namespace Guaxinim.Core
{
    /// <summary>
    /// Pedido de viagem no mapa (plano de próximos passos, Etapa 4): o resultado da fase registra de onde o guaxinim
    /// vem e para onde vai antes de carregar o mapa, e o mapa toca a viagem pela estrada. Vale uma vez só e morre
    /// com o Play Mode.
    /// </summary>
    public static class MapTravelRequest
    {
        private static LevelData _from;
        private static LevelData _to;

        public static void Request(LevelData from, LevelData to)
        {
            _from = from;
            _to = to;
        }

        /// <summary>Devolve a viagem pedida, se houver, e limpa o pedido.</summary>
        public static bool TryConsume(out LevelData from, out LevelData to)
        {
            from = _from;
            to = _to;
            bool hasRequest = from != null && to != null;
            ResetStatics();
            return hasRequest;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _from = null;
            _to = null;
        }
    }
}
