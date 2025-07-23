namespace CurseOfNaga.BehaviourTree
{
    public class Invertor : Node
    {
        public override NodeState Evaluate(int currCount)
        {
            _CurrCount = currCount;
            switch (_NodeState)
            {
                case NodeState.RUNNING:
                    _NodeState = NodeState.RUNNING;
                    break;

                case NodeState.SUCCESS:
                    _NodeState = NodeState.FAILURE;
                    break;

                case NodeState.FAILURE:
                    _NodeState = NodeState.SUCCESS;
                    break;
            }

            return _NodeState;
        }
    }
}