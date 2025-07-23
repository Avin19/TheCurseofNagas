using System.Collections.Generic;

namespace CurseOfNaga.BehaviourTree
{
    public class Selector : Node
    {
        protected Node[] _NodeList;

        public Selector(Node[] nodes)
        {
            _NodeList = nodes;
        }

        public override NodeState Evaluate(int currCount)
        {
            _CurrCount = currCount;
            for (int i = 0; i < _NodeList.Length; i++)
            {
                switch (_NodeList[i].Evaluate(currCount))
                {
                    case NodeState.RUNNING:
                        _NodeState = NodeState.RUNNING;
                        return NodeState.RUNNING;

                    case NodeState.SUCCESS:
                        _NodeState = NodeState.SUCCESS;
                        return NodeState.SUCCESS;

                    case NodeState.FAILURE:
                        break;
                }
            }

            _NodeState = NodeState.FAILURE;
            return _NodeState;
        }
    }
}