using System;
using UnityEngine;

namespace Guaxinim.Noise
{
    /// <summary>
    /// Anel no chão que mostra um barulho (GDD §5): abre até o raio e some. Vem de um pool:
    /// <see cref="Play"/> começa a animação e <see cref="OnFinished"/> avisa quando pode voltar ao pool.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class NoiseRingVFX : MonoBehaviour
    {
        private const int Segments = 48;
        private const float FloorOffset = 0.03f; // evita brigar com o piso pela profundidade

        [Tooltip("Segundos até o anel sumir")]
        [SerializeField, Min(0.05f)] private float _durationSeconds = 0.8f;

        [SerializeField] private Color _color = new Color(1f, 0.92f, 0.6f);

        private LineRenderer _line;
        private float _elapsedSeconds;
        private float _maxRadius;

        public event Action<NoiseRingVFX> OnFinished;

        private void Awake()
        {
            _line = GetComponent<LineRenderer>();
            BuildUnitCircle();
        }

        private void Update()
        {
            _elapsedSeconds += Time.deltaTime;
            float progress = NoiseRingAnimation.GetProgress(_elapsedSeconds, _durationSeconds);
            ApplyFrame(progress);

            if (progress >= 1f)
            {
                OnFinished?.Invoke(this);
            }
        }

        public void Play(NoiseEvent noise)
        {
            transform.position = noise.Position + Vector3.up * FloorOffset;
            _maxRadius = noise.Radius;
            _elapsedSeconds = 0f;
            ApplyFrame(0f);
        }

        private void ApplyFrame(float progress)
        {
            float radius = Mathf.Max(NoiseRingAnimation.GetRadius(progress, _maxRadius), 0.001f);
            transform.localScale = new Vector3(radius, 1f, radius);

            Color color = _color;
            color.a = NoiseRingAnimation.GetAlpha(progress);
            _line.startColor = color;
            _line.endColor = color;
        }

        private void BuildUnitCircle()
        {
            _line.useWorldSpace = false; // a escala do objeto define o raio; a largura da linha não muda
            _line.loop = true;
            _line.positionCount = Segments;
            for (int i = 0; i < Segments; i++)
            {
                float angle = 2f * Mathf.PI * i / Segments;
                _line.SetPosition(i, new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)));
            }
        }
    }
}
