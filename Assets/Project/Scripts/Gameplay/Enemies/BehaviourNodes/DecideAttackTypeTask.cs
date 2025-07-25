#define TESTING_BT

using UnityEngine;

using CurseOfNaga.BehaviourTree;
using static CurseOfNaga.Global.UniversalConstant;

namespace CurseOfNaga.Gameplay.Enemies
{
    [System.Serializable]
    // The conditions would be different for different enemy types, so this should be abstract
    // public abstract class DecideAttackTypeTask : Node
    public class DecideAttackTypeTask : Node
    {
#if !TESTING_BT
        // private Transform _origin;
        // private Transform _target;
        // private float _range;
#else
#endif
        private EnemyBoard _board;

#if TESTING_BT
        public void Initialize(EnemyBoard board)
        {
            _board = board;
        }
#endif

        public DecideAttackTypeTask(EnemyBoard board)
        {
            _board = board;
        }

        public override NodeState Evaluate(int currCount)
        {
            _CurrCount = currCount;

            //Check conditions for determining what to attack player with
            //This can logically never return FAILURE and will always return SUCESS, as enemy will always attack player, but FEAR SYSTEM?
            //Always fall back to Melee

            _board.AttackType |= EnemyAttackType.MELEE;

            _NodeState = NodeState.SUCCESS;
            return NodeState.SUCCESS;

            // _NodeState = NodeState.FAILURE;
            // return NodeState.FAILURE;
        }

        public virtual void CheckAttackConditions() { }
    }
}