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
        public string dialogue_id;
        public string next_dialogue_id;

        // public DialogueLink() { dialogue_id = "NOT_SET"; next_dialogue_id = "NOT_SET"; }

        public DialogueLink(string dialogueID = "NOT_SET", string nextDialogueID = "NOT_SET")
        {
            dialogue_id = dialogueID;
            next_dialogue_id = nextDialogueID;
        }
    }

    [System.Serializable]
    public class DialogueData
    {
        public int dialogue_flags;
        public int dialogue_type;
        public DialogueLink dialogueLink;
        public string dialogue;
        public List<DialogueLink> choices;

        public DialogueData()
        {
            dialogue_flags = 0;
            dialogue_type = 0;
            dialogueLink = new DialogueLink();
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