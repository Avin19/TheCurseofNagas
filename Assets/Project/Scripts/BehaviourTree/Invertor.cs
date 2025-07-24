namespace CurseOfNaga.BehaviourTree
{
    public class Invertor : Node
    {
        private Node _baseNode;

        public Invertor(Node node)
        {
            _baseNode = node;
        }

        public override NodeState Evaluate(int currCount)
        {
            _CurrCount = currCount;
            _baseNode.Evaluate(currCount);
            switch (_baseNode.CurrNodeState)
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