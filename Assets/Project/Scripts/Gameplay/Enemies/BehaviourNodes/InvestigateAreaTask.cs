#define TESTING_BT

using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;

using CurseOfNaga.BehaviourTree;
using static CurseOfNaga.Global.UniversalConstant;

namespace CurseOfNaga.Gameplay.Enemies
{
    [System.Serializable]
    public class InvestigateAreaTask : Node
    {
        private EnemyBoard _board;

#if !TESTING_BT
        private float _searchRange, _speedMult;
        private float _totalSearchDuration;          //Max time enemy will spend to search the player
        private Transform _self;
#else
        public float _searchRange, _speedMult;
        public float _totalSearchDuration;
        public Transform _self;
#endif

        private float _currDuration;
        private Vector3 _startPos, _searchPos;

        private const int _TOTAL_SEARCH_PHASES = 3;             //3 direction search | Toggle atleast 3 times during searching
        private const float _MINIMUM_DIFF = 0.5f;
        private const float _MINIMUM_SEARCH_DURATION = 0.5f;

        private CancellationTokenSource _cts;

#if TESTING_BT
        public void Initialize(EnemyBoard board)
        {
            _cts = new CancellationTokenSource();

            _board = board;
        }
#endif

        ~InvestigateAreaTask()
        {
            _cts.Cancel();
        }

        public InvestigateAreaTask(Transform self, EnemyBoard board, float searchRange, float totalSearchDuration,
            float investigateSpeedMult)
        {
            _cts = new CancellationTokenSource();

            _board = board;
            _searchRange = searchRange;
            _totalSearchDuration = totalSearchDuration;
            _speedMult = investigateSpeedMult;
            _self = self;
        }

        private Vector3 dirVec;         //TEST
        public override NodeState Evaluate(int currCount)
        {
            _CurrCount = currCount;

            //Select a random unit vector and move the Enemy along the direction within the searchRadius
            // Vector3 finalPos = SelectRandomPos();

            if ((_board.Status & EnemyStatus.LOST_PLAYER) != 0 && (_board.Status & EnemyStatus.INVESTIGATE_AREA) == 0)
            {
                _board.Status |= EnemyStatus.INVESTIGATE_AREA;

                _startPos = _self.position;
                SelectRandomSearchPos();

                int minSearchDuration = (int)(_totalSearchDuration / _TOTAL_SEARCH_PHASES);                 //Min search duration for each direction
                ChangeSearchPos(Random.Range(minSearchDuration, minSearchDuration + _TOTAL_SEARCH_PHASES));
                // _NodeState = NodeState.RUNNING;
                // return NodeState.RUNNING;
            }
            else if (_currDuration >= _totalSearchDuration)
            {
                ResetValues();

                _NodeState = NodeState.FAILURE;
                return NodeState.FAILURE;
            }

            // Vector3
            dirVec = _self.position - _searchPos;
            if (Vector3.SqrMagnitude(dirVec) >= _MINIMUM_DIFF)
            {
                _self.position += dirVec.normalized * Time.deltaTime * _speedMult;
            }

            _NodeState = NodeState.RUNNING;
            return NodeState.RUNNING;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResetValues()
        {
            _board.Status &= ~EnemyStatus.PLAYER_VISIBLE;
            _board.Status &= ~EnemyStatus.LOST_PLAYER;
            _board.Status &= ~EnemyStatus.INVESTIGATE_AREA;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SelectRandomSearchPos()
        {
            Vector2 randomUnitVec;

            //Ranomd point on the unit circle
            float randomAngle = Random.Range(0, 360);
            randomUnitVec.x = Mathf.Cos(randomAngle);
            randomUnitVec.y = Mathf.Sin(randomAngle);

            _startPos = _self.position;
            _searchPos = _startPos + new Vector3(randomUnitVec.x, 0f, randomUnitVec.y)
                    * Random.Range(_MINIMUM_SEARCH_DURATION, _searchRange);
        }

        public async void ChangeSearchPos(int minSearchDuration)
        {
            await Task.Delay(minSearchDuration * 1000);
            if (_cts.IsCancellationRequested || _currDuration >= _totalSearchDuration)
            {
                _currDuration = 0;
                return;
            }

            _currDuration += minSearchDuration;
            SelectRandomSearchPos();

            minSearchDuration = (int)(_totalSearchDuration / _TOTAL_SEARCH_PHASES);
            ChangeSearchPos(Random.Range(minSearchDuration, minSearchDuration + _TOTAL_SEARCH_PHASES));
        }
    }
}