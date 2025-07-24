#define TESTING_BT

using UnityEngine;

using CurseOfNaga.BehaviourTree;

using static CurseOfNaga.Global.UniversalConstant;

namespace CurseOfNaga.Gameplay.Enemies
{
    public class TestEnemyController : MonoBehaviour
    {
        private Node _rootNode;
        [SerializeField] private Transform _playerTransform;

        [Header("Chase Controls")]
        [SerializeField] private float _playerVisibleRange;
        [SerializeField] private float _chaseStopRange, _chaseSpeedMult;

        [Header("Patrol Controls")]
        [SerializeField] private Transform[] _patrolPoints;
        [Range(1f, 10f), SerializeField] private float _patrolSpeedMult;
        [Range(1f, 3f), SerializeField] private float _patrolWaitTime;

        [Header("Behaviour Tree Controls")]
        [SerializeField] private PatrolAreaTask patrolArea;
        [SerializeField] private CheckPlayerRange checkPlayerVisibility;

        private EnemyBoard _mainBoard;
        // private EnemyStatus _mainEnemyStatus;

        int debugIntVar = 0;

        private void Start()
        {
            // Invoke(nameof(InitializeTree), 2f);
            _mainBoard = new EnemyBoard(transform);
            InitializeTree();
        }

        private void InitializeTree()
        {
            // Selector 

            ChaseTargetTask chasePlayer = new ChaseTargetTask(transform, _playerTransform, _chaseStopRange, _chaseSpeedMult);

#if TESTING_BT
            // checkPlayerVisibility.Initialize(ref _mainEnemyStatus);
            checkPlayerVisibility.Initialize(_mainBoard);

            patrolArea._patrolPoints = new Vector3[_patrolPoints.Length];
            for (int i = 0; i < _patrolPoints.Length; i++)
            {
                patrolArea._patrolPoints[i] = _patrolPoints[i].position;
            }
#else
            checkPlayerVisibility = new CheckPlayerRange(transform, _playerTransform, _playerVisibleRange, _mainBoard);

            Vector3 patrolPoints = new Vector3[_patrolPoints.Length];
            for (int i = 0; i < _patrolPoints.Length; i++)
            {
                patrolPoints[i] = _patrolPoints[i].position;
            }
            patrolArea = new PatrolAreaTask(transform, patrolPoints, _patrolSpeedMult, _patrolWaitTime);
#endif

            Sequence chasePlayerSequence = new Sequence(new Node[] {
                checkPlayerVisibility,
                // chasePlayer
            });

            _rootNode = new Selector(new Node[] {
                chasePlayerSequence,
                patrolArea
            });
        }


        private void Update()
        {
            _rootNode.Evaluate(debugIntVar);
        }
    }
}