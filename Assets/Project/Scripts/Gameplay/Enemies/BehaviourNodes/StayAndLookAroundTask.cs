using CurseOfNaga.BehaviourTree;
using UnityEngine;
using static CurseOfNaga.Global.UniversalConstant;

namespace CurseOfNaga.Gameplay.Enemies
{
    public class StayAndLookAroundTask : Node
    {
        private EnemyBoard _board;
        private float _totalSearchDuration, _currDuration;          //Max time enemy will take to search the player

        public StayAndLookAroundTask(EnemyBoard board)
        {
            _board = board;
        }

        public override NodeState Evaluate(int currCount)
        {
            _CurrCount = currCount;

            if ((_board.Status & EnemyStatus.LOST_PLAYER) != 0 && (_board.Status & EnemyStatus.INVESTIGATE_AREA) == 0)
            {
                _board.Status |= EnemyStatus.INVESTIGATE_AREA;
                _currDuration = Time.time;

                // _NodeState = NodeState.SUCCESS;
                // return NodeState.SUCCESS;
            }
            //Search Time up
            else if (Time.time - _currDuration >= (_totalSearchDuration / 2))           //This will be less than the investigation
            {
                _board.Status &= ~EnemyStatus.PLAYER_VISIBLE;
                _board.Status &= ~EnemyStatus.LOST_PLAYER;
                _board.Status &= ~EnemyStatus.INVESTIGATE_AREA;

                _NodeState = NodeState.FAILURE;
                return NodeState.FAILURE;
            }

            //Some rotate Code to scan the area around the Enemy for player

            _NodeState = NodeState.RUNNING;
            return NodeState.RUNNING;
        }
    }
}