using System.Collections.Generic;
using UnityEngine;

namespace Guaxinim.Guards
{
    /// <summary>
    /// Limite de luzes em tempo real (GDD §14.6, WebGL): com muitos inimigos, só as lanternas mais perto do guaxinim
    /// ficam acesas. As outras continuam com o cone desenhado no chão, que é o que importa para o jogo (a visão não
    /// depende da luz). Reavalia algumas vezes por segundo, sem alocar.
    /// </summary>
    public class FlashlightBudget : MonoBehaviour
    {
        [Tooltip("O guaxinim (ligado na montagem da fase)")]
        [SerializeField] private Transform _player;

        [Tooltip("Lanternas acesas ao mesmo tempo. Com a lua e 2 luzes da loja, 2 lanternas somam 5 luzes")]
        [SerializeField, Min(0)] private int _maxLitFlashlights = 2;

        [Tooltip("Segundos entre duas reavaliações")]
        [SerializeField, Min(0.05f)] private float _refreshSeconds = 0.25f;

        private readonly List<Light> _flashlights = new List<Light>();
        private float[] _sqrDistances = new float[0];
        private float _secondsUntilRefresh;

        private void Awake()
        {
            if (_player == null)
            {
                Debug.LogError("FlashlightBudget needs the Player assigned.", this);
                enabled = false;
            }
        }

        private void Start()
        {
            // Só no setup: os inimigos da fase não mudam.
            foreach (GuardAI guard in FindObjectsByType<GuardAI>(FindObjectsSortMode.None))
            {
                Light flashlight = guard.GetComponentInChildren<Light>(true);
                if (flashlight != null)
                {
                    _flashlights.Add(flashlight);
                }
            }

            _sqrDistances = new float[_flashlights.Count];
            Refresh();
        }

        private void Update()
        {
            _secondsUntilRefresh -= Time.deltaTime;
            if (_secondsUntilRefresh <= 0f)
            {
                Refresh();
            }
        }

        private void Refresh()
        {
            _secondsUntilRefresh = _refreshSeconds;
            Vector3 playerPosition = _player.position;
            for (int i = 0; i < _flashlights.Count; i++)
            {
                _sqrDistances[i] = (_flashlights[i].transform.position - playerPosition).sqrMagnitude;
            }

            for (int i = 0; i < _flashlights.Count; i++)
            {
                _flashlights[i].enabled = LightBudget.IsLit(i, _sqrDistances, _maxLitFlashlights);
            }
        }
    }
}
