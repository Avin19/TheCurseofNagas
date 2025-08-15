using UnityEngine;

using CurseOfNaga.DialogueSystem.Runtime;
using CurseOfNaga.Utils;
using CurseOfNaga.Gameplay.Managers;
using static CurseOfNaga.Global.UniversalConstant;

namespace CurseOfNaga.DialogueSystem.Test
{
    //TODO: This will be a non-monobehaviour script
    public class TestDialogueManager : MonoBehaviour
    {
        /*
        * - This will contain all the dialogue information/data to be used in the game
        * - Provide dialogue to display on the screen
        * - This will track all the dialogue status/progress
        *   [=] Ongoing Conversation
        *       {+} Which dialogue index is the NPC / Player at while interacting
        *   [=] Which dialogue options should be available to the Player for interaction
        *   [=] Evaluate conditions to go to the appropriate next dialogue
        * - Recieve NPC decision to talk or not?
        *   [=] Player does not fulfill enough points/requirements to talk to the NPC
        * - 
        */

        private DialogueTemplate _dialogueTemplate;
        // private int _currDialogueIndex = 0;

        // IMP: Assumption made that the NPC will be in the same order as the Dialogue JSON or something defined
        [SerializeField] private int[] _npcDialogueTracker;

        private const string _FILENAME = "Dialogues_SerializeTest.json";
        private const int SET_VAL = 1, DEFAULT_VAL = 1, UNSET_VAL = -1;

        private void OnDisable()
        {
            TestDialogueMainManager.Instance.OnPlayerInteraction -= EvaluateAndLoadDialogue;
        }

        private void OnEnable()
        {
            Invoke(nameof(Initialize), 1f);
            LoadDialoguesJson();
        }

        private void Initialize()
        {
            TestDialogueMainManager.Instance.OnPlayerInteraction += EvaluateAndLoadDialogue;
        }

        public void Initialize(int totalNPCCount)
        {
            _npcDialogueTracker = new int[totalNPCCount];
            TestDialogueMainManager.Instance.OnPlayerInteraction += EvaluateAndLoadDialogue;
        }

        public async void LoadDialoguesJson()
        {
            // fileName += UniversalConstant.JSON_EXTENSION;
            string pathToJson = System.IO.Path.Join(Application.streamingAssetsPath, _FILENAME);
            Debug.Log($"Loadiing Json from: {pathToJson}");

            if (!System.IO.File.Exists(pathToJson))
            {
                Debug.LogError($"Invalid Dialogues JSON file path: {pathToJson}");
                return;
            }

            FileDataHelper fileDataHelper = new FileDataHelper();
            string jsonData = await fileDataHelper.GetFileData_Async(pathToJson);

            _dialogueTemplate = JsonUtility.FromJson<DialogueTemplate>(jsonData);
            // Debug.Log($"Dialogue Template: \n{dialogueTemplate} | jsonData: {jsonData}"); return;         //TEST
        }

        // Track progress of ongoing conversation
        public void EvaluateAndLoadDialogue(InteractionType type, int uid, int npcID)
        {
            if (type != InteractionType.INTERACTING_WITH_NPC) return;

            /*
            * - What can be there to evaluate for a dialogue before presenting
            *   [=] No Condition Check
            *       {+} Conversation is straightforward / back-forth
            *       {+} Go from one node to another untill the end
            *   [=] Conditions need to be checked
            *       {+} Quest In Progress | Hint Needed
            *   [=] Condition one-time Check
            *       {+} Quest In Progress
            *       {+} Quest Completed / Failed
            *   [=] [BOTH] Condition multi/one-time Check
            *       {+} Have conversation with the NPCs that require something first
            */

            DialogueData dialogueData = _dialogueTemplate.characters[npcID].dialogues_list[_npcDialogueTracker[uid]];

            //TODO: Configure this in the future if needed
            //Check dialogue type
            int dialogueType = dialogueData.type;
            switch (dialogueType)
            {
                case (int)DialogueType.SPEECH:
                    break;

                case (int)DialogueType.QUESTION:
                    break;

                case (int)DialogueType.ANSWER:
                    break;

                //We would have to iterate over every choice and check if the requirements are met or not
                case (int)DialogueType.CHOICE:

                    break;

                case 420:
                    //Evaluate the conditions to show correct dialogue
                    int flagToCheck = dialogueData.flags;

                    //Check if the flag is set for the current dialogue
                    if (((int)TestDialogueMainManager.Instance.flag1 & flagToCheck) == 0)
                    {
                        //Check if there are any alternatives to the current dialogue
                        return;
                    }
                    break;
            }

            //Get the dialogue
            string tempString = dialogueData.dialogue;
            TestDialogueMainManager.Instance.OnShowDialogue?.Invoke(tempString);

            //Get the Next Node ID
            if (dialogueData.ports == null)     //Reached End Of Conversation
            {
                TestDialogueMainManager.Instance.OnPlayerInteraction?
                    .Invoke(InteractionType.INTERACTING_WITH_NPC, DEFAULT_VAL, UNSET_VAL);
                return;
            }

            int nextDIndex;
            tempString = dialogueData.ports[0].target_uid;
            int.TryParse(tempString.Substring(6, 3), out nextDIndex);

            // Update tracker value
            _npcDialogueTracker[npcID] = nextDIndex;
        }
    }
}