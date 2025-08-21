using System.Collections.Generic;
using CurseOfNaga.Global;

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

        public DialoguePort(string dialogueID = UniversalConstant.NOT_SET_STR, string portName = UniversalConstant.NOT_SET_STR
            , string targetDialogueID = UniversalConstant.NOT_SET_STR)
        {
            base_uid = dialogueID;
            name = portName;
            target_uid = targetDialogueID;
        }
    }

    [System.Serializable]
    public struct Vector2Serialized
    {
        public float x, y;
        public Vector2Serialized(int x, int y) { this.x = x; this.y = y; }
        public Vector2Serialized(UnityEngine.Vector2 vec2) { x = vec2.x; y = vec2.y; }
    }

    [System.Serializable]
    public class DialogueData
    {
        public string flags;
        public int type;
        public int nodeIndex;                   // Store the index of the node in the GraphView | Not needed in JSON
        public int quest_index;
        // public DialoguePort port;
        public string base_uid;                 //Transform each character to int spaced at 5 bits?
        public string dialogue;
        public List<DialoguePort> ports;
        public Vector2Serialized position;

        public DialogueData()
        {
            flags = "";
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

        public Dictionary<string, CharacterData> CharactersDict;
        public void FillDictionary()
        {
            characters = null;
            CharactersDict = new Dictionary<string, CharacterData>();

            int charactersCount = characters.Count;
            for (int i = 0; i < charactersCount; i++)
                CharactersDict.Add(characters[i].parent_id, characters[i]);
        }
    }

    public static class DialogueExtensions
    {
        public static UnityEngine.Vector2 ToVec2(this Vector2Serialized vec2S)
        {
            return new UnityEngine.Vector2(vec2S.x, vec2S.y);
        }

        public static Vector2Serialized ToVec2Srlz(this UnityEngine.Vector2 vec2)
        {
            return new Vector2Serialized(vec2);
        }
    }
}