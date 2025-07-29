#define TESTING_BT

using UnityEngine;

using System.Threading.Tasks;
using System.Threading;

using CurseOfNaga.BehaviourTree;
using static CurseOfNaga.Global.UniversalConstant;

namespace CurseOfNaga.Gameplay.Enemies
{
    [System.Serializable]
    // The conditions would be different for different enemy types, so this should be abstract
    // public abstract class DecideAttackTypeTask : Node
    public class MakeCombatDecisionTask : Node
    {
#if !TESTING_BT
        // private Transform _origin;
        // private Transform _target;
        // private float _range;
#else
#endif
        private EnemyBoard _board;
        private CancellationTokenSource _cts;

#if TESTING_BT
        public void Initialize(EnemyBoard board)
        {
            _cts = new CancellationTokenSource();
            _board = board;
        }
#endif

        ~MakeCombatDecisionTask() { _cts.Cancel(); }

        public MakeCombatDecisionTask(EnemyBoard board)
        {
            _cts = new CancellationTokenSource();
            _board = board;
        }

        /*
        *   - We are parsing the attack data on another script, so we have the data required
        *   - We are choosing a random combo from the list to follow for an attack decision
        *   - The Enemy will continue to chain all the combos and never stop. To stop this, we need the decision 
        *     to be of different types and not the combos. Different decisions eg: Attack, Defend, Strafe around the Player,
        *     Just stand and wait, etc.
        *   - We would need to wait to make another decision. The flow could be:
        *     Current Loop[L0] Make a Decision -> [L0] Return SUCCESS -> Next Loop[L1] Wait for decision to finish 
        *     -> [L1] Check flags to see if the decision has finished -> [L1] Return SUCCESS 
        *     -> Continue checking till [LN] -> [LN] Make another decision
        *
        *   - For Attack:
        *       [=] Need to decide which type of attack to perform
        *           {+} Random combo from list | Normal Combo [NORMAL_M0 + NORMAL_M1] | Normal M0 or M1 attack
        *       [=] Wait for the Combo to finish and then select another type
        *   - For Strafe:
        *       [=] Wait for the enemy to be at the position or something
        *   - For Defend:
        *       [=] Stop defending when player has stopped atacking
        *       [=] If random defending, then wait for timeout
        *   - For Wait:
        *       [=] This might be random wait?
        */
        public override NodeState Evaluate(int currCount)
        {
            _CurrCount = currCount;

            //This can logically never return FAILURE and will always return SUCESS, as enemy will always attack player, but FEAR SYSTEM?
            //Always fall back to Melee

            if (_board.SelectedCombatDecision == (byte)CombatDecision.NOT_DECIDED)
            {
                // TODO: Check conditions for determining which combat decision to take
                // For now, just choosing a random decision

                // _board.SelectedCombatDecision = (byte)Random.Range((int)CombatDecision.ATTACK, (int)CombatDecision.WAIT + 1);
                _board.SelectedCombatDecision = (byte)CombatDecision.WAIT;          //TEST

                switch ((CombatDecision)_board.SelectedCombatDecision)
                {
                    case CombatDecision.ATTACK:
                        _board.AttackTypeBase = (byte)EnemyAttackType.MELEE;

                        // Select a combo if not selected
                        // Offset due to DEFAULT
                        _board.SelectedCombo = (byte)ComboType.COMBO_0 - 1;
                        // _board.SelectedComboIndex = (byte)Random.Range(0, _board.MeleeCombos.Count) - 1;

                        break;

                    // Just Pass to next node
                    case CombatDecision.DEFEND:
                    case CombatDecision.STRAFE:
                        MakeNewDecision(Random.Range(1f, 3f));          //TEST

                        break;

                    case CombatDecision.WAIT:
                        MakeNewDecision(Random.Range(1f, 3f));

                        break;
                }

                // If Melee is selected , then decide which combo to use

                Debug.Log($"SelectedComboIndex: {_board.SelectedCombatDecision}");
            }


            switch ((CombatDecision)_board.SelectedCombatDecision)
            {
                case CombatDecision.WAIT:
                    _NodeState = NodeState.FAILURE;
                    return NodeState.FAILURE;

                default:
                case CombatDecision.ATTACK:
                case CombatDecision.DEFEND:
                case CombatDecision.STRAFE:
                    _NodeState = NodeState.SUCCESS;
                    return NodeState.SUCCESS;
            }
        }

        private async void MakeNewDecision(float delayInSec)
        {
            await Task.Delay((int)(delayInSec * 1000));
            if (_cts.IsCancellationRequested) return;

            _board.SelectedCombatDecision = (byte)CombatDecision.NOT_DECIDED;
        }

        public virtual void CheckAttackConditions() { }
    }
}