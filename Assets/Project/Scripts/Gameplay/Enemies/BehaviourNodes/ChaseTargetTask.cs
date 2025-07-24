#define TESTING_BT

using UnityEngine;

using CurseOfNaga.BehaviourTree;
using static CurseOfNaga.Global.UniversalConstant;

namespace CurseOfNaga.Gameplay.Enemies
{
    [System.Serializable]
    public class ChaseTargetTask : Node
    {
#if !TESTING_BT
        private Transform _self;
        private Transform _target;
        private float _stopRange, _chaseSpeedMult;
#else
        public Transform _self;
        public Transform _target;
        public float _stopRange, _chaseSpeedMult;
#endif

        private EnemyBoard _board;

#if TESTING_BT
        public void Initialize(EnemyBoard board)
        {
            _board = board;
        }
#endif

        public ChaseTargetTask(EnemyBoard board, Transform self, Transform target, float stopRange, float speedMult)
        {
            _board = board;
            _target = target;
            _self = self;
            _stopRange = stopRange;
            _chaseSpeedMult = speedMult;
        }

        public override NodeState Evaluate(int currCount)
        {
            _CurrCount = currCount;
            Vector3 dirVec = _target.position - _self.position;


            if (dirVec.sqrMagnitude <= (_stopRange * _stopRange))
            {
                _board.Status |= EnemyStatus.REACHED_PLAYER;
                _board.Status &= ~EnemyStatus.CHASING_PLAYER;

                _NodeState = NodeState.SUCCESS;
                return NodeState.SUCCESS;
            }

            _board.Status |= EnemyStatus.CHASING_PLAYER;
            _self.transform.position += dirVec.normalized * Time.deltaTime * _chaseSpeedMult;

            _NodeState = NodeState.RUNNING;
            return NodeState.RUNNING;
        }
    }
}