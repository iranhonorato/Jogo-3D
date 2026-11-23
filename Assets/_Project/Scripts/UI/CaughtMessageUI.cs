using Guaxinim.Core;
using UnityEngine;

namespace Guaxinim.UI
{
    /// <summary>Mostra o aviso "Você foi pego!" (GDD §13) quando o <see cref="LevelFlow"/> avisa.</summary>
    public class CaughtMessageUI : MonoBehaviour
    {
        [SerializeField] private LevelFlow _levelFlow;

        [Tooltip("Objeto com o texto do aviso; começa escondido")]
        [SerializeField] private GameObject _message;

        private void Awake()
        {
            if (_levelFlow == null || _message == null)
            {
                Debug.LogError("CaughtMessageUI needs Level Flow and Message assigned.", this);
                enabled = false;
                return;
            }

            _message.SetActive(false);
        }

        private void OnEnable()
        {
            if (_levelFlow != null)
            {
                _levelFlow.OnPlayerCaught += HandlePlayerCaught;
            }
        }

        private void OnDisable()
        {
            if (_levelFlow != null)
            {
                _levelFlow.OnPlayerCaught -= HandlePlayerCaught;
            }
        }

        private void HandlePlayerCaught() => _message.SetActive(true);
    }
}
