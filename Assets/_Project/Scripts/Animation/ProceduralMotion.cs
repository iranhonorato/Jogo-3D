using UnityEngine;

namespace Guaxinim.Animation
{
    /// <summary>
    /// Animação procedural de um personagem sem esqueleto (GDD §15.4): quique e balanço a cada passo, inclinação
    /// com a velocidade, achatar ao arrancar e frear, respiração parado e o pulinho de susto. Lê a velocidade pela
    /// mudança de posição, então serve ao guaxinim e aos inimigos. Mexe só no modelo-filho: é só visual.
    /// </summary>
    public class ProceduralMotion : MonoBehaviour
    {
        [SerializeField] private MotionData _data;

        [Tooltip("O modelo (filho) que balança; o objeto raiz e a colisão não mudam")]
        [SerializeField] private Transform _model;

        private readonly StepCycle _stepCycle = new StepCycle();
        private Vector3 _baseLocalPosition;
        private Quaternion _baseLocalRotation;
        private Vector3 _baseLocalScale;
        private Vector3 _lastPosition;
        private float _speed;
        private float _squash;
        private float _hopElapsed = -1f;

        private void Awake()
        {
            if (_data == null || _model == null)
            {
                Debug.LogError("ProceduralMotion needs Data and Model assigned.", this);
                enabled = false;
                return;
            }

            _baseLocalPosition = _model.localPosition;
            _baseLocalRotation = _model.localRotation;
            _baseLocalScale = _model.localScale;
        }

        private void OnEnable() => _lastPosition = transform.position;

        private void LateUpdate()
        {
            float deltaTime = Time.deltaTime;
            if (deltaTime <= 0f)
            {
                return; // pausado
            }

            float distance = MeasureDistanceMoved();
            UpdateSpeedAndSquash(distance / deltaTime, deltaTime);
            _stepCycle.Advance(distance, _data.StepLength);
            ApplyPose(deltaTime);
        }

        private void OnDisable()
        {
            _model.localPosition = _baseLocalPosition;
            _model.localRotation = _baseLocalRotation;
            _model.localScale = _baseLocalScale;
        }

        /// <summary>Pulinho de susto (o inimigo viu o guaxinim).</summary>
        public void Hop() => _hopElapsed = 0f;

        /// <summary>Achata o corpo por um instante (pegou um item).</summary>
        public void Squash() => _squash = Mathf.Max(_squash, _data.SquashAmount);

        private float MeasureDistanceMoved()
        {
            Vector3 delta = transform.position - _lastPosition;
            _lastPosition = transform.position;
            delta.y = 0f;
            return delta.magnitude;
        }

        private void UpdateSpeedAndSquash(float measuredSpeed, float deltaTime)
        {
            float previousSpeed = _speed;
            float follow = 1f - Mathf.Exp(-_data.SpeedSmoothing * deltaTime);
            _speed = Mathf.Lerp(_speed, measuredSpeed, follow);

            float acceleration = Mathf.Abs(_speed - previousSpeed) / deltaTime;
            float fromAcceleration = Mathf.Min(acceleration * _data.SquashPerAcceleration, _data.SquashAmount);
            _squash = Mathf.Max(_squash, fromAcceleration);
            _squash = Mathf.MoveTowards(_squash, 0f, _data.SquashRecoveryPerSecond * _data.SquashAmount * deltaTime);
        }

        private void ApplyPose(float deltaTime)
        {
            float moving = MotionMath.MovingIntensity(_speed, _data.FullStepSpeed);
            float phase = _stepCycle.Phase;
            float height = MotionMath.Bounce(phase, _data.BounceHeight, moving) + NextHopHeight(deltaTime);
            float sway = MotionMath.Sway(phase, _data.SwayDegrees, moving);
            float lean = MotionMath.Lean(_speed, _data.LeanReferenceSpeed, _data.LeanDegrees);
            float breath = MotionMath.Breath(Time.time, _data.BreathPeriodSeconds, _data.BreathAmplitude) * (1f - moving);

            _model.localPosition = _baseLocalPosition + Vector3.up * height;
            _model.localRotation = _baseLocalRotation * Quaternion.Euler(lean, 0f, sway);
            float vertical = 1f - _squash + breath;
            float horizontal = 1f + (_squash - breath) * 0.5f; // mantém o volume mais ou menos igual
            _model.localScale = Vector3.Scale(_baseLocalScale, new Vector3(horizontal, vertical, horizontal));
        }

        private float NextHopHeight(float deltaTime)
        {
            if (_hopElapsed < 0f)
            {
                return 0f;
            }

            _hopElapsed += deltaTime;
            float progress = _hopElapsed / _data.HopSeconds;
            if (progress > 1f)
            {
                _hopElapsed = -1f;
                return 0f;
            }

            return MotionMath.Hop(progress, _data.HopHeight);
        }
    }
}
