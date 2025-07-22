using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CurseOfNaga.TestBehaviourTree
{
    public class RangeNode : Node
    {
        private float range;
        private Transform target;
        private Transform self;

        public RangeNode(float range, Transform target, Transform self)
        {
            this.range = range;
            this.target = target;
            this.self = self;
        }

        public override NodeState Evaluate(int currCount)
        {
            _CurrCount = currCount;

            float distance = Vector3.Distance(target.position, self.position);
            return distance <= range ? NodeState.SUCCESS : NodeState.FAILURE;
        }
    }
}