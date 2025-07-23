using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;

using CurseOfNaga.BehaviourTree;
using static CurseOfNaga.Global.UniversalConstant;

namespace CurseOfNaga.Gameplay.Enemies
{
    public class InvestigateAreaTask : Node
    {
        private EnemyStatus _enemyStatus;

        private float _searchRange;
        private float _totalSearchDuration, _currDuration;          //Max time enemy will take to search the player
        private int _totalSearchPhase;
        private Transform _self;
        private Vector3 _startPos, _searchPos;

        private CancellationTokenSource _cts;

        ~InvestigateAreaTask()
        {
            _cts.Cancel();
        }

        public InvestigateAreaTask(ref EnemyStatus status, float searchRange, float totalSearchDuration, Transform self)
        {
            _enemyStatus = status;
            _searchRange = searchRange;
            _totalSearchDuration = totalSearchDuration;
            _self = self;
            _startPos = self.position;
        }

        public override NodeState Evaluate(int currCount)
        {
            _CurrCount = currCount;

            //Select a random unit vector and move the Enemy along the direction within the searchRadius
            // Vector3 finalPos = SelectRandomPos();

            if ((_enemyStatus & EnemyStatus.LOST_PLAYER) != 0)
            {
                _enemyStatus |= EnemyStatus.INVESTIGATE_AREA;

                _searchPos = SelectRandomPos();
                // _NodeState = NodeState.RUNNING;
                // return NodeState.RUNNING;
            }
            else if (_currDuration >= _totalSearchDuration)
            {

            }



            _NodeState = NodeState.RUNNING;
            return NodeState.RUNNING;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 SelectRandomPos()
        {
            Vector2 randomUnitVec = Random.insideUnitCircle;
            return _startPos + new Vector3(randomUnitVec.x, 0f, randomUnitVec.y) * Random.Range(_searchRange - 2, _searchRange);
        }

        public async void ChangeSearchPos(int searchDuration)
        {
            await Task.Delay(searchDuration * 1000);
            if (_cts.IsCancellationRequested) return;

            _currDuration += searchDuration;
        }
    }
}