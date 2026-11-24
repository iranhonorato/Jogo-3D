using System;
using Guaxinim.Core;
using Guaxinim.Noise;
using UnityEngine;

namespace Guaxinim.Player
{
    /// <summary>
    /// Barulhos do guaxinim (GDD §5): correr emite som em intervalos e o assobio (F) emite um som
    /// de 8 m. Andar é silencioso.
    /// </summary>
    public class RaccoonNoiseMaker : MonoBehaviour
    {
        [SerializeField] private NoiseData _noiseData;
        [SerializeField] private InputReader _input;
        [SerializeField] private RaccoonController _controller;
        [SerializeField] private Inventory _inventory;

        private float _secondsUntilNextRunNoise;

        /// <summary>Um passo de corrida fez barulho (o áudio toca a pisada).</summary>
        public event Action OnRunStep;

        public event Action OnWhistled;

        private void Awake()
        {
            if (_noiseData == null || _input == null || _controller == null || _inventory == null)
            {
                Debug.LogError("RaccoonNoiseMaker needs Noise Data, Input, Controller and Inventory assigned.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (_input != null)
            {
                _input.OnWhistlePressed += HandleWhistlePressed;
            }
        }

        private void OnDisable()
        {
            if (_input != null)
            {
                _input.OnWhistlePressed -= HandleWhistlePressed;
            }
        }

        private void Update()
        {
            if (!_controller.IsRunning)
            {
                _secondsUntilNextRunNoise = 0f; // o primeiro passo de corrida já faz barulho
                return;
            }

            _secondsUntilNextRunNoise -= Time.deltaTime;
            if (_secondsUntilNextRunNoise > 0f)
            {
                return;
            }

            _secondsUntilNextRunNoise = _noiseData.RunEmitIntervalSeconds;
            float radius = NoiseRadius.ForRunning(_noiseData.RunRadius, _noiseData.RunCarryBonusRadius,
                _inventory.CurrentWeight, _inventory.MaxWeight);
            NoiseSystem.Emit(transform.position, radius);
            OnRunStep?.Invoke();
        }

        private void HandleWhistlePressed()
        {
            NoiseSystem.Emit(transform.position, _noiseData.WhistleRadius);
            OnWhistled?.Invoke();
        }
    }
}
