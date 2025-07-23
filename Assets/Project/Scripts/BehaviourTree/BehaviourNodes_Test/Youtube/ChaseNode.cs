using UnityEngine;
using UnityEngine.AI;

namespace CurseOfNaga.BehaviourTree
{

    public class ChaseNode : Node
    {
        private Transform target, self;

        public ChaseNode(Transform target, Transform self)
        {
            this.target = target;
            this.self = self;
        }

        public override NodeState Evaluate(int currCount)
        {
            _CurrCount = currCount;

            float distance = Vector3.Distance(target.position, self.position);
            if (distance > 0.2f)
            {
                return NodeState.RUNNING;
            }
            else
            {
                return NodeState.SUCCESS;
            }
        }
    }

}