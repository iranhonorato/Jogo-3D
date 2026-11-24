using UnityEngine;

namespace Guaxinim.Shared
{
    /// <summary>
    /// Tudo que responde ao E (GDD §3.2): item, lixeira, aspirador. O guaxinim escolhe o mais próximo na
    /// layer Interactable e chama <see cref="Interact"/>, sem saber o que é.
    /// </summary>
    public interface IInteractable
    {
        Vector3 InteractionPoint { get; }

        /// <summary>O que o E faz agora, em uma palavra ("Pegar", "Esconder", "Sair"); aparece na indicação da HUD.</summary>
        string PromptText { get; }

        void Interact(GameObject interactor);
    }
}
