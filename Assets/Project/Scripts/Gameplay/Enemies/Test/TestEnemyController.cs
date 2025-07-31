#define TESTING_BT

using UnityEngine;

using CurseOfNaga.BehaviourTree;

using static CurseOfNaga.Global.UniversalConstant;
using CurseOfNaga.Global;

namespace CurseOfNaga.Gameplay.Enemies.Test
{
    public class TestEnemyController : MonoBehaviour
    {
        private Node _rootNode;
        [SerializeField] private Transform _targetTransform, _weaponCollider, _playerVisual;

        [Header("Stats")]
        [SerializeField] private EntityInfo _baseInfo;

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

        [Header("Strafe Controls")]
        [Range(1.5f, 3f), SerializeField] private float _strafeRadius = 1.5f;
        [Range(20f, 50f), SerializeField] private float _dirSpeedMult = 30f;
        [Range(1f, 5f), SerializeField] private float _strafeSpeedMult = 2f;


        [Header("Behaviour Tree Controls")]
        [SerializeField] private PatrolAreaTask patrolArea;
        [SerializeField] private CheckPlayerRange checkPlayerVisibility;
        [SerializeField] private ChaseTargetTask chasePlayer;
        [SerializeField] private CheckIfLostPlayer checkIfLostPlayer;
        [SerializeField] private InvestigateAreaTask investigateArea;
        [SerializeField] private StayAndLookAroundTask lookAroundArea;
        [SerializeField] private CheckPlayerInAttackRange checkPlayerInAttackRange;
        [SerializeField] private MakeCombatDecisionTask makeCombatDecision;
        [SerializeField] private PerformAttackTask performAttack;
        [SerializeField] private PerformDefendTask performDefend;
        [SerializeField] private PerformStrafeTask performStrafe;

        private EnemyBoard _mainBoard;
        private IDamageable _damageableObject;
        // private EnemyStatus _mainEnemyStatus;

        private const float xPosFB = 0.05f, zPosFB = 0.57f, colOffset = 0.57f;
        private const int _PLAYER_LAYER = 6, _ENEMY_LAYER = 7;

        int debugIntVar = 0;
        bool _initialized = false;

        private void Start()
        {
            // InitializeTree();
            Invoke(nameof(InitializeTree), 2f);
        }

        // For the use of animator
        private void AnimationClipSatus(AnimationClipStatus status)
        {
            switch (status)
            {
                case AnimationClipStatus.PLAYING:
                    break;

                case AnimationClipStatus.FINISHED:
                    //An animation is already playing
                    _mainBoard.CurrentDecisionIndex |= EnemyBoard.PLAY_FINISHED;
                    _mainBoard.Status &= ~EnemyStatus.ATTACK_AT_HIT_POINT;

                    // Debug.Log($"Finished Clip | Setting Index : {_mainBoard.CurrentDecisionIndex}");
                    break;

                case AnimationClipStatus.REACHED_HIT_POINT:
                    _mainBoard.Status |= EnemyStatus.ATTACK_AT_HIT_POINT;

                    if ((_mainBoard.Status & EnemyStatus.PLAYER_WITHIN_RANGE) != 0)
                    {
                        Debug.Log($"Hit Player");
                        if (_damageableObject.ReceiveDamage(_baseInfo.Damage) <= 0)
                        {
                            // _mainBoard.Status &= ~EnemyStatus.HAVE_A_TARGET;
                            _targetTransform = null;

                            _mainBoard.Status = EnemyStatus.IDLE;
                            _mainBoard.CurrentDecisionIndex = EnemyBoard.NO_DECISION;
                            _mainBoard.SelectedCombatDecision = (byte)CombatDecision.NOT_DECIDED;
                            _mainBoard.EnemyAnimator.SetInteger(EnemyBoard.COMBAT_DECISION, (int)CombatDecision.NOT_DECIDED);
                            _mainBoard.EnemyAnimator.SetBool(EnemyBoard.LOCK_ANIMATION, false);
                        }
                    }

                    break;
            }
        }

        private void InitializeTree()
        {
            float[] clipLengths = new float[_animClips.Length];
            for (int i = 0; i < clipLengths.Length; i++)
                clipLengths[i] = _animClips[i].length;

            _mainBoard = new EnemyBoard(transform,
                TestAttackDataLoader.Instance.AttackDataParser.AttackTemplateData.attack_data[0].melee_combos,
                _enemyAnimator, clipLengths);

            _mainBoard.Status |= EnemyStatus.HAVE_A_TARGET;

#if TESTING_BT
            checkPlayerInAttackRange.Initialize(_mainBoard);
            makeCombatDecision.Initialize(_mainBoard);
            performAttack.Initialize(_mainBoard);
            performDefend.Initialize(_mainBoard);
            performStrafe.Initialize(_mainBoard);

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
            checkPlayerInAttackRange = new CheckPlayerInAttackRange(_mainBoard, transform, _targetTransform, _attackRange);
            makeCombatDecision = new MakeCombatDecisionTask(_mainBoard);
            performAttack = new PerformAttackTask(_mainBoard);
            performDefend = new PerformDefendTask(_mainBoard);
            performStrafe = new PerformStrafeTask(_mainBoard, transform, _targetTransform, _strafeRadius, _strafeSpeedMult,
                _chaseSpeedMult);

            checkPlayerVisibility = new CheckPlayerRange(_mainBoard, transform, _targetTransform, _playerVisibleRange);
            chasePlayer = new ChaseTargetTask(_mainBoard, transform, _targetTransform, _chaseStopRange, _chaseSpeedMult);

            checkIfLostPlayer = new CheckIfLostPlayer(_mainBoard);
            investigateArea = new InvestigateAreaTask(transform, _mainBoard, _searchRange, _totalSearchDuration,
                _investigateSpeedMult);
            lookAroundArea = new StayAndLookAroundTask(_mainBoard, _totalSearchDuration);

            Vector3[] patrolPoints = new Vector3[_patrolPoints.Length];
            for (int i = 0; i < _patrolPoints.Length; i++)
            {
                patrolPoints[i] = _patrolPoints[i].position;
            }
            patrolArea = new PatrolAreaTask(transform, patrolPoints, _patrolSpeedMult, _patrolWaitTime);
#endif
            Selector attackSelector = new Selector(new Node[]{
                new Invertor(makeCombatDecision),
                performAttack,
                performDefend,
                performStrafe
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

        /*
        *        |                        |      
        *  [-,+] | [+,+]  Turn 180  [+,-] | [-,-]
        *  -------------    =>      -------------
        *  [-,-] | [+,-]            [+,+] | [-,+]
        *        |                        |      
        */
        private void Update()
        {
            if (!_initialized) return;

            _rootNode.Evaluate(debugIntVar);
        }

        // public Vector3 _dirVecDebug;
        // public float XDebug, YDebug;
        private void FixedUpdate()
        {
            if (!_initialized) return;

            // Face the Player when attacking
            if ((_mainBoard.Status & EnemyStatus.ATTACKING_PLAYER) != 0)
            {
                Vector3 dirVec = (_targetTransform.position - transform.position).normalized;
                Vector3 colliderPos = _weaponCollider.localPosition;
                colliderPos.x = Mathf.Round(dirVec.x) * colOffset;
                colliderPos.z = Mathf.Round(dirVec.z) * colOffset;
                _weaponCollider.localPosition = colliderPos;

                Vector3 finalRot = Vector3.zero;
                finalRot.y = 180f * Mathf.Abs(Mathf.Floor(dirVec.x));       //Floor down to get -1 and Abs it
                _playerVisual.localEulerAngles = finalRot;
            }
            // _dirVecDebug = (_playerTransform.position - transform.position).normalized;
            // XDebug = Mathf.Round(_dirVecDebug.x);
            // YDebug = Mathf.Round(_dirVecDebug.y);
        }

        private void OnTriggerEnter(Collider other)
        {
            // Can boxcast instead
            if (other.gameObject.layer == _PLAYER_LAYER)
            {
                Debug.Log($"Trigger Enter: {other.name}");
                _mainBoard.Status |= EnemyStatus.PLAYER_WITHIN_RANGE;
                _damageableObject = other.GetComponent<IDamageable>();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            // Can boxcast instead
            if (other.gameObject.layer == _PLAYER_LAYER)
            {
                Debug.Log($"Trigger Exit: {other.name}");
                _mainBoard.Status &= ~EnemyStatus.PLAYER_WITHIN_RANGE;
                _damageableObject = null;
            }
        }
    }
}