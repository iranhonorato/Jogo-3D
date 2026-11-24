using UnityEngine;

namespace Guaxinim.Core
{
    /// <summary>
    /// Pedido de uma tela ao menu principal que vai carregar: por exemplo, a tela de fim de jogo pede para o menu
    /// abrir direto nos Créditos. Vale uma vez só e morre com o Play Mode.
    /// </summary>
    public static class MenuRequest
    {
        private static bool _showCredits;

        public static void RequestCredits() => _showCredits = true;

        /// <summary>Devolve se o menu deve abrir nos Créditos e limpa o pedido.</summary>
        public static bool ConsumeCreditsRequest()
        {
            bool wasRequested = _showCredits;
            _showCredits = false;
            return wasRequested;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => _showCredits = false;
    }
}
