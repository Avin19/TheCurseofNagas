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
using System.Threading;
using System.Threading.Tasks;

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
        private CancellationTokenSource _cts;


#if TESTING_BT
        public void Initialize(EnemyBoard board)
        {
            _cts = new CancellationTokenSource();
            _board = board;
        }
#endif

        ~PerformAttackTask()
        {
            _cts.Cancel();
        }

        public PerformAttackTask(EnemyBoard board)
        {
            _cts = new CancellationTokenSource();
            _board = board;
        }

        //  Play out the attack combo
        public override NodeState Evaluate(int currCount)
        {
            _CurrCount = currCount;

            if (_board.SelectedCombatDecision != (byte)CombatDecision.ATTACK)
            {
                _NodeState = NodeState.FAILURE;
                return NodeState.FAILURE;
            }

            // An Attack is already playing
            if ((_board.CurrentDecisionIndex & EnemyBoard.ALREADY_PLAYING) != 0)
            {
                _NodeState = NodeState.SUCCESS;
                return NodeState.SUCCESS;
            }

            switch (_board.AttackTypeBase)
            {
                case (byte)EnemyAttackType.MELEE:
                    // Debug.Log($"CurrentAttackIndex: {_board.CurrentDecisionIndex} | MeleeCombos: {_board.MeleeCombos[_board.SelectedCombo]}");

                    Time.timeScale = 0.15f;
                    EnemyAttackType attackType = (EnemyAttackType)_board.MeleeCombos[_board.SelectedCombo]
                                                .ComboSequence[_board.CurrentDecisionIndex];

                    _board.CurrentDecisionIndex |= EnemyBoard.ALREADY_PLAYING;
                    switch (attackType)
                    {
                        case EnemyAttackType.NORMAL_M0:
                        case EnemyAttackType.NORMAL_M1:
                        case EnemyAttackType.NORMAL_M2:
                        case EnemyAttackType.NORMAL_M3:
                        case EnemyAttackType.NORMAL_M4:
                        case EnemyAttackType.NORMAL_M5:
                            _board.DamageMultiplier = 1f;
                            _board.EnemyAnimator.SetInteger(EnemyBoard.COMBAT_DECISION, _board.SelectedCombatDecision);
                            _board.EnemyAnimator.SetInteger(EnemyBoard.PERFORM_ATTACK, (int)attackType);

                            /*
                            // Offset of 2 as NORMAL_M0 starts at 3
                            ChangeAnimatorClip(_board.AnimClipLengths[(int)attackType
                                - (int)EnemyAttackType.NORMAL_M0 + 1]);
                            */
                            ChangeAnimatorClip();

                            break;

                        case EnemyAttackType.HEAVY_M0:
                        case EnemyAttackType.HEAVY_M1:
                        case EnemyAttackType.HEAVY_M2:
                        case EnemyAttackType.HEAVY_M3:
                        case EnemyAttackType.HEAVY_M4:
                        case EnemyAttackType.HEAVY_M5:
                            _board.DamageMultiplier = 1.5f;
                            _board.EnemyAnimator.SetInteger(EnemyBoard.COMBAT_DECISION, _board.SelectedCombatDecision);
                            _board.EnemyAnimator.SetInteger(EnemyBoard.PERFORM_ATTACK, (int)attackType);

                            /*
                            // Offset of 12 as HEAVY_M0 starts at 13 + Offset of Normal Attacks
                            ChangeAnimatorClip(_board.AnimClipLengths[(int)attackType - (int)EnemyAttackType.HEAVY_M0
                                + 1 + (int)EnemyAttackType.NORMAL_M5]);
                            */
                            ChangeAnimatorClip();

                            break;
                    }

                    break;

                case (byte)EnemyAttackType.RANGED_ATTACK:
                    break;
            }

            _NodeState = NodeState.SUCCESS;
            return NodeState.SUCCESS;

            // _NodeState = NodeState.FAILURE;
            // return NodeState.FAILURE;
        }

        //FIXME: Would need to change this. Not as fluid, the switching between animations also take time
        //       leading to CurrentAttackIndex being ahead of animations
        private async void ChangeAnimatorClip()
        {
            // _board.CurrentAttackIndex -= EnemyBoard.ALREADY_PLAYING;
            while (true)
            {
                await Task.Delay(10);
                if (_cts.IsCancellationRequested) return;

                if ((_board.CurrentDecisionIndex & EnemyBoard.PLAY_FINISHED) != 0)
                {
                    _board.CurrentDecisionIndex &= ~EnemyBoard.ALREADY_PLAYING;
                    _board.CurrentDecisionIndex &= ~EnemyBoard.PLAY_FINISHED;

                    if (_board.CurrentDecisionIndex < _board.MeleeCombos[_board.SelectedCombo].ComboSequence.Length - 1)
                    {
                        _board.CurrentDecisionIndex++;
                        // Debug.Log($"Changing Clip: {_board.CurrentDecisionIndex}");
                        break;
                    }
                    else
                    {
                        _board.SelectedCombo = (byte)ComboType.DEFAULT;
                        _board.SelectedCombatDecision = (byte)CombatDecision.NOT_DECIDED;
                        _board.CurrentDecisionIndex = EnemyBoard.NO_DECISION;
                        _NodeState = NodeState.IDLE;
                        _board.EnemyAnimator.SetInteger(EnemyBoard.PERFORM_ATTACK, (int)EnemyAttackType.NOT_ATTACKING);
                        _board.EnemyAnimator.SetInteger(EnemyBoard.COMBAT_DECISION, (int)CombatDecision.NOT_DECIDED);
                        _board.EnemyAnimator.SetBool(EnemyBoard.LOCK_ANIMATION, false);
                        break;
                    }
                }
            }
            // Debug.Log($"Clip Changed: {_board.CurrentDecisionIndex}");
        }

        public virtual void CheckAttackConditions() { }
    }
}