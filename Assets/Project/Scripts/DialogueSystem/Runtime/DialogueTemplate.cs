using System.Collections.Generic;

namespace CurseOfNaga.DialogueSystem.Runtime
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    [System.Serializable]
    public class CharacterData
    {
        public string parent_id;
        public string character_name;
        public List<DialogueData> dialogues_list;
    }

    [System.Serializable]
    public class DialoguePort
    {
        public string base_uid;
        public string name;
        public string target_uid;

        // public DialogueLink() { dialogue_id = "NOT_SET"; next_dialogue_id = "NOT_SET"; }

        public DialoguePort(string dialogueID = "NOT_SET", string portName = "NOT_SET", string targetDialogueID = "NOT_SET")
        {
            base_uid = dialogueID;
            name = portName;
            target_uid = targetDialogueID;
        }
    }

    [System.Serializable]
    public class DialogueData
    {
        public int flags;
        public int type;
        public int nodeIndex;                   // Store the index of the node in the GraphView | Not needed in JSON
        // public DialoguePort port;
        public string base_uid;
        public string dialogue;
        public List<DialoguePort> ports;

        public DialogueData()
        {
            flags = 0;
            type = 0;
            // port = new DialoguePort();
            base_uid = "NOT_SET";
            ports = new List<DialoguePort>();
            dialogue = "Enter Dialogue";
        }
    }

    [System.Serializable]
    public class DialogueTemplate
    {
        public List<CharacterData> characters;
    }
}