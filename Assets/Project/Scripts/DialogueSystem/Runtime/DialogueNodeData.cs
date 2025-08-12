namespace CurseOfNaga.DialogueSystem.Runtime
{
    [System.Serializable]
    public class DialogueNodeData
    {
        public string GUID;
        // public string DialogueText;
        public UnityEngine.Vector2 Position;
    }

    [System.Serializable]
    public class DialogueNodeLinkData
    {
        public string BaseNodeGUID;
        public string PortName;
        public string TargetNodeGUID;
    }
}