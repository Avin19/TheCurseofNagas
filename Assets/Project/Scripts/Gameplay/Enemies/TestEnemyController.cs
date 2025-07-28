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

        [Header("Animation Controls")]
        [SerializeField] private Animator _enemyAnimator;
        [SerializeField] private AnimationClip[] _animClips;

        [Header("Chase Controls")]
        [SerializeField] private float _playerVisibleRange;
        [SerializeField] private float _chaseStopRange, _chaseSpeedMult;

        [Header("Patrol Controls")]
        [SerializeField] private Transform[] _patrolPoints;
        [Range(1f, 10f), SerializeField] private float _patrolSpeedMult;
        [Range(1f, 3f), SerializeField] private float _patrolWaitTime;

        [Header("Investigation Controls")]
        [Range(1f, 3f), SerializeField] private float _searchRange;
        [Range(1f, 3f), SerializeField] private float _totalSearchDuration;
        [Range(1f, 3f), SerializeField] private float _investigateSpeedMult;

        [Header("Attack Controls")]
        [Range(1f, 3f), SerializeField] private float _attackRange;

        [Header("Behaviour Tree Controls")]
        [SerializeField] private PatrolAreaTask patrolArea;
        [SerializeField] private CheckPlayerRange checkPlayerVisibility;
        [SerializeField] private ChaseTargetTask chasePlayer;
        [SerializeField] private CheckIfLostPlayer checkIfLostPlayer;
        [SerializeField] private InvestigateAreaTask investigateArea;
        [SerializeField] private StayAndLookAroundTask lookAroundArea;
        [SerializeField] private CheckPlayerInAttackRange checkPlayerInAttackRange;
        [SerializeField] private DecideAttackTypeTask decideAttackType;
        [SerializeField] private PerformAttackTask performAttack;

        private EnemyBoard _mainBoard;
        // private EnemyStatus _mainEnemyStatus;

        int debugIntVar = 0;
        bool _initialized = false;

        private void Start()
        {
            // InitializeTree();
            Invoke(nameof(InitializeTree), 2f);
        }

        // For the use of animator
        private void AnimationClipFinished()
        {
            //An animation is already playing
            _mainBoard.CurrentAttackIndex |= EnemyBoard.PLAY_FINISHED;

            Debug.Log($"Finished Clip | Setting Index : {_mainBoard.CurrentAttackIndex}");
        }

        private void InitializeTree()
        {
            float[] clipLengths = new float[_animClips.Length];
            for (int i = 0; i < clipLengths.Length; i++)
                clipLengths[i] = _animClips[i].length;

            _mainBoard = new EnemyBoard(transform,
                TestAttackDataLoader.Instance.AttackDataParser.AttackTemplateData.attack_data[0].melee_combos,
                _enemyAnimator, clipLengths);

#if TESTING_BT
            checkPlayerInAttackRange.Initialize(_mainBoard);
            decideAttackType.Initialize(_mainBoard);
            performAttack.Initialize(_mainBoard);

            chasePlayer.Initialize(_mainBoard);
            checkPlayerVisibility.Initialize(_mainBoard);

            checkIfLostPlayer.Initialize(_mainBoard);
            investigateArea.Initialize(_mainBoard);
            // lookAroundArea.Initialize(_mainBoard);

            patrolArea._patrolPoints = new Vector3[_patrolPoints.Length];
            for (int i = 0; i < _patrolPoints.Length; i++)
            {
                patrolArea._patrolPoints[i] = _patrolPoints[i].position;
            }
#else
            checkPlayerInAttackRange = new CheckPlayerInAttackRange(_mainBoard, transform, _playerTransform, _attackRange);
            decideAttackType = new DecideAttackTypeTask(_mainBoard);
            performAttack = new PerformAttackTask(_mainBoard);

            checkPlayerVisibility = new CheckPlayerRange(_mainBoard, transform, _playerTransform, _playerVisibleRange, _mainBoard);
            chasePlayer = new ChaseTargetTask(transform, _playerTransform, _chaseStopRange, _chaseSpeedMult);

            checkIfLostPlayer = new CheckIfLostPlayer(_mainBoard);
            investigateArea = new InvestigateAreaTask(transform, _mainBoard, _searchRange, _totalSearchDuration, _investigateSpeedMult);
            lookAroundArea = new StayAndLookAroundTask(_mainBoard, _totalSearchDuration);

            Vector3 patrolPoints = new Vector3[_patrolPoints.Length];
            for (int i = 0; i < _patrolPoints.Length; i++)
            {
                patrolPoints[i] = _patrolPoints[i].position;
            }
            patrolArea = new PatrolAreaTask(transform, patrolPoints, _patrolSpeedMult, _patrolWaitTime);
#endif
            Selector attackSelector = new Selector(new Node[]{
                new Invertor(decideAttackType),
                performAttack
            });

            Sequence attackSequence = new Sequence(new Node[] {
                checkPlayerInAttackRange,
                attackSelector
            });

            Sequence chasePlayerSequence = new Sequence(new Node[] {
                checkPlayerVisibility,
                chasePlayer
            });

            Sequence investigateForPlayer = new Sequence(new Node[] {
                checkIfLostPlayer,
                investigateArea
            });

            _rootNode = new Selector(new Node[] {
                attackSequence,
                chasePlayerSequence,
                investigateForPlayer,
                patrolArea
            });

            _initialized = true;
        }


        private void Update()
        {
            if (!_initialized) return;

            _rootNode.Evaluate(debugIntVar);
        }
    }
}