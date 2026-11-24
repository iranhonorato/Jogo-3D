using Guaxinim.Core;
using Guaxinim.Hazards;
using Guaxinim.Items;
using Guaxinim.Player;
using Guaxinim.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace Guaxinim.UI
{
    /// <summary>
    /// Dicas do tutorial (GDD §11 e §13): faixas curtas no topo da tela, cada uma mostrada uma vez, na ordem em que
    /// o jogador precisa delas. Os textos e os gatilhos vêm do <see cref="TutorialTipsData"/> do <c>LevelData</c>;
    /// uma fase sem esse asset não mostra dicas.
    /// </summary>
    public class TutorialTipsUI : MonoBehaviour
    {
        [SerializeField] private LevelFlow _levelFlow;
        [SerializeField] private RaccoonInteractor _interactor;
        [SerializeField] private Inventory _inventory;

        [Tooltip("A faixa inteira (fundo e texto); começa escondida")]
        [SerializeField] private GameObject _banner;

        [SerializeField] private Text _text;

        private TutorialTipQueue _queue;
        private float _displaySeconds;
        private float _remainingSeconds;

        private void Awake()
        {
            if (_levelFlow == null || _interactor == null || _inventory == null || _banner == null || _text == null)
            {
                Debug.LogError("TutorialTipsUI is missing references.", this);
                enabled = false;
                return;
            }

            _banner.SetActive(false);
        }

        private void OnEnable()
        {
            if (_interactor != null && _inventory != null)
            {
                _interactor.OnTargetChanged += HandleTargetChanged;
                _inventory.OnChanged += HandleInventoryChanged;
            }
        }

        private void Start()
        {
            TutorialTipsData tips = _levelFlow.Level.Tips;
            if (tips == null)
            {
                enabled = false; // fase sem tutorial
                return;
            }

            _queue = new TutorialTipQueue(tips.Tips);
            _displaySeconds = tips.DisplaySeconds;
        }

        private void Update()
        {
            if (_levelFlow.IsLevelOver)
            {
                _banner.SetActive(false);
                return;
            }

            _queue.NotifyElapsedSeconds(_levelFlow.ElapsedSeconds);
            if (_remainingSeconds > 0f)
            {
                _remainingSeconds -= Time.deltaTime;
                if (_remainingSeconds <= 0f)
                {
                    _banner.SetActive(false);
                }

                return;
            }

            if (_queue.TryDequeue(out TutorialTip tip))
            {
                _text.text = tip.Text;
                _banner.SetActive(true);
                _remainingSeconds = _displaySeconds;
            }
        }

        private void OnDisable()
        {
            if (_interactor != null && _inventory != null)
            {
                _interactor.OnTargetChanged -= HandleTargetChanged;
                _inventory.OnChanged -= HandleInventoryChanged;
            }
        }

        private void HandleTargetChanged(IInteractable target)
        {
            if (target is PickupItem)
            {
                _queue.NotifyTrigger(TipTrigger.NearPickup);
            }
            else if (target is HidingSpot)
            {
                _queue.NotifyTrigger(TipTrigger.NearHidingSpot);
            }
        }

        private void HandleInventoryChanged()
        {
            if (_inventory.Items.Count > 0)
            {
                _queue.NotifyTrigger(TipTrigger.FirstPickup);
            }
        }
    }
}
