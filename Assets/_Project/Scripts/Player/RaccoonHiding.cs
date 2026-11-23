using System.Collections;
using Guaxinim.Shared;
using UnityEngine;

namespace Guaxinim.Player
{
    /// <summary>
    /// Entrar e sair da lixeira (GDD §9): 0,5 s indo até o esconderijo, e o modelo some; ao sair, o modelo
    /// volta e o guaxinim vai até o ponto de saída. Durante a transição, ele não se mexe.
    /// </summary>
    [RequireComponent(typeof(PlayerState), typeof(CharacterController), typeof(RaccoonBulkyLoad))]
    public class RaccoonHiding : MonoBehaviour, IHider
    {
        [SerializeField] private PlayerData _data;

        [Tooltip("Visual do guaxinim, escondido dentro da lixeira")]
        [SerializeField] private GameObject _model;

        private PlayerState _state;
        private CharacterController _controller;
        private RaccoonBulkyLoad _bulkyLoad;

        public bool CanHide => !_state.IsCaught && !_state.IsHidden && !_state.IsInTransition && !_bulkyLoad.IsBulky;
        public bool IsHidden => _state.IsHidden;
        public bool IsInTransition => _state.IsInTransition;

        private void Awake()
        {
            _state = GetComponent<PlayerState>();
            _controller = GetComponent<CharacterController>();
            _bulkyLoad = GetComponent<RaccoonBulkyLoad>();
            if (_data == null || _model == null)
            {
                Debug.LogError("RaccoonHiding needs Data and Model assigned.", this);
                enabled = false;
            }
        }

        public void EnterHiding(Vector3 spotPosition) => StartCoroutine(EnterRoutine(spotPosition));

        public void ExitHiding(Vector3 exitPosition) => StartCoroutine(ExitRoutine(exitPosition));

        private IEnumerator EnterRoutine(Vector3 spotPosition)
        {
            _state.SetInTransition(true);
            yield return MoveTo(spotPosition);
            _model.SetActive(false);
            _state.SetHidden(true);
            _state.SetInTransition(false);
        }

        private IEnumerator ExitRoutine(Vector3 exitPosition)
        {
            _state.SetInTransition(true);
            _model.SetActive(true);
            yield return MoveTo(exitPosition);
            _state.SetHidden(false);
            _state.SetInTransition(false);
        }

        /// <summary>Desliza até o destino em 0,5 s, com a colisão desligada (atravessa a borda da lixeira).</summary>
        private IEnumerator MoveTo(Vector3 destination)
        {
            _controller.enabled = false;
            Vector3 start = transform.position;
            float elapsed = 0f;
            float duration = _data.HideTransitionSeconds;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(start, destination, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            transform.position = destination;
            _controller.enabled = true;
        }
    }
}
