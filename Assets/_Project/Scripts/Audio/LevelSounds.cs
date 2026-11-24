using Guaxinim.Core;
using Guaxinim.Items;
using Guaxinim.Shared;
using UnityEngine;

namespace Guaxinim.Audio
{
    /// <summary>
    /// Sons da fase (GDD §15.6): item entregue na toca, inimigo que começa a perseguir, pego e vitória. Vive no
    /// LevelSystems. O áudio não depende do Time.timeScale, então os sons do fim tocam com o jogo congelado.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class LevelSounds : MonoBehaviour
    {
        [SerializeField] private SoundData _sounds;
        [SerializeField] private LevelFlow _levelFlow;
        [SerializeField] private ShoppingList _shoppingList;

        private AudioSource _source;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            if (_sounds == null || _levelFlow == null || _shoppingList == null)
            {
                Debug.LogError("LevelSounds needs Sound Data, Level Flow and Shopping List assigned.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (_levelFlow == null || _shoppingList == null)
            {
                return;
            }

            _levelFlow.OnPlayerCaught += HandlePlayerCaught;
            _levelFlow.OnLevelWon += HandleLevelWon;
            _shoppingList.OnItemDelivered += HandleItemDelivered;
            ChaseReports.OnChaseStarted += HandleChaseStarted;
        }

        private void OnDisable()
        {
            if (_levelFlow == null || _shoppingList == null)
            {
                return;
            }

            _levelFlow.OnPlayerCaught -= HandlePlayerCaught;
            _levelFlow.OnLevelWon -= HandleLevelWon;
            _shoppingList.OnItemDelivered -= HandleItemDelivered;
            ChaseReports.OnChaseStarted -= HandleChaseStarted;
        }

        private void HandlePlayerCaught() => _sounds.Caught.PlayOn(_source);

        private void HandleLevelWon() => _sounds.Win.PlayOn(_source);

        private void HandleItemDelivered(ItemData item, bool isRequired) => _sounds.Deliver.PlayOn(_source);

        private void HandleChaseStarted() => _sounds.Spotted.PlayOn(_source);
    }
}
