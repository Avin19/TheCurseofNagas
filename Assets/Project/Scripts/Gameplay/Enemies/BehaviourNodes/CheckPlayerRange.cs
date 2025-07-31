#define TESTING_BT

using UnityEngine;

using CurseOfNaga.BehaviourTree;
using static CurseOfNaga.Global.UniversalConstant;

namespace CurseOfNaga.Gameplay.Enemies
{
    [System.Serializable]
    public class CheckPlayerRange : Node
    {
#if !TESTING_BT
        private Transform _origin;
        private Transform _target;
        private float _range;
#else
        public Transform _origin;
        public Transform _target;
        public float _range;
#endif
        private EnemyBoard _board;

#if TESTING_BT
        public void Initialize(EnemyBoard board)
        {
            _board = board;
        }
#endif

        public CheckPlayerRange(EnemyBoard board, Transform origin, Transform target, float range)
        {
            _target = target;
            _origin = origin;
            _range = range;
            _board = board;
        }

        public override NodeState Evaluate(int currCount)
        {
            _CurrCount = currCount;

            if ((_board.Status & EnemyStatus.HAVE_A_TARGET) == 0)
            {
                _NodeState = NodeState.FAILURE;
                return NodeState.FAILURE;
            }
            else if (Vector3.SqrMagnitude(_target.position - _origin.position) <= (_range * _range))
            {
                _board.Status |= EnemyStatus.PLAYER_VISIBLE;
                _board.Status &= ~EnemyStatus.LOST_PLAYER;

                _NodeState = NodeState.SUCCESS;
                return NodeState.SUCCESS;
            }
            // else if ((_enemyStatus & EnemyStatus.PLAYER_VISIBLE) != 0)
            //     _enemyStatus |= EnemyStatus.LOST_PLAYER;
            // else
            //     _enemyStatus &= ~EnemyStatus.PLAYER_VISIBLE;

            //We will not reset the PLAYER_VISIBLE flag for EnemyStatus as the next Node will take care of it 

            _NodeState = NodeState.FAILURE;
            return NodeState.FAILURE;
        }
    }
}