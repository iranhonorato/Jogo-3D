using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Guaxinim.Core
{
    /// <summary>
    /// Fade entre cenas (GDD §13): escurece ao sair e clareia ao entrar, com o tempo real, porque a vitória e a tela
    /// de pego congelam o jogo. Enquanto a tela não termina de aparecer, nem o mouse nem o teclado respondem.
    /// É um serviço único e global, mas não guarda nada da fase: vive num prefab de <c>Resources</c>, criado antes
    /// da primeira cena. Sem o prefab (ainda não gerado), a troca de cena é direta.
    /// </summary>
    public class ScreenFader : MonoBehaviour
    {
        private const string ResourcePath = "ScreenFader";
        private const float SceneLoadTimeoutSeconds = 5f;

        [Tooltip("Segundos para escurecer ao sair e para clarear ao entrar")]
        [SerializeField, Min(0f)] private float _fadeSeconds = 0.3f;

        [Tooltip("Grupo da imagem preta que cobre a tela")]
        [SerializeField] private CanvasGroup _group;

        private static ScreenFader _instance;

        private bool _isBusy;

        /// <summary>Uma troca de cena está em andamento (escurecendo, carregando ou clareando).</summary>
        public static bool IsBusy => _instance != null && _instance._isBusy;

        /// <summary>Escurece a tela, executa <paramref name="onFaded"/> (que carrega a cena) e clareia na cena nova.</summary>
        public static void FadeOutThen(Action onFaded)
        {
            if (_instance == null || !_instance.enabled)
            {
                onFaded();
                return;
            }

            if (!_instance._isBusy)
            {
                _instance.StartCoroutine(_instance.FadeOutLoadFadeIn(onFaded));
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Create()
        {
            var prefab = Resources.Load<GameObject>(ResourcePath);
            if (prefab != null)
            {
                Instantiate(prefab);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => _instance = null;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            if (_group == null)
            {
                Debug.LogError("ScreenFader needs its Canvas Group assigned.", this);
                enabled = false;
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            _group.alpha = 1f; // o jogo abre escuro e clareia: é o primeiro fade-in
            _group.blocksRaycasts = true;
        }

        private IEnumerator Start()
        {
            if (_instance != this)
            {
                yield break;
            }

            _isBusy = true;
            yield return FadeIn();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private IEnumerator FadeOutLoadFadeIn(Action onFaded)
        {
            _isBusy = true;
            _group.blocksRaycasts = true;
            SetNavigationEnabled(false);
            yield return Fade(1f);

            bool isSceneLoaded = false;
            void HandleSceneLoaded(Scene scene, LoadSceneMode mode) => isSceneLoaded = true;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            onFaded();

            float waitedSeconds = 0f;
            while (!isSceneLoaded && waitedSeconds < SceneLoadTimeoutSeconds)
            {
                waitedSeconds += Time.unscaledDeltaTime;
                yield return null;
            }

            SceneManager.sceneLoaded -= HandleSceneLoaded;
            yield return null; // um quadro para a cena nova rodar o Start e montar a tela
            yield return FadeIn();
        }

        private IEnumerator FadeIn()
        {
            SetNavigationEnabled(false);
            yield return Fade(0f);
            _group.blocksRaycasts = false;
            SetNavigationEnabled(true);
            _isBusy = false;
        }

        private IEnumerator Fade(float targetAlpha)
        {
            float startAlpha = _group.alpha;
            for (float elapsed = 0f; elapsed < _fadeSeconds; elapsed += Time.unscaledDeltaTime)
            {
                _group.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / _fadeSeconds);
                yield return null;
            }

            _group.alpha = targetAlpha;
        }

        /// <summary>Sem navegação, Enter e as setas não acionam botões enquanto a tela está escura.</summary>
        private static void SetNavigationEnabled(bool isEnabled)
        {
            if (EventSystem.current != null)
            {
                EventSystem.current.sendNavigationEvents = isEnabled;
            }
        }
    }
}
