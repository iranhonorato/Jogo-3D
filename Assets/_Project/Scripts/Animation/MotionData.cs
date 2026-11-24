using UnityEngine;

namespace Guaxinim.Animation
{
    /// <summary>Números da animação procedural de um personagem (guaxinim, segurança, faxineiro). Somente leitura.</summary>
    [CreateAssetMenu(fileName = "MotionData", menuName = "Guaxinim/Motion Data")]
    public class MotionData : ScriptableObject
    {
        [Header("Passo")]
        [Tooltip("Metros andados por passo")]
        [SerializeField, Min(0.05f)] private float _stepLength = 0.45f;

        [Tooltip("Metros que o corpo sobe no meio de cada passo")]
        [SerializeField, Min(0f)] private float _bounceHeight = 0.05f;

        [Tooltip("Graus de balanço para cada lado a cada passo")]
        [SerializeField, Min(0f)] private float _swayDegrees = 6f;

        [Tooltip("Metros por segundo a partir dos quais o passo está completo (abaixo disso, vai sumindo)")]
        [SerializeField, Min(0.01f)] private float _fullStepSpeed = 1f;

        [Header("Inclinação")]
        [Tooltip("Graus de inclinação para a frente na velocidade de referência")]
        [SerializeField, Range(0f, 45f)] private float _leanDegrees = 12f;

        [Tooltip("Metros por segundo em que a inclinação chega ao máximo (a corrida)")]
        [SerializeField, Min(0.1f)] private float _leanReferenceSpeed = 6f;

        [Header("Parado")]
        [Tooltip("Fração da escala que a respiração muda")]
        [SerializeField, Range(0f, 0.2f)] private float _breathAmplitude = 0.02f;

        [Tooltip("Segundos de uma respiração")]
        [SerializeField, Min(0.1f)] private float _breathPeriodSeconds = 2.5f;

        [Header("Achatar e pular")]
        [Tooltip("Fração da altura que achata ao arrancar, frear ou pegar algo")]
        [SerializeField, Range(0f, 0.5f)] private float _squashAmount = 0.12f;

        [Tooltip("Achatamento por (m/s²) de mudança de velocidade")]
        [SerializeField, Min(0f)] private float _squashPerAcceleration = 0.01f;

        [Tooltip("Quanto do achatamento some por segundo")]
        [SerializeField, Min(0.1f)] private float _squashRecoveryPerSecond = 6f;

        [Tooltip("Metros do pulinho de susto")]
        [SerializeField, Min(0f)] private float _hopHeight = 0.25f;

        [Tooltip("Segundos do pulinho de susto")]
        [SerializeField, Min(0.05f)] private float _hopSeconds = 0.3f;

        [Tooltip("Quão rápido a velocidade lida acompanha a real (maior = mais rápido)")]
        [SerializeField, Min(0.1f)] private float _speedSmoothing = 10f;

        public float StepLength => _stepLength;
        public float BounceHeight => _bounceHeight;
        public float SwayDegrees => _swayDegrees;
        public float FullStepSpeed => _fullStepSpeed;
        public float LeanDegrees => _leanDegrees;
        public float LeanReferenceSpeed => _leanReferenceSpeed;
        public float BreathAmplitude => _breathAmplitude;
        public float BreathPeriodSeconds => _breathPeriodSeconds;
        public float SquashAmount => _squashAmount;
        public float SquashPerAcceleration => _squashPerAcceleration;
        public float SquashRecoveryPerSecond => _squashRecoveryPerSecond;
        public float HopHeight => _hopHeight;
        public float HopSeconds => _hopSeconds;
        public float SpeedSmoothing => _speedSmoothing;
    }
}
