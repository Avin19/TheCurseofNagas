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
    public class DialogueLink
    {
        public string id;
        public string next_id;

        // public DialogueLink() { dialogue_id = "NOT_SET"; next_dialogue_id = "NOT_SET"; }

        public DialogueLink(string dialogueID = "NOT_SET", string nextDialogueID = "NOT_SET")
        {
            id = dialogueID;
            next_id = nextDialogueID;
        }
    }

    [System.Serializable]
    public class DialogueData
    {
        public int flags;
        public int type;
        public DialogueLink link;
        public string dialogue;
        public List<DialogueLink> choices;

        public DialogueData()
        {
            flags = 0;
            type = 0;
            link = new DialogueLink();
            choices = null;
            dialogue = "Enter Dialogue";
        }
    }

    [System.Serializable]
    public class DialogueTemplate
    {
        public List<CharacterData> characters;
    }
}