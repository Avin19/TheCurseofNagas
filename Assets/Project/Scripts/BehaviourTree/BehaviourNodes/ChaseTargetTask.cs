using UnityEngine;

namespace CurseOfNaga.BehaviourTree
{
    public class ChaseTargetTask : Node
    {
        private Transform _self;
        private Transform _target;
        private float _stopRange, _chaseSpeedMult;

        public ChaseTargetTask(Transform self, Transform target, float stopRange, float speedMult)
        {
            _target = target;
            _self = self;
            _stopRange = stopRange;
            _chaseSpeedMult = speedMult;
        }

        public override NodeState Evaluate(int currCount)
        {
            _CurrCount = currCount;
            Vector3 dirVec = _target.position - _self.position;

            _self.transform.position += dirVec.normalized * Time.deltaTime * _chaseSpeedMult;

            if (dirVec.sqrMagnitude <= (_stopRange * _stopRange))
            {
                _NodeState = NodeState.SUCCESS;
                return NodeState.SUCCESS;
            }

            _NodeState = NodeState.RUNNING;
            return NodeState.RUNNING;
        }
    }
}