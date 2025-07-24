#define TESTING_BT

using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace CurseOfNaga.BehaviourTree
{
    [System.Serializable]
    public class PatrolAreaTask : Node
    {
        internal enum PatrolStatus { WAITING = 0, WALKING = 1 }

#if !TESTING_BT
        private Vector3[] _patrolPoints;
        private Transform _self;
        private float _speedMult, _waitTime;
#else
        public Vector3[] _patrolPoints;
        public Transform _self;
        public float _speedMult, _waitTime;
#endif

        private PatrolStatus _patrolStatus;
        private byte _currPatrolIndex;
        private const float _STOP_RANGE = 0.2f;

        private CancellationTokenSource _cts;

        ~PatrolAreaTask()
        {
            _cts.Cancel();
        }

#if TESTING_BT
        public PatrolAreaTask()
        {
            _cts = new CancellationTokenSource();
            _currPatrolIndex = 0;
            _patrolStatus = PatrolStatus.WALKING;
        }
#endif

        public PatrolAreaTask(Transform self, Vector3[] patrolPoints, float patrolSpeedMult, float waitTime)
        {
            _cts = new CancellationTokenSource();

            _self = self;
            _waitTime = waitTime;
            _patrolPoints = patrolPoints;
            _speedMult = patrolSpeedMult;

            _currPatrolIndex = 0;
            _patrolStatus = PatrolStatus.WALKING;
        }

        public override NodeState Evaluate(int currCount)
        {
            _CurrCount = currCount;
            Vector3 dirVec = _patrolPoints[_currPatrolIndex] - _self.position;

            _self.position += dirVec.normalized * Time.deltaTime * _speedMult * (int)_patrolStatus;

            if (_patrolStatus == PatrolStatus.WALKING && dirVec.sqrMagnitude <= (_STOP_RANGE * _STOP_RANGE))
            {
                _patrolStatus = PatrolStatus.WAITING;
                WaitAtPlace();
            }

            _NodeState = NodeState.RUNNING;
            return NodeState.RUNNING;
        }

        private async void WaitAtPlace()
        {
            await Task.Delay((int)(_waitTime * 1000));
            if (_cts.IsCancellationRequested) return;

            _patrolStatus = PatrolStatus.WALKING;
            _currPatrolIndex = ((_currPatrolIndex + 1) >= _patrolPoints.Length) ? (byte)0 : (byte)(_currPatrolIndex + 1);
        }
    }
}