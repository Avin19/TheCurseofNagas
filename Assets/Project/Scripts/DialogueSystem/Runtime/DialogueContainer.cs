using System.Collections.Generic;
using UnityEngine;

namespace CurseOfNaga.DialogueSystem.Runtime
{
    [System.Serializable]
    public class DialogueContainer : ScriptableObject
    {
        public List<DialogueNodeLinkData> NodeLinks = new List<DialogueNodeLinkData>();
        public List<DialogueNodeData> DialogueNodeDatas = new List<DialogueNodeData>();
    }
}