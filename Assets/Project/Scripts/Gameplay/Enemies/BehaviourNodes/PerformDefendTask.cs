#define TESTING_BT

using UnityEngine;

using System.Threading.Tasks;
using System.Threading;

using CurseOfNaga.BehaviourTree;
using static CurseOfNaga.Global.UniversalConstant;

namespace CurseOfNaga.Gameplay.Enemies
{
    [System.Serializable]
    public class PerformDefendTask : Node
    {
#if !TESTING_BT
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

        ~PerformDefendTask() { _cts.Cancel(); }

        public PerformDefendTask(EnemyBoard board)
        {
            _cts = new CancellationTokenSource();
            _board = board;
        }

        /*
        *   - For Defend:
        *       [=] Stop defending when player has stopped atacking
        *       [=] If random defending, then wait for timeout
        */
        public override NodeState Evaluate(int currCount)
        {
            _CurrCount = currCount;

            if (_board.SelectedCombatDecision != (byte)CombatDecision.DEFEND)
            {
                _NodeState = NodeState.FAILURE;
                return NodeState.FAILURE;
            }

            // Defend is already playing 
            if ((_board.CurrentDecisionIndex & EnemyBoard.ALREADY_PLAYING) != 0)
            {
                _NodeState = NodeState.SUCCESS;
                return NodeState.SUCCESS;
            }

            _board.CurrentDecisionIndex |= EnemyBoard.ALREADY_PLAYING;
            _board.EnemyAnimator.SetInteger(EnemyBoard.COMBAT_DECISION, _board.SelectedCombatDecision);
            MakeNewDecision(Random.Range(1f, 3f));

            _NodeState = NodeState.SUCCESS;
            return NodeState.SUCCESS;
        }

        private async void MakeNewDecision(float delayInSec)
        {
            await Task.Delay((int)(delayInSec * 1000));
            if (_cts.IsCancellationRequested) return;

            _NodeState = NodeState.IDLE;
            _board.CurrentDecisionIndex = EnemyBoard.NO_DECISION;
            _board.SelectedCombatDecision = (byte)CombatDecision.NOT_DECIDED;
            _board.EnemyAnimator.SetInteger(EnemyBoard.COMBAT_DECISION, (int)CombatDecision.NOT_DECIDED);
        }

        public virtual void CheckAttackConditions() { }
    }
}