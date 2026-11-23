using System;
using Guaxinim.Shared;
using UnityEngine;

namespace Guaxinim.Player
{
    /// <summary>
    /// Estado do guaxinim visto de fora (GDD §14.3): o que os seguranças e as câmeras podem saber dele,
    /// se pode se mexer e o momento em que é pego (<see cref="OnCaught"/>, uma vez só por fase).
    /// </summary>
    public class PlayerState : MonoBehaviour, IDetectable
    {
        [Tooltip("Metros acima dos pés: ponto mirado pela visão (meio do corpo)")]
        [SerializeField, Min(0f)] private float _detectionHeight = 0.3f;

        public event Action OnCaught;

        public Vector3 DetectionPoint => transform.position + Vector3.up * _detectionHeight;

        // TODO(passo 3.2): falso também no modo cogumelo.
        public bool IsVisible => !IsHidden;

        // TODO(passo 3.2): verdadeiro no modo cogumelo.
        public bool IsUntouchable => false;

        public bool IsCaught { get; private set; }

        /// <summary>Dentro de uma lixeira (GDD §9).</summary>
        public bool IsHidden { get; private set; }

        /// <summary>Entrando ou saindo de um esconderijo.</summary>
        public bool IsInTransition { get; private set; }

        public bool CanMove => !IsCaught && !IsHidden && !IsInTransition;

        private void OnEnable() => DetectableRegistry.Register(this);

        private void OnDisable() => DetectableRegistry.Unregister(this);

        public void Catch()
        {
            if (IsCaught || IsUntouchable)
            {
                return; // ser pego duas vezes no mesmo frame não pode reiniciar duas vezes
            }

            IsCaught = true;
            OnCaught?.Invoke();
        }

        public void SetHidden(bool isHidden) => IsHidden = isHidden;

        public void SetInTransition(bool isInTransition) => IsInTransition = isInTransition;
    }
}
