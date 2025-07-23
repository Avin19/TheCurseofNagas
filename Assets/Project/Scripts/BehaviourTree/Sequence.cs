// #define YOUTUBE_IMPLEMENTATION

using System.Collections.Generic;

namespace CurseOfNaga.BehaviourTree
{
    public class Sequence : Node
    {
        protected Node[] _NodeList;

        public Sequence(Node[] nodes)
        {
            _NodeList = nodes;
        }

        public override NodeState Evaluate(int currCount)
        {
#if YOUTUBE_IMPLEMENTATION
            bool isAnyNodeRunning = false;
#endif
            _CurrCount = currCount;
            for (int i = 0; i < _NodeList.Length; i++)
            {
                switch (_NodeList[i].Evaluate(currCount))
                {
                    case NodeState.RUNNING:
#if YOUTUBE_IMPLEMENTATION
                        isAnyNodeRunning = true;
                        break;
#else
                        _NodeState = NodeState.RUNNING;
                        return NodeState.RUNNING;
#endif

                    case NodeState.SUCCESS:
                        break;

                    case NodeState.FAILURE:
                        _NodeState = NodeState.FAILURE;
                        return NodeState.FAILURE;
                }
            }
#if YOUTUBE_IMPLEMENTATION
            _NodeState = isAnyNodeRunning ? NodeState.RUNNING : NodeState.SUCCESS;
            return _NodeState;
#else
            _NodeState = NodeState.SUCCESS;
            return NodeState.SUCCESS;
#endif
        }
    }
}