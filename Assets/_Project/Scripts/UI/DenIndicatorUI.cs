using Guaxinim.Core;
using Guaxinim.Player;
using UnityEngine;

namespace Guaxinim.UI
{
    /// <summary>
    /// Mostra onde fica a toca enquanto o guaxinim carrega algo: com a toca na tela, uma seta apontando para baixo
    /// sobre ela; fora da tela, a seta fica na borda, apontando na direção dela. Nas fases grandes, a câmera mostra
    /// só um pedaço do mercado.
    /// </summary>
    [DefaultExecutionOrder(AfterCameraExecutionOrder)]
    public class DenIndicatorUI : MonoBehaviour
    {
        // Como o balão de interação: lê a câmera depois que o Cinemachine a moveu, para a seta não tremer.
        private const int AfterCameraExecutionOrder = 200;
        private const float PointingDownDegrees = -90f;

        [SerializeField] private Inventory _inventory;
        [SerializeField] private LevelFlow _levelFlow;

        [Tooltip("Câmera do jogo, para achar onde a toca aparece na tela")]
        [SerializeField] private Camera _camera;

        [SerializeField] private Transform _den;

        [Tooltip("O indicador inteiro (seta e rótulo); começa escondido")]
        [SerializeField] private RectTransform _indicator;

        [Tooltip("Só a seta, que gira; desenhada apontando para a direita")]
        [SerializeField] private RectTransform _arrow;

        [Tooltip("Pixels entre a seta e a borda da tela")]
        [SerializeField, Min(0f)] private float _edgeMarginPixels = 90f;

        [Tooltip("Metros acima da toca onde a seta aparece quando ela está na tela")]
        [SerializeField, Min(0f)] private float _heightOffsetMeters = 1.4f;

        private void Awake()
        {
            if (_inventory == null || _levelFlow == null || _camera == null || _den == null || _indicator == null ||
                _arrow == null)
            {
                Debug.LogError("DenIndicatorUI is missing references.", this);
                enabled = false;
                return;
            }

            _indicator.gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            if (_levelFlow.IsLevelOver || _inventory.Items.Count == 0)
            {
                SetVisible(false);
                return;
            }

            var screenSize = new Vector2(Screen.width, Screen.height);
            Vector3 denPoint = _camera.WorldToScreenPoint(_den.position + Vector3.up * _heightOffsetMeters);
            if (EdgeArrow.IsOnScreen(denPoint, screenSize, _edgeMarginPixels))
            {
                Place(denPoint, PointingDownDegrees);
            }
            else
            {
                Vector2 edgePoint = EdgeArrow.ClampToEdge(denPoint, screenSize, _edgeMarginPixels);
                Place(edgePoint, EdgeArrow.AngleDegrees(screenSize * 0.5f, edgePoint));
            }

            SetVisible(true);
        }

        private void Place(Vector2 screenPosition, float angleDegrees)
        {
            _indicator.position = new Vector3(screenPosition.x, screenPosition.y, 0f);
            _arrow.localEulerAngles = new Vector3(0f, 0f, angleDegrees);
        }

        private void SetVisible(bool isVisible)
        {
            if (_indicator.gameObject.activeSelf != isVisible)
            {
                _indicator.gameObject.SetActive(isVisible);
            }
        }
    }
}
