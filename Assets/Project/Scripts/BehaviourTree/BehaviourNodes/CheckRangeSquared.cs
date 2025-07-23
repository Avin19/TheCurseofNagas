using UnityEngine;

namespace CurseOfNaga.BehaviourTree
{
    public class CheckRangeSquared : Node
    {
        private Transform _origin;
        private Transform _target;
        private float _range;

        public CheckRangeSquared(Transform origin, Transform target, float range)
        {
            _target = target;
            _origin = origin;
            _range = range;
        }

        public override NodeState Evaluate(int currCount)
        {
            _CurrCount = currCount;

            if (Vector3.SqrMagnitude(_target.position - _origin.position) <= (_range * _range))
            {
                _NodeState = NodeState.SUCCESS;
                return NodeState.SUCCESS;
            }

            _NodeState = NodeState.RUNNING;
            return NodeState.RUNNING;
        }
    }
}