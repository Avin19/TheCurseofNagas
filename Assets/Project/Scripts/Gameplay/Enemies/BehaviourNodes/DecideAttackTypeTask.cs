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

            //Check conditions for determining what to attack player with
            //This can logically never return FAILURE and will always return SUCESS, as enemy will always attack player, but FEAR SYSTEM?
            //Always fall back to Melee

            // If Melee is selected , then decide which combo to use
            _board.AttackTypeBase = (byte)EnemyAttackType.MELEE;

            // Select a combo if not selected
            if (_board.SelectedComboIndex == 255)
                _board.SelectedComboIndex = (byte)Random.Range(0, _board.MeleeCombos.Count);

            _NodeState = NodeState.SUCCESS;
            return NodeState.SUCCESS;

            // _NodeState = NodeState.FAILURE;
            // return NodeState.FAILURE;
        }

        public virtual void CheckAttackConditions() { }
    }
}