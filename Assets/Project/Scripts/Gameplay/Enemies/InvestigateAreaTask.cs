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
        private EnemyBoard _board;

        private float _searchRange;
        private float _speedMult;
        private float _totalSearchDuration, _currDuration;          //Max time enemy will take to search the player
        private Transform _self;
        private Vector3 _startPos, _searchPos;

        private const int _TOTAL_SEARCH_PHASES = 3;
        private const float _MINIMUM_DIFF = 0.5f;

        private CancellationTokenSource _cts;

        ~InvestigateAreaTask()
        {
            _cts.Cancel();
        }

        public InvestigateAreaTask(EnemyBoard board, float searchRange, float totalSearchDuration,
            float investigateSpeedMult, Transform self)
        {
            _cts = new CancellationTokenSource();

            _board = board;
            _searchRange = searchRange;
            _totalSearchDuration = totalSearchDuration;
            _speedMult = investigateSpeedMult;
            _self = self;
            _startPos = self.position;
        }

        public override NodeState Evaluate(int currCount)
        {
            _CurrCount = currCount;

            //Select a random unit vector and move the Enemy along the direction within the searchRadius
            // Vector3 finalPos = SelectRandomPos();

            if ((_board.Status & EnemyStatus.LOST_PLAYER) != 0 && (_board.Status & EnemyStatus.INVESTIGATE_AREA) == 0)
            {
                _board.Status |= EnemyStatus.INVESTIGATE_AREA;

                SelectRandomSearchPos();

                int minSearchDuration = (int)(_totalSearchDuration / _TOTAL_SEARCH_PHASES);
                ChangeSearchPos(Random.Range(minSearchDuration, minSearchDuration + _TOTAL_SEARCH_PHASES));
                // _NodeState = NodeState.RUNNING;
                // return NodeState.RUNNING;
            }
            else if (_currDuration >= _totalSearchDuration)
            {
                _board.Status &= ~EnemyStatus.PLAYER_VISIBLE;
                _board.Status &= ~EnemyStatus.LOST_PLAYER;
                _board.Status &= ~EnemyStatus.INVESTIGATE_AREA;

                _NodeState = NodeState.FAILURE;
                return NodeState.FAILURE;
            }

            Vector3 dirVec = _self.position - _searchPos;
            if (Vector3.SqrMagnitude(dirVec) <= _MINIMUM_DIFF)
            {
                _self.position += dirVec.normalized * Time.deltaTime * _speedMult;
            }

            _NodeState = NodeState.RUNNING;
            return NodeState.RUNNING;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SelectRandomSearchPos()
        {
            Vector2 randomUnitVec = Random.insideUnitCircle;
            _searchPos = _startPos + new Vector3(randomUnitVec.x, 0f, randomUnitVec.y) * Random.Range(_searchRange - 2, _searchRange);
        }

        public async void ChangeSearchPos(int minSearchDuration)
        {
            await Task.Delay(minSearchDuration * 1000);
            if (_cts.IsCancellationRequested || _currDuration >= _totalSearchDuration) return;

            _currDuration += minSearchDuration;
            SelectRandomSearchPos();

            minSearchDuration = (int)(_totalSearchDuration / _TOTAL_SEARCH_PHASES);
            ChangeSearchPos(Random.Range(minSearchDuration, minSearchDuration + _TOTAL_SEARCH_PHASES));
        }
    }
}