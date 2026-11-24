using Guaxinim.Core;
using Guaxinim.Player;
using Guaxinim.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace Guaxinim.UI
{
    /// <summary>
    /// Indicação de interação (GDD §3.2 e §13): um balão "[E] Pegar", "[E] Esconder" ou "[E] Sair" sobre o objeto
    /// que o E usaria agora (o mais próximo escolhido pelo <see cref="RaccoonInteractor"/>). Some quando não há
    /// nada ao alcance e quando a fase termina.
    /// </summary>
    [DefaultExecutionOrder(AfterCameraExecutionOrder)]
    public class InteractionPromptUI : MonoBehaviour
    {
        // A câmera do Cinemachine se move no LateUpdate; o balão precisa ler a posição já atualizada, senão fica
        // um quadro atrasado e treme em relação ao objeto.
        private const int AfterCameraExecutionOrder = 200;
        private const string KeyHint = "[E] ";

        [SerializeField] private RaccoonInteractor _interactor;
        [SerializeField] private LevelFlow _levelFlow;

        [Tooltip("Câmera do jogo, para achar onde o objeto aparece na tela")]
        [SerializeField] private Camera _camera;

        [Tooltip("O balão inteiro (fundo e texto); começa escondido")]
        [SerializeField] private RectTransform _prompt;

        [SerializeField] private Text _text;

        [Tooltip("Metros acima do ponto de interação onde o balão aparece")]
        [SerializeField, Min(0f)] private float _heightOffsetMeters = 0.6f;

        private string _shownPromptText;

        private void Awake()
        {
            if (_interactor == null || _levelFlow == null || _camera == null || _prompt == null || _text == null)
            {
                Debug.LogError("InteractionPromptUI is missing references.", this);
                enabled = false;
                return;
            }

            _prompt.gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            IInteractable target = _interactor.CurrentTarget;
            if (target == null || _levelFlow.IsLevelOver)
            {
                SetVisible(false);
                return;
            }

            Vector3 screenPoint = _camera.WorldToScreenPoint(target.InteractionPoint + Vector3.up * _heightOffsetMeters);
            if (screenPoint.z <= 0f)
            {
                SetVisible(false); // atrás da câmera
                return;
            }

            string promptText = target.PromptText;
            if (promptText != _shownPromptText)
            {
                _shownPromptText = promptText;
                _text.text = KeyHint + promptText;
            }

            _prompt.position = new Vector3(screenPoint.x, screenPoint.y, 0f);
            SetVisible(true);
        }

        private void SetVisible(bool isVisible)
        {
            if (_prompt.gameObject.activeSelf != isVisible)
            {
                _prompt.gameObject.SetActive(isVisible);
            }
        }
    }
}
