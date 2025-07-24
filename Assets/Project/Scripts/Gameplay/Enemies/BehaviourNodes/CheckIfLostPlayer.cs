#define TESTING_BT

using CurseOfNaga.BehaviourTree;
using static CurseOfNaga.Global.UniversalConstant;

namespace CurseOfNaga.Gameplay.Enemies
{
    [System.Serializable]
    public class CheckIfLostPlayer : Node
    {
        private EnemyBoard _board;

#if TESTING_BT
        public void Initialize(EnemyBoard board)
        {
            _board = board;
        }
#endif

        public CheckIfLostPlayer(EnemyBoard board)
        {
            _board = board;
        }

        public override NodeState Evaluate(int currCount)
        {
            _CurrCount = currCount;

            //Player was visible and we have lost the player
            if ((_board.Status & EnemyStatus.PLAYER_VISIBLE) != 0)
            {
                _board.Status |= EnemyStatus.LOST_PLAYER;

                _NodeState = NodeState.SUCCESS;
                return NodeState.SUCCESS;
            }

            _NodeState = NodeState.FAILURE;
            return NodeState.FAILURE;
        }
    }
}