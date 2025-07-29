#define TESTING_BT

using UnityEngine;

using System.Threading.Tasks;
using System.Threading;

using CurseOfNaga.BehaviourTree;
using static CurseOfNaga.Global.UniversalConstant;

namespace CurseOfNaga.Gameplay.Enemies
{
    [System.Serializable]
    public class PerformStrafeTask : Node
    {
#if !TESTING_BT
        private Transform _self;
        private Transform _target;
        private float _radius, _speedMult, _minStrafeTime, _maxStrafeTime;          //TODO: Make _minStrafeTime/_maxStrafeTime constant
#else
        public Transform _self;
        public Transform _target;
        public float _radius, _speedMult, _minStrafeTime, _maxStrafeTime;
#endif

        private int _dirMult;
        private float _selectedAngle;
        private EnemyBoard _board;
        private CancellationTokenSource _cts;

#if TESTING_BT
        public void Initialize(EnemyBoard board)
        {
            _cts = new CancellationTokenSource();
            _board = board;
        }
#endif

        ~PerformStrafeTask() { _cts.Cancel(); }

        public PerformStrafeTask(EnemyBoard board, Transform self, Transform target,
                float strafeRadius, float strafeSpeedMult)
        {
            _cts = new CancellationTokenSource();
            _board = board;
            _self = self;
            _target = target;
            _radius = strafeRadius;
            _speedMult = strafeSpeedMult;
        }

        /*
        *   - For Strafe:
        *       [=] Wait for the enemy to be at the position or something
        */
        public override NodeState Evaluate(int currCount)
        {
            _CurrCount = currCount;

            if (_board.SelectedCombatDecision != (byte)CombatDecision.STRAFE)
            {
                _NodeState = NodeState.FAILURE;
                return NodeState.FAILURE;
            }

            // Strafe is already playing
            if ((_board.CurrentDecisionIndex & EnemyBoard.ALREADY_PLAYING) == 0)
            {
                // _NodeState = NodeState.SUCCESS;
                // return NodeState.SUCCESS;
                _board.CurrentDecisionIndex |= EnemyBoard.ALREADY_PLAYING;
                _board.EnemyAnimator.SetInteger(EnemyBoard.COMBAT_DECISION, _board.SelectedCombatDecision);

                // Strafe to a random angle | Continue strafing for a random interval of time
                // _selectedAngle = Random.Range(45, 360);

                _dirMult = (Random.Range(0, 2) * 2) - 1;       //To invert the direction
                // _selectedAngle = Vector3.SignedAngle((_self.position - _target.position), _target.right, Vector3.up);
                _selectedAngle = Vector2.SignedAngle(new Vector2(_self.position.x - _target.position.x, _self.position.z - _target.position.z),
                                    new Vector2(_target.right.x, _target.right.z)) * -1;
                // Debug.Log($"_selectedAngle: {_selectedAngle} | x: {Mathf.Cos(_selectedAngle)} | z: {Mathf.Sin(_selectedAngle)}");

                MakeNewDecision(Random.Range(_minStrafeTime, _maxStrafeTime));
            }

            //Strafe around the player
            Vector3 dirVec = Vector3.zero;

            _selectedAngle += Time.deltaTime * _speedMult * _dirMult;
            dirVec.x = Mathf.Cos(_selectedAngle * Mathf.Deg2Rad) * _radius;
            dirVec.z = Mathf.Sin(_selectedAngle * Mathf.Deg2Rad) * _radius;

            _self.position = _target.position + dirVec;
            // MakeNewDecision(Random.Range(1f, 3f));

            _NodeState = NodeState.SUCCESS;
            return NodeState.SUCCESS;
        }

        private async void MakeNewDecision(float delayInSec)
        {
            await Task.Delay((int)(delayInSec * 1000));
            if (_cts.IsCancellationRequested) return;

            _board.CurrentDecisionIndex &= ~EnemyBoard.ALREADY_PLAYING;
            _NodeState = NodeState.IDLE;
            _board.SelectedCombatDecision = (byte)CombatDecision.NOT_DECIDED;
        }

        public virtual void CheckAttackConditions() { }
    }
}