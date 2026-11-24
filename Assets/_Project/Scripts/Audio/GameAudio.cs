using Guaxinim.UI;
using UnityEngine;

namespace Guaxinim.Audio
{
    /// <summary>
    /// Música de fundo em loop e clique de navegação dos menus (GDD §15.6). É um serviço único e global que atravessa
    /// as cenas (a música não recomeça a cada troca), mas não guarda nada da fase: vive num prefab de
    /// <c>Resources</c>, criado antes da primeira cena, como o ScreenFader. Sem o prefab, o jogo fica sem música.
    /// </summary>
    public class GameAudio : MonoBehaviour
    {
        private const string ResourcePath = "GameAudio";

        [SerializeField] private AudioSource _music;
        [SerializeField] private AudioSource _interface;
        [SerializeField] private SoundCue _select;

        private static GameAudio _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            if (_music == null || _interface == null)
            {
                Debug.LogError("GameAudio needs the Music and Interface audio sources assigned.", this);
                enabled = false;
            }
        }

        private void OnEnable() => SelectableHighlight.OnAnySelected += HandleAnySelected;

        private void Start()
        {
            if (_music != null && _music.clip != null && !_music.isPlaying)
            {
                _music.Play();
            }
        }

        private void OnDisable() => SelectableHighlight.OnAnySelected -= HandleAnySelected;

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
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

        private void HandleAnySelected() => _select.PlayOn(_interface);
    }
}
