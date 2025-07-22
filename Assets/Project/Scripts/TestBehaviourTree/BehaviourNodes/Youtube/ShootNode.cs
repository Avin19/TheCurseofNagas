using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace CurseOfNaga.TestBehaviourTree
{
    public class ShootNode : Node
    {
        public ShootNode()
        {
        }

        public override NodeState Evaluate(int currCount)
        {
            _CurrCount = currCount;
            return NodeState.RUNNING;
        }

    }
}