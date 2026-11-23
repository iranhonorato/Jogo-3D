using UnityEngine;

namespace Guaxinim.Shared
{
    /// <summary>Quem pode entrar num esconderijo (GDD §9). A lixeira usa este contrato, sem conhecer o guaxinim.</summary>
    public interface IHider
    {
        /// <summary>Pode entrar agora? (não pego, sem carga volumosa, fora de outra transição)</summary>
        bool CanHide { get; }

        bool IsHidden { get; }

        /// <summary>Entrando ou saindo (0,5 s): não aceita outro comando.</summary>
        bool IsInTransition { get; }

        void EnterHiding(Vector3 spotPosition);

        void ExitHiding(Vector3 exitPosition);
    }
}
