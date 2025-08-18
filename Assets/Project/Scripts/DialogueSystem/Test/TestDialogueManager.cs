using UnityEngine;

using CurseOfNaga.DialogueSystem.Runtime;
using CurseOfNaga.Utils;

using CurseOfNaga.Gameplay.Managers;
using static CurseOfNaga.Global.UniversalConstant;
using System.Collections.Generic;

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
        [SerializeField] private int[] _dialogueTracker;
        private int _charUID, _npcObjUID;      //For tracking the dialogueNodes
        private List<string> _targetNodeIds;

        private const string _FILENAME = "Dialogues_SerializeTest.json";
        private const int SET_VAL = 1, DEFAULT_VAL = 1, UNSET_VAL = -1, _PLAYER_OFFSET = 1;

        private void OnDisable()
        {
            TestDialogueMainManager.Instance.OnPlayerInteraction -= EvaluateAndLoadDialogue;
        }

        private void OnEnable()
        {
            Invoke(nameof(Initialize), 1f);
            LoadDialoguesJson();

            _charUID = UNSET_VAL;
            _npcObjUID = UNSET_VAL;
        }

        private void Initialize()
        {
            _targetNodeIds = new List<string>();
            TestDialogueMainManager.Instance.OnPlayerInteraction += EvaluateAndLoadDialogue;
        }

        public void Initialize(int totalNPCCount)
        {
            _dialogueTracker = new int[totalNPCCount + _PLAYER_OFFSET];
            _targetNodeIds = new List<string>();
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
        public void EvaluateAndLoadDialogue(InteractionType interactionType, int uid, int npcID)
        {
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


            DialogueData dialogueData;
            string tempString;
            int nextDgIndex, nextChIndex;
            bool showChoices = false;

            // Only the player will have choices to select from. For NPC->NPC, the dialogue flow will be in
            // one direction only
            if (interactionType == InteractionType.MADE_CHOICE)
            {
                //Get the current Player Dialogue Node
                _npcObjUID = PLAYER_ID;         //Player ID
                int.TryParse(_targetNodeIds[uid].Substring(0, 3), out _charUID);
                int.TryParse(_targetNodeIds[uid].Substring(6, 3), out nextDgIndex);

                // As the player has answered, there will be only 1 dialogue from the NPC side everytime, as they 
                // do not have choices to make. 
                // TODO: Maybe NPC can choose what to say based on some conditions
                tempString = _dialogueTemplate.characters[_charUID].dialogues_list[nextDgIndex].ports[0].target_uid;
                // Debug.Log($"_targetNodeIds: {_targetNodeIds[uid]} | _charUID: {_charUID} | _npcObjUID: {_npcObjUID}");

                // Extract the Target Dialogue Node to display
                int.TryParse(tempString.Substring(0, 3), out _charUID);
                int.TryParse(tempString.Substring(6, 3), out nextDgIndex);
                _dialogueTracker[_npcObjUID] = nextDgIndex;
                // Debug.Log($"_targetNodeIds: {_targetNodeIds[uid]} | _charUID: {_charUID} | _npcObjUID: {_npcObjUID}");

                //Clear everything as strings will be replaced
                _targetNodeIds.Clear();
            }
            else
            {
                if (interactionType != InteractionType.INTERACTING_WITH_NPC || uid == UNSET_VAL) return;
                //Set the Character ID tracker
                else if (_npcObjUID == UNSET_VAL)
                {
                    _charUID = uid;
                    _npcObjUID = npcID + _PLAYER_OFFSET;
                }
            }

            //TODO: Configure this in the future if needed
            dialogueData = _dialogueTemplate.characters[_charUID].dialogues_list[_dialogueTracker[_npcObjUID]];

            //Get the dialogue
            tempString = dialogueData.dialogue;
            TestDialogueMainManager.Instance.OnShowDialogue?.Invoke(tempString, showChoices);
            Debug.Log($"Shwoing Dialogue | tempString: {tempString} | Ports: {dialogueData.ports.Count}");

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
                    TestDialogueMainManager.Instance.OnPlayerInteraction?
                        .Invoke(InteractionType.INTERACTING_WITH_NPC, UNSET_VAL, -(int)DialogueType.CHOICE);

                    int choicesCount = dialogueData.ports.Count;
                    showChoices = true;

                    //Iterate over each choice and display buttons
                    for (int i = 0; i < choicesCount; i++)
                    {
                        tempString = dialogueData.ports[i].target_uid;
                        _targetNodeIds.Add(tempString);
                        int.TryParse(tempString.Substring(0, 3), out nextChIndex);
                        int.TryParse(tempString.Substring(6, 3), out nextDgIndex);
                        TestDialogueMainManager.Instance.OnShowDialogue
                            ?.Invoke(_dialogueTemplate.characters[nextChIndex].dialogues_list[nextDgIndex].dialogue, showChoices);
                    }

                    return;

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


            //Get the Next Node ID
            //FIXME: JsonUtility does not returns null | Newtonsoft.Json will return null
            // if (dialogueData.ports == null)     //Reached End Of Conversation
            if (dialogueData.ports.Count == 0)     //Reached End Of Conversation
            {
                /* What happens when the conversation reaches EOC [End Of Conversation]
                * - The current dialogueIndex is at the end of a Node
                *   [=] Need to place it in such a place where the next conversation takes place
                *       {+} Normally, the index should be placed to point to the next part of the story
                *       {+} What if, the player has began a quest? In this case:
                *           <~> The player might want to interact again with the NPC to hear about the 
                *               quest one more time, if the player wasnt paying attention (assuming always).
                *           <~> This would require revisiting the dialogue-tree from the start.
                *           <~> There wont be any choices as the NPC has given the Quest and all objectives will be set.
                *           <~> The dialogueIndex should start at the base of that particular Quest's DialogueGraph
                *               [IMP] Need to update once the player has made progress
                *       {+} What if, the player is casually talking to an NPC? In this case:
                *           <~> The NPC has only 1 choice available at this point of story
                *               [;] The NPC should be able to repeat the same set of dialogue for its normal routine
                *           <~> The NPC has multiple choices to select from
                *               [;] Same as above, the player just selects a choice but the flow will be the same
                *           <~> The dialogueIndex should land at the position where it started from.
                *               [IMP] Need to update index once the player has made progress
                */

                TestDialogueMainManager.Instance.OnPlayerInteraction?
                    .Invoke(InteractionType.INTERACTING_WITH_NPC, UNSET_VAL, UNSET_VAL);
                return;
            }

            tempString = dialogueData.ports[0].target_uid;
            int.TryParse(tempString.Substring(6, 3), out nextDgIndex);

            // Update tracker value
            _dialogueTracker[_npcObjUID] = nextDgIndex;
        }
    }
}