#define TESTING_BT

using UnityEngine;

using System.Threading.Tasks;
using System.Threading;

using CurseOfNaga.BehaviourTree;
using static CurseOfNaga.Global.UniversalConstant;

namespace CurseOfNaga.Gameplay.Enemies
{
    [System.Serializable]
    public class PerformStrafeTask : Node
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

        ~PerformStrafeTask() { _cts.Cancel(); }

        public PerformStrafeTask(EnemyBoard board)
        {
            _cts = new CancellationTokenSource();
            _board = board;
        }

        /*
        *   - For Strafe:
        *       [=] Wait for the enemy to be at the position or something
        */
        public override NodeState Evaluate(int currCount)
        {
            _CurrCount = currCount;

            if (_board.SelectedCombatDecision != (byte)CombatDecision.STRAFE)
            {
                _NodeState = NodeState.FAILURE;
                return NodeState.FAILURE;
            }

            // Strafe is already playing
            if ((_board.CurrentDecisionIndex & EnemyBoard.ALREADY_PLAYING) != 0)
            {
                _NodeState = NodeState.SUCCESS;
                return NodeState.SUCCESS;
            }

            _board.CurrentDecisionIndex |= EnemyBoard.ALREADY_PLAYING;
            MakeNewDecision(Random.Range(1f, 3f));

            _NodeState = NodeState.SUCCESS;
            return NodeState.SUCCESS;
        }

        private async void MakeNewDecision(float delayInSec)
        {
            await Task.Delay((int)(delayInSec * 1000));
            if (_cts.IsCancellationRequested) return;

            _board.CurrentDecisionIndex &= ~EnemyBoard.ALREADY_PLAYING;
            _NodeState = NodeState.IDLE;
            _board.SelectedCombatDecision = (byte)CombatDecision.NOT_DECIDED;
        }

        public virtual void CheckAttackConditions() { }
    }
}