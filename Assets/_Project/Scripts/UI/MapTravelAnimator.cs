using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

namespace Guaxinim.UI
{
    /// <summary>
    /// Viagem do guaxinim no mapa (plano de próximos passos, Etapa 4): o marcador sai da parada do mercado vencido,
    /// percorre a estrada em pulinhos acendendo os pontos, e o próximo mercado dá um salto de escala ao ser liberado.
    /// Qualquer tecla, botão ou clique pula para o fim, para ninguém ficar preso esperando (rubrica 4). Usa o tempo
    /// real: o mapa nunca fica com o jogo congelado, mas a viagem não depende disso.
    /// </summary>
    public class MapTravelAnimator : MonoBehaviour
    {
        [Tooltip("Marcador do guaxinim, irmão das paradas e dos pontos")]
        [SerializeField] private RectTransform _marker;

        [Tooltip("Uma parada por fase do catálogo, na mesma ordem, embaixo de cada fachada")]
        [SerializeField] private RectTransform[] _stops = new RectTransform[0];

        [Tooltip("Os pontos da estrada, trecho por trecho: os do trecho 0 → 1 primeiro, depois os do 1 → 2...")]
        [SerializeField] private Image[] _roadDots = new Image[0];

        [SerializeField, Min(1)] private int _dotsPerSegment = 5;
        [SerializeField] private Color _idleDotColor = new Color(1f, 1f, 1f, 0.35f);
        [SerializeField] private Color _litDotColor = new Color32(0xFF, 0xC8, 0x4D, 0xFF);

        [Header("Tempo (segundos, tempo real)")]
        [SerializeField, Min(0f)] private float _holdSeconds = 0.5f;
        [SerializeField, Min(0.1f)] private float _travelSeconds = 1.8f;
        [SerializeField, Min(0.05f)] private float _popSeconds = 0.4f;

        [Header("Movimento")]
        [Tooltip("Pixels de altura de cada pulinho")]
        [SerializeField, Min(0f)] private float _hopHeightPixels = 26f;

        [SerializeField, Min(1)] private int _hopCount = 5;

        [Tooltip("Escala máxima do salto da fachada liberada")]
        [SerializeField, Min(1f)] private float _popScale = 1.15f;

        private Coroutine _travel;
        private IDisposable _skipListener;
        private int _toStop;
        private Transform _popTarget;
        private bool _hasArrived;

        /// <summary>O marcador chegou ao próximo mercado: hora de mostrá-lo liberado.</summary>
        public event Action OnArrived;

        /// <summary>A viagem acabou (ou foi pulada): hora de abrir o início da fase.</summary>
        public event Action OnFinished;

        public bool IsTraveling => _travel != null;

        private void Awake()
        {
            if (_marker == null || _stops.Length == 0)
            {
                Debug.LogError("MapTravelAnimator needs the Marker and the Stops assigned.", this);
                enabled = false;
            }
        }

        private void OnDisable() => StopListeningForSkip();

        /// <summary>Sem viagem: o marcador fica na parada da fase e a estrada até ela aparece acesa.</summary>
        public void PlaceAt(int stopIndex)
        {
            int stop = Mathf.Clamp(stopIndex, 0, _stops.Length - 1);
            _marker.localPosition = _stops[stop].localPosition;
            LightDotsUpTo(stop * _dotsPerSegment);
        }

        /// <summary>Toca a viagem da parada <paramref name="fromStop"/> até <paramref name="toStop"/>.</summary>
        public void Play(int fromStop, int toStop, Transform popTarget)
        {
            if (!enabled || IsTraveling)
            {
                return;
            }

            _toStop = Mathf.Clamp(toStop, 0, _stops.Length - 1);
            _popTarget = popTarget;
            _hasArrived = false;
            PlaceAt(fromStop);
            _skipListener = InputSystem.onAnyButtonPress.CallOnce(_ => Skip());
            _travel = StartCoroutine(Travel(Mathf.Clamp(fromStop, 0, _stops.Length - 1)));
        }

        private IEnumerator Travel(int fromStop)
        {
            yield return new WaitForSecondsRealtime(_holdSeconds);

            int firstDot = fromStop * _dotsPerSegment;
            int dotCount = (_toStop - fromStop) * _dotsPerSegment;
            for (float elapsed = 0f; elapsed < _travelSeconds; elapsed += Time.unscaledDeltaTime)
            {
                MoveMarker(fromStop, elapsed / _travelSeconds);
                LightDotsUpTo(firstDot + RoadPath.LitDotCount(dotCount, elapsed / _travelSeconds));
                yield return null;
            }

            Arrive();
            for (float elapsed = 0f; elapsed < _popSeconds; elapsed += Time.unscaledDeltaTime)
            {
                float pop = Mathf.Sin(elapsed / _popSeconds * Mathf.PI); // sobe e volta
                SetPopScale(Mathf.Lerp(1f, _popScale, pop));
                yield return null;
            }

            Finish();
        }

        /// <summary>Pula para o fim. O início da fase só abre no quadro seguinte, para a tecla que pulou não o confirmar.</summary>
        private void Skip()
        {
            if (!IsTraveling)
            {
                return;
            }

            StopCoroutine(_travel);
            Arrive();
            _travel = StartCoroutine(FinishNextFrame());
        }

        private IEnumerator FinishNextFrame()
        {
            yield return null;
            Finish();
        }

        private void Arrive()
        {
            PlaceAt(_toStop);
            if (_hasArrived)
            {
                return;
            }

            _hasArrived = true;
            OnArrived?.Invoke();
        }

        private void Finish()
        {
            SetPopScale(1f);
            StopListeningForSkip();
            _travel = null;
            OnFinished?.Invoke();
        }

        private void MoveMarker(int fromStop, float progress)
        {
            Vector2 start = _stops[fromStop].localPosition;
            Vector2 end = _stops[_toStop].localPosition;
            _marker.localPosition = RoadPath.Evaluate(start, end, progress, _hopHeightPixels, _hopCount);
        }

        private void LightDotsUpTo(int litCount)
        {
            for (int i = 0; i < _roadDots.Length; i++)
            {
                _roadDots[i].color = i < litCount ? _litDotColor : _idleDotColor;
            }
        }

        private void SetPopScale(float scale)
        {
            if (_popTarget != null)
            {
                _popTarget.localScale = Vector3.one * scale;
            }
        }

        private void StopListeningForSkip()
        {
            _skipListener?.Dispose(); // IDisposable comum: pode usar ?.
            _skipListener = null;
        }
    }
}
