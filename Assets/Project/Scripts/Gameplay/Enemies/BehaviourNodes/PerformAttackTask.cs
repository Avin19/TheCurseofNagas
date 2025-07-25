#define TESTING_BT

using UnityEngine;

using CurseOfNaga.BehaviourTree;
using static CurseOfNaga.Global.UniversalConstant;

namespace CurseOfNaga.Gameplay.Enemies
{
    [System.Serializable]
    // This might also need to be an astract class for different enemies
    // Only one class to perform the attack as the decision to do what kind of attack has already been made in the earlier node
    public class PerformAttackTask : Node
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

        public PerformAttackTask(EnemyBoard board)
        {
            _board = board;
        }

        //  - Choose a random combo? OR Choose the base combo for everyone?
        //      [=] Would be more defined than others
        //          <~> How to access the combo data?
        //  - Choose a random attack every time?
        //      [=] Could be more hectic to manage
        //          <~> Would need to define more constraints for this to look good or perform good
        //      [=] Eg: Normal_M1 -> Normal_M2 -> Heavy_M1 -> Normal_M2
        //      [=] Eg: Normal_M2 -> Heavy_M2 -> Heavy_M1 -> Normal_M1

        public override NodeState Evaluate(int currCount)
        {
            _CurrCount = currCount;

            //  Determine which kind of attack can the Enemy perform from the the list of attacks

            //  Play out the attack combo
            // if ((_board.AttackType & EnemyAttackType.ATTACK_DECIDED) != 0
            //     && (_board.AttackType & EnemyAttackType.MELEE) != 0)
            if (_board.AttackTypeBase == (byte)EnemyAttackType.MELEE)
            {
                // Determine the attack type
                EnemyAttackType attackType;


            }
            // else if ((_board.AttackType & EnemyAttackType.ATTACK_DECIDED) != 0
            //     && (_board.AttackType & EnemyAttackType.RANGED_ATTACK) != 0)
            else if (_board.AttackTypeBase == (byte)EnemyAttackType.RANGED_ATTACK)
            {

            }

            _NodeState = NodeState.SUCCESS;
            return NodeState.SUCCESS;

            // _NodeState = NodeState.FAILURE;
            // return NodeState.FAILURE;
        }

        public virtual void CheckAttackConditions() { }
    }
}