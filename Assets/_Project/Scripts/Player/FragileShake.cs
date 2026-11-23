using System;
using Guaxinim.Noise;
using Guaxinim.Shared;
using UnityEngine;

namespace Guaxinim.Player
{
    /// <summary>
    /// Barra de chacoalhar (GDD §7.5): com um item frágil na carga, enche enquanto o guaxinim corre e
    /// esvazia andando. Cheia, o último frágil quebra: some, faz um barulho de 10 m e deixa uma poça.
    /// </summary>
    [RequireComponent(typeof(Inventory), typeof(RaccoonController))]
    public class FragileShake : MonoBehaviour, IMeterValue
    {
        private const float PuddleFloorOffset = 0.005f;

        [SerializeField] private PlayerData _data;
        [SerializeField] private NoiseData _noiseData;

        [Tooltip("Poça deixada no chão quando um frágil quebra")]
        [SerializeField] private GameObject _puddlePrefab;

        private Inventory _inventory;
        private RaccoonController _controller;
        private ShakeMeter _meter;

        public event Action OnItemBroken;

        public float Value => _meter != null ? _meter.Value : 0f;

        private void Awake()
        {
            _inventory = GetComponent<Inventory>();
            _controller = GetComponent<RaccoonController>();
            if (_data == null || _noiseData == null || _puddlePrefab == null)
            {
                Debug.LogError("FragileShake needs Data, Noise Data and Puddle Prefab assigned.", this);
                enabled = false;
                return;
            }

            _meter = new ShakeMeter(_data.ShakeFillSeconds, _data.ShakeDrainPerSecond);
        }

        private void Update()
        {
            if (!_inventory.HasFragile)
            {
                _meter.Reset();
                return;
            }

            if (_meter.Tick(_controller.IsRunning, Time.deltaTime))
            {
                Break();
            }
        }

        private void Break()
        {
            if (!_inventory.TryBreakLastFragile())
            {
                return;
            }

            Vector3 position = transform.position;
            NoiseSystem.Emit(position, _noiseData.FragileBreakRadius);
            // Poucas por fase e ficam até o fim: Instantiate simples, sem pool.
            Instantiate(_puddlePrefab, position + Vector3.up * PuddleFloorOffset, Quaternion.identity);
            OnItemBroken?.Invoke();
        }
    }
}
