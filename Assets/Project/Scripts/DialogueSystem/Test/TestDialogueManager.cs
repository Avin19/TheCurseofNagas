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
        private int _currDialogueIndex = 0;

        // IMP: Assumption made that the NPC will be in the same order as the Dialogue JSON or something defined
        [SerializeField] private int[] _npcDialogueTracker;

        private const string _FILENAME = "Dialogues_SerializeTest.json";

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

            //Evaluate the conditions to show correct dialogue
            //Get the dialogue
            string dialogue = _dialogueTemplate.characters[npcID].dialogues_list[_npcDialogueTracker[uid]].dialogue;
            TestDialogueMainManager.Instance.OnShowDialogue?.Invoke(dialogue);

            // Update tracker value
            _npcDialogueTracker[npcID] = _currDialogueIndex;
        }
    }
}