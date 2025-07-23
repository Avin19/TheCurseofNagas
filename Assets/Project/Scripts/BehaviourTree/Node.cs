namespace CurseOfNaga.BehaviourTree
{
    public enum NodeState { RUNNING, SUCCESS, FAILURE, }

    [System.Serializable]
    public abstract class Node
    {
        protected int _CurrCount;
        protected NodeState _NodeState;
        public NodeState CurrNodeState { get => _NodeState; }

        public abstract NodeState Evaluate(int currCount);
    }

}