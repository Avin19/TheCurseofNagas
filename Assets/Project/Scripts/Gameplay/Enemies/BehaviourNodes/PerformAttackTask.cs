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

        private const int _ALREADY_PLAYING = 1000;

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

            // An Attack is already playing
            if (_board.CurrentAttackIndex >= _ALREADY_PLAYING)
            {
                _NodeState = NodeState.SUCCESS;
                return NodeState.SUCCESS;
            }

            switch (_board.AttackTypeBase)
            {
                case (byte)EnemyAttackType.MELEE:
                    Debug.Log($"MeleeCombos: {_board.MeleeCombos[_board.SelectedComboIndex]}");
                    EnemyAttackType attackType = (EnemyAttackType)_board.MeleeCombos[_board.SelectedComboIndex]
                                                .ComboSequence[_board.CurrentAttackIndex];
                    _board.CurrentAttackIndex += _ALREADY_PLAYING;
                    switch (attackType)
                    {
                        case EnemyAttackType.NORMAL_M0:
                        case EnemyAttackType.NORMAL_M1:
                        case EnemyAttackType.NORMAL_M2:
                        case EnemyAttackType.NORMAL_M3:
                        case EnemyAttackType.NORMAL_M4:
                        case EnemyAttackType.NORMAL_M5:
                            _board.DamageMultiplier = 1f;
                            _board.EnemyAnimator.SetInteger(EnemyBoard.PERFORM_ATTACK, 3);
                            ChangeAnimatorClip(_board.AnimClipLengths[1]);
                            Debug.Log($"Clip Length: {_board.AnimClipLengths[1]}");

                            /*
                            _board.EnemyAnimator.SetInteger(EnemyBoard.PERFORM_ATTACK, (int)attackType);
                            // Offset of 2 as NORMAL_M0 starts at 3
                            ResetAnimator(_board.AnimClipLengths[(int)attackType
                                - (int)EnemyAttackType.NORMAL_M0 + 1]);
                            */

                            break;

                        case EnemyAttackType.HEAVY_M0:
                        case EnemyAttackType.HEAVY_M1:
                        case EnemyAttackType.HEAVY_M2:
                        case EnemyAttackType.HEAVY_M3:
                        case EnemyAttackType.HEAVY_M4:
                        case EnemyAttackType.HEAVY_M5:
                            _board.DamageMultiplier = 1.5f;
                            _board.EnemyAnimator.SetInteger(EnemyBoard.PERFORM_ATTACK, 13);
                            ChangeAnimatorClip(_board.AnimClipLengths[2]);

                            /*
                            _board.EnemyAnimator.SetInteger(EnemyBoard.PERFORM_ATTACK, (int)attackType);
                            // Offset of 12 as HEAVY_M0 starts at 13 + Offset of Normal Attacks
                            ResetAnimator(_board.AnimClipLengths[(int)attackType - (int)EnemyAttackType.HEAVY_M0
                                + 1 + (int)EnemyAttackType.NORMAL_M5]);
                            */

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
        private async void ChangeAnimatorClip(float delayInSec)
        {
            await Task.Delay((int)(delayInSec * 1000));
            if (_cts.IsCancellationRequested) return;

            _board.CurrentAttackIndex -= _ALREADY_PLAYING;

            if (_board.CurrentAttackIndex < 4)
                _board.CurrentAttackIndex++;
            else
            {
                _board.SelectedComboIndex = 255;
                _board.EnemyAnimator.SetInteger(EnemyBoard.PERFORM_ATTACK, (int)EnemyAttackType.NOT_ATTACKING);
            }
        }

        public virtual void CheckAttackConditions() { }
    }
}