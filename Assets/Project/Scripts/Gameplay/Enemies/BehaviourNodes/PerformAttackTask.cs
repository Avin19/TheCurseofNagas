/*
*   TODO: Sub-divide to different classes,or this will turn out to be a long class instead
*   TODO: This might also need to be an astract class for different enemies
*   [IMP] 1 Attack = 1 Action | Eg: A horizontal slice/A vertical slice from a weapon
*   Only one class to perform the attack as the decision to do what kind of attack has already been made in the earlier node
*      
*   [IMP] Should this be an animation class?   
*       [=] Cannot think of any logic to put here
*       [=] During ranged attack, if Enemy has bow, woudl need to add force to the bow
*           {+} Can be done by invoking action for separate script callback
*
*/

#define TESTING_BT

using UnityEngine;

using CurseOfNaga.BehaviourTree;
using static CurseOfNaga.Global.UniversalConstant;

namespace CurseOfNaga.Gameplay.Enemies
{
    [System.Serializable]
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


        public override NodeState Evaluate(int currCount)
        {
            _CurrCount = currCount;

            //  Play out the attack combo
            // if ((_board.AttackType & EnemyAttackType.ATTACK_DECIDED) != 0
            //     && (_board.AttackType & EnemyAttackType.MELEE) != 0)
            switch (_board.AttackTypeBase)
            {
                case (byte)EnemyAttackType.MELEE:
                    EnemyAttackType attackType = (EnemyAttackType)_board.MeleeCombos[_board.SelectedComboIndex]
                                                .ComboSequence[_board.CurrentAttackIndex];
                    switch (attackType)
                    {
                        case
                    }

                    break;

                case (byte)EnemyAttackType.RANGED_ATTACK:
                    break;
            }
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