using System.Collections;
using UnityEngine;

namespace Guaxinim.Hazards
{
    /// <summary>
    /// Resposta visual da lixeira (plano de próximos passos, Etapa 5.4): ao entrar ou sair, a tampa abre e fecha
    /// (gira na dobradiça) e a lixeira balança. Só visual; a regra de esconder fica no <see cref="HidingSpot"/>.
    /// </summary>
    [RequireComponent(typeof(HidingSpot))]
    public class BinLidAnimator : MonoBehaviour
    {
        [Tooltip("Tampa, com o pivô na dobradiça (fundo da lixeira)")]
        [SerializeField] private Transform _lid;

        [Tooltip("Parte visual que balança (o collider fica parado)")]
        [SerializeField] private Transform _visual;

        [Tooltip("Graus que a tampa abre")]
        [SerializeField, Range(0f, 180f)] private float _openDegrees = 110f;

        [Tooltip("Segundos com a tampa aberta (a transição do guaxinim dura 0,5 s)")]
        [SerializeField, Min(0f)] private float _openSeconds = 0.5f;

        [Tooltip("Segundos para abrir e para fechar")]
        [SerializeField, Min(0.01f)] private float _swingSeconds = 0.12f;

        [Tooltip("Graus do balanço da lixeira ao fechar")]
        [SerializeField, Min(0f)] private float _wobbleDegrees = 6f;

        [Tooltip("Segundos do balanço")]
        [SerializeField, Min(0.01f)] private float _wobbleSeconds = 0.35f;

        private HidingSpot _spot;
        private Quaternion _closedLidRotation;

        private void Awake()
        {
            _spot = GetComponent<HidingSpot>();
            if (_lid == null || _visual == null)
            {
                Debug.LogError("BinLidAnimator needs Lid and Visual assigned.", this);
                enabled = false;
                return;
            }

            _closedLidRotation = _lid.localRotation;
        }

        private void OnEnable() => _spot.OnUsed += HandleUsed;

        private void OnDisable() => _spot.OnUsed -= HandleUsed;

        private void HandleUsed()
        {
            StopAllCoroutines();
            StartCoroutine(OpenThenClose());
        }

        private IEnumerator OpenThenClose()
        {
            yield return SwingLid(0f, _openDegrees);
            yield return new WaitForSeconds(_openSeconds);
            yield return SwingLid(_openDegrees, 0f);
            yield return Wobble();
        }

        private IEnumerator SwingLid(float fromDegrees, float toDegrees)
        {
            for (float elapsed = 0f; elapsed < _swingSeconds; elapsed += Time.deltaTime)
            {
                SetLidAngle(Mathf.Lerp(fromDegrees, toDegrees, elapsed / _swingSeconds));
                yield return null;
            }

            SetLidAngle(toDegrees);
        }

        private IEnumerator Wobble()
        {
            for (float elapsed = 0f; elapsed < _wobbleSeconds; elapsed += Time.deltaTime)
            {
                float fade = 1f - elapsed / _wobbleSeconds;
                float angle = Mathf.Sin(elapsed / _wobbleSeconds * 3f * Mathf.PI) * _wobbleDegrees * fade;
                _visual.localRotation = Quaternion.Euler(0f, 0f, angle);
                yield return null;
            }

            _visual.localRotation = Quaternion.identity;
        }

        // Abre para trás: a borda da frente sobe girando em X negativo na dobradiça.
        private void SetLidAngle(float degrees) => _lid.localRotation = _closedLidRotation * Quaternion.Euler(-degrees, 0f, 0f);
    }
}
