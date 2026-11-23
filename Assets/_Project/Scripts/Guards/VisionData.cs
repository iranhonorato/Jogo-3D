using UnityEngine;

namespace Guaxinim.Guards
{
    /// <summary>
    /// Alcance e abertura de um cone de visão: o do segurança (GDD §6.1, 9 m e 70°) e, depois, o da
    /// câmera de segurança (§6.6, 8 m e 50°). Somente leitura durante o jogo.
    /// </summary>
    [CreateAssetMenu(fileName = "VisionData", menuName = "Guaxinim/Vision Data")]
    public class VisionData : ScriptableObject
    {
        [Tooltip("Metros")]
        [SerializeField, Min(0.1f)] private float _rangeMeters = 9f;

        [Tooltip("Graus de abertura total do cone")]
        [SerializeField, Range(1f, 180f)] private float _angleDegrees = 70f;

        [Tooltip("Metros acima do pivô: de onde sai o olhar (raycast)")]
        [SerializeField, Min(0f)] private float _eyeHeight = 1.5f;

        public float RangeMeters => _rangeMeters;
        public float AngleDegrees => _angleDegrees;
        public float EyeHeight => _eyeHeight;
    }
}
