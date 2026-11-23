using UnityEngine;

namespace Guaxinim.Shared
{
    /// <summary>
    /// Algo que os seguranças e as câmeras podem ver (GDD §6.1). Os detectores dependem deste
    /// contrato, não do guaxinim.
    /// </summary>
    public interface IDetectable
    {
        /// <summary>Ponto do corpo usado no raycast da visão.</summary>
        Vector3 DetectionPoint { get; }

        /// <summary>Falso dentro da lixeira e no modo cogumelo: nunca é visto.</summary>
        bool IsVisible { get; }

        /// <summary>Verdadeiro no modo cogumelo: não pode ser pego.</summary>
        bool IsUntouchable { get; }

        /// <summary>O segurança alcançou o alvo (GDD §6.5). Ignorado se intocável ou já pego.</summary>
        void Catch();
    }
}
