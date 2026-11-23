using Guaxinim.Shared;
using UnityEngine;

namespace Guaxinim.Player
{
    /// <summary>
    /// Estado do guaxinim visto de fora (GDD §14.3): o que os seguranças e as câmeras podem saber dele.
    /// </summary>
    public class PlayerState : MonoBehaviour, IDetectable
    {
        [Tooltip("Metros acima dos pés: ponto mirado pela visão (meio do corpo)")]
        [SerializeField, Min(0f)] private float _detectionHeight = 0.3f;

        public Vector3 DetectionPoint => transform.position + Vector3.up * _detectionHeight;

        // TODO(passo 2.8): falso dentro da lixeira. TODO(passo 3.2): falso no modo cogumelo.
        public bool IsVisible => true;

        // TODO(passo 3.2): verdadeiro no modo cogumelo.
        public bool IsUntouchable => false;

        private void OnEnable() => DetectableRegistry.Register(this);

        private void OnDisable() => DetectableRegistry.Unregister(this);
    }
}
