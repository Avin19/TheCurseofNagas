namespace CurseOfNaga.TestBehaviourTree
{
    [System.Serializable]
    public class DoNode_1 : Node
    {
        public NodeState SetNode
        {
            set
            {
                _NodeState = value;
            }
        }

        public override NodeState Evaluate(int currCount)
        {
            _CurrCount = currCount;
            return _NodeState;
        }
    }
}