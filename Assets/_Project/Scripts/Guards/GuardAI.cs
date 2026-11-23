using System.Collections.Generic;
using Guaxinim.Guards.States;
using Guaxinim.Noise;
using UnityEngine;
using UnityEngine.AI;

namespace Guaxinim.Guards
{
    /// <summary>
    /// Máquina de estados do segurança (GDD §6.2). Guarda o estado atual, faz a troca e liga os
    /// sentidos aos estados: medidor cheio com o guaxinim à vista leva à Perseguição; um som ou um
    /// relance levam ao Desconfiado. Cada estado (em Guards/States) decide quando sair.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class GuardAI : MonoBehaviour
    {
        [SerializeField] private GuardData _data;

        [Tooltip("Pontos da patrulha, na ordem. Vazio: fica no ponto inicial olhando em volta")]
        [SerializeField] private Transform[] _waypoints = new Transform[0];

        [Header("Sentidos")]
        [SerializeField] private VisionCone _vision;
        [SerializeField] private DetectionMeter _meter;
        [SerializeField] private NoiseListener _hearing;

        private IGuardState _currentState;

        public GuardData Data => _data;
        public VisionCone Vision => _vision;
        public NavMeshAgent Agent { get; private set; }
        public IReadOnlyList<Transform> Waypoints => _waypoints;
        public Vector3 HomePosition { get; private set; }

        public PatrolState Patrol { get; private set; }
        public SuspiciousState Suspicious { get; private set; }
        public ChaseState Chase { get; private set; }
        public SearchState Search { get; private set; }
        public ReturnState Return { get; private set; }

        public bool IsChasing => _currentState == Chase;

        private void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();
            if (_data == null || _vision == null || _meter == null || _hearing == null)
            {
                Debug.LogError("GuardAI needs Data, Vision, Meter and Hearing assigned.", this);
                enabled = false;
                return;
            }

            HomePosition = transform.position;
            CreateStates();
        }

        private void OnEnable()
        {
            if (_hearing != null)
            {
                _hearing.OnNoiseHeard += HandleNoiseHeard;
            }
        }

        private void Start()
        {
            if (!Agent.isOnNavMesh)
            {
                Debug.LogError("Guard is not on a NavMesh. Bake the NavMeshSurface of the scene.", this);
                enabled = false;
                return;
            }

            ChangeState(Patrol);
        }

        private void Update()
        {
            ReactToSight();
            _currentState.Tick(Time.deltaTime);
        }

        private void OnDisable()
        {
            if (_hearing != null)
            {
                _hearing.OnNoiseHeard -= HandleNoiseHeard;
            }
        }

        public void ChangeState(IGuardState nextState)
        {
            if (nextState == _currentState)
            {
                return;
            }

            _currentState?.Exit(); // IGuardState não é UnityEngine.Object, então ?. é seguro
            _currentState = nextState;
            _currentState.Enter();
        }

        /// <summary>Vai investigar um ponto (som ou relance) no estado Desconfiado.</summary>
        public void Investigate(Vector3 point)
        {
            Suspicious.SetPoint(point);
            ChangeState(Suspicious);
        }

        public void MoveTo(Vector3 destination)
        {
            Agent.isStopped = false;
            Agent.SetDestination(destination);
        }

        public bool HasArrived() => !Agent.pathPending && Agent.remainingDistance <= _data.ArrivalDistance;

        private void CreateStates()
        {
            Patrol = new PatrolState(this);
            Suspicious = new SuspiciousState(this);
            Chase = new ChaseState(this);
            Search = new SearchState(this);
            Return = new ReturnState(this);
        }

        private void ReactToSight()
        {
            if (IsChasing || !_vision.HasVisibleTarget)
            {
                return;
            }

            if (_meter.IsFull)
            {
                ChangeState(Chase);
                return;
            }

            Investigate(_vision.VisibleTarget.DetectionPoint); // GDD §6.2: viu de relance
        }

        private void HandleNoiseHeard(NoiseEvent noise)
        {
            if (IsChasing || _currentState == null)
            {
                return;
            }

            Investigate(noise.Position);
        }
    }
}
