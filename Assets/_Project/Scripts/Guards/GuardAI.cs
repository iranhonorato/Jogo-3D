using System.Collections.Generic;
using Guaxinim.Guards.States;
using UnityEngine;
using UnityEngine.AI;

namespace Guaxinim.Guards
{
    /// <summary>
    /// Máquina de estados do segurança (GDD §6.2). Guarda o estado atual e faz a troca; cada estado
    /// (em Guards/States) decide quando sair. Os estados são criados uma vez, no Awake.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class GuardAI : MonoBehaviour
    {
        [SerializeField] private GuardData _data;

        [Tooltip("Pontos da patrulha, na ordem. Vazio: fica parado olhando em volta")]
        [SerializeField] private Transform[] _waypoints = new Transform[0];

        private IGuardState _currentState;

        public GuardData Data => _data;
        public NavMeshAgent Agent { get; private set; }
        public IReadOnlyList<Transform> Waypoints => _waypoints;
        public PatrolState Patrol { get; private set; }

        private void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();
            if (_data == null)
            {
                Debug.LogError("GuardAI needs Guard Data assigned.", this);
                enabled = false;
                return;
            }

            Patrol = new PatrolState(this);
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

        private void Update() => _currentState.Tick(Time.deltaTime);

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
    }
}
