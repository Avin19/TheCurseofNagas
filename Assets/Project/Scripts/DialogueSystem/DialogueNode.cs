#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;

namespace CurseOfNaga.DialogueSystem
{
    public class DialogueNode : Node
    {
        public string GUID;
        public string DialogueText;
        public bool EntryPoint = false;
    }
}
#endif