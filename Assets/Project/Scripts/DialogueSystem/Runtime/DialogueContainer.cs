using System.Collections.Generic;
using UnityEngine;

namespace CurseOfNaga.DialogueSystem.Runtime
{
    [System.Serializable]
    public class DialogueContainer : ScriptableObject
    {
        public List<DialogueNodePortData> NodePorts = new List<DialogueNodePortData>();
        public List<DialogueNodeData> NodeDatas = new List<DialogueNodeData>();
    }
}