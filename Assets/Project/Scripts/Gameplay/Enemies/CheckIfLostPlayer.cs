using CurseOfNaga.BehaviourTree;
using static CurseOfNaga.Global.UniversalConstant;

namespace CurseOfNaga.Gameplay.Enemies
{
    public class CheckIfLostPlayer : Node
    {
        private EnemyStatus _enemyStatus;

        public CheckIfLostPlayer(ref EnemyStatus status)
        {
            _enemyStatus = status;
        }

        public override NodeState Evaluate(int currCount)
        {
            _CurrCount = currCount;

            //Player was visible and we have lost the player
            if ((_enemyStatus & EnemyStatus.PLAYER_VISIBLE) != 0)
            {
                _enemyStatus |= EnemyStatus.LOST_PLAYER;

                _NodeState = NodeState.SUCCESS;
                return NodeState.SUCCESS;
            }

            _NodeState = NodeState.FAILURE;
            return NodeState.FAILURE;
        }
    }
}