using UnityEngine;

using CurseOfNaga.BehaviourTree;
using static CurseOfNaga.Global.UniversalConstant;

namespace CurseOfNaga.Gameplay.Enemies
{
    public class CheckPlayerRange : Node
    {
        private Transform _origin;
        private Transform _target;
        private float _range;
        private EnemyStatus _enemyStatus;

        public CheckPlayerRange(Transform origin, Transform target, float range, ref EnemyStatus status)
        {
            _target = target;
            _origin = origin;
            _range = range;
            _enemyStatus = status;
        }

        public override NodeState Evaluate(int currCount)
        {
            _CurrCount = currCount;

            if (Vector3.SqrMagnitude(_target.position - _origin.position) <= (_range * _range))
            {
                _enemyStatus |= EnemyStatus.PLAYER_VISIBLE;
                _enemyStatus &= ~EnemyStatus.LOST_PLAYER;

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