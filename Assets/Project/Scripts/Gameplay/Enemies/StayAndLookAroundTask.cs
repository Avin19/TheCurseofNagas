using CurseOfNaga.BehaviourTree;
using static CurseOfNaga.Global.UniversalConstant;

namespace CurseOfNaga.Gameplay.Enemies
{
    public class StayAndLookAroundTask : Node
    {
        private EnemyStatus _enemyStatus;

        public StayAndLookAroundTask(ref EnemyStatus status)
        {
            _enemyStatus = status;
        }

        public override NodeState Evaluate(int currCount)
        {
            _CurrCount = currCount;

            if ((_enemyStatus & EnemyStatus.LOST_PLAYER) != 0)
            {
                _NodeState = NodeState.SUCCESS;
                return NodeState.SUCCESS;
            }


            _NodeState = NodeState.FAILURE;
            return NodeState.FAILURE;
        }
    }
}