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
    public class DialogueNodePortData
    {
        public string BaseNodeGUID;
        public string PortName;
        public string TargetNodeGUID;
    }
}