using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace CurseOfNaga.BehaviourTree
{
    public class PatrolAreaTask : Node
    {
        internal enum PatrolStatus { WAITING = 0, WALKING = 1 }

        private Vector3[] _patrolPoints;
        private Transform _self;
        private float _patrolSpeedMult, _waitTime;

        private PatrolStatus _patrolStatus;
        private byte _currPatrolIndex;
        private const float _STOP_RANGE = 0.2f;

        private CancellationTokenSource _cts;

        ~PatrolAreaTask()
        {
            _cts.Cancel();
        }

        public PatrolAreaTask(Vector3[] patrolPoints, float waitTime)
        {
            _waitTime = waitTime;
            _patrolPoints = patrolPoints;

            _currPatrolIndex = 0;
            _patrolStatus = PatrolStatus.WALKING;
        }

        public override NodeState Evaluate(int currCount)
        {
            _CurrCount = currCount;
            Vector3 dirVec = _patrolPoints[_currPatrolIndex] - _self.position;

            _self.position += dirVec.normalized * Time.deltaTime * _patrolSpeedMult * (int)_patrolStatus;

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