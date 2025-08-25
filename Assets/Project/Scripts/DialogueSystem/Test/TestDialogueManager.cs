// #define DEBUG_1
// #define DEBUG_2

using UnityEngine;

using CurseOfNaga.DialogueSystem.Runtime;
using CurseOfNaga.Utils;

using static CurseOfNaga.Global.UniversalConstant;
using System.Collections.Generic;
using CurseOfNaga.QuestSystem;

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

        // ==========================================> TODO: NEED TO BE SAVED <===================================
        // IMP: Assumption made that the NPC will be in the same order as the Dialogue JSON or something defined
        [SerializeField] private int[] _dialogueTracker;
        // ==========================================> NEED TO BE SAVED <===================================
        [SerializeField] private int[] _dialogueTrackerPrev;
        private int _currCharUID, _invokedNpcObjUID, _currNpcObjUID;      //For tracking the dialogueNodes
        private List<string> _targetNodeIds;

        private const string _FILENAME = "Dialogues_SerializeTest.json";
        private const string _EMPTY_STR = "";
        private const int _PLAYER_OFFSET = 1;
        private const int _SHOW_MAIN_DIALOGUE = 1000;
        private const int _QUEST_QCCEPTED = 0, _QUEST_DECLINED = 1;

        private void OnDisable()
        {
            TestDialogueMainManager.Instance.OnPlayerInteraction -= EvaluateAndLoadDialogue;
            TestDialogueMainManager.Instance.OnQuestUpdate -= UpdateQuestDialogue;
            TestDialogueMainManager.Instance.OnDialogueUpdateRequested -= CheckForAvailableQuests;
        }

        private void OnEnable()
        {
            Invoke(nameof(Initialize), 1f);
            LoadDialoguesJson();

            _currCharUID = _DEFAULT_VAL;
            _currNpcObjUID = _DEFAULT_VAL;
        }

        private void Initialize()
        {
            _targetNodeIds = new List<string>();
            TestDialogueMainManager.Instance.OnPlayerInteraction += EvaluateAndLoadDialogue;
            TestDialogueMainManager.Instance.OnQuestUpdate += UpdateQuestDialogue;
            TestDialogueMainManager.Instance.OnDialogueUpdateRequested += CheckForAvailableQuests;
        }

        public void Initialize(int totalNPCCount)
        {
            _dialogueTracker = new int[totalNPCCount + _PLAYER_OFFSET];
            _dialogueTrackerPrev = new int[totalNPCCount + _PLAYER_OFFSET];
            _targetNodeIds = new List<string>();
            TestDialogueMainManager.Instance.OnPlayerInteraction += EvaluateAndLoadDialogue;
            TestDialogueMainManager.Instance.OnQuestUpdate += UpdateQuestDialogue;
            TestDialogueMainManager.Instance.OnDialogueUpdateRequested += CheckForAvailableQuests;
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

        private void CheckForAvailableQuests(string completedQuestID)
        {
            // Some Main Quest complete | Update the _dialogueTracker for all characters that unlocked new nodes
            // This will be to provide unlocked quest choices
            for (int i = 1; i < _dialogueTemplate.characters.Count; i++)
            {
                // Search for dialogueNodes which can be unlocked by the character
            }

            // Some SubQuest/SideQuest got accepted | Update _dialogueTracker
            // This will be to say dialogue relevant to ongoing quest



            // int gpIndex, qtIndex;

            // int.TryParse(idVal.Substring(GROUP_INDEX_START, GROUP_INDEX_LENGTH), out gpIndex);
            // int.TryParse(idVal.Substring(QUEST_INDEX_START, QUEST_INDEX_LENGTH), out qtIndex);

            // _requestedQuestIndex = gpIndex * (int)Mathf.Pow(10, QUEST_INDEX_LENGTH);
            // _requestedQuestIndex += qtIndex;
        }

        //Player has accepted the quest | Quest other than MAIN_QUEST
        private void UpdateQuestDialogue(string idVal, QuestStatus status, int questIndex)
        {
            if (_targetNodeIds.Count == 0 || status <= QuestStatus.REQUESTED_INFO) return;

            int nextDgIndex = 0;
            string tempString;
            if (status == QuestStatus.ACCEPTED)
            {
                // Extract the Target Dialogue Node to display
                int.TryParse(_targetNodeIds[_QUEST_QCCEPTED].Substring(0, 3), out _currCharUID);
                int.TryParse(_targetNodeIds[_QUEST_QCCEPTED].Substring(6, 3), out nextDgIndex);
            }
            else if (status == QuestStatus.DECLINED)
            {
                // Extract the Target Dialogue Node to display
                int.TryParse(_targetNodeIds[_QUEST_DECLINED].Substring(0, 3), out _currCharUID);
                int.TryParse(_targetNodeIds[_QUEST_DECLINED].Substring(6, 3), out nextDgIndex);
            }

            tempString = _dialogueTemplate.characters[_currCharUID].dialogues_list[nextDgIndex].ports[0].target_uid;
            int.TryParse(tempString.Substring(0, 3), out _currCharUID);
            int.TryParse(tempString.Substring(6, 3), out nextDgIndex);

            _dialogueTracker[_currNpcObjUID] = nextDgIndex;

            TestDialogueMainManager.Instance.CurrPlayerStatus &= ~PlayerStatus.MAKING_CHOICE;

            EvaluateAndLoadDialogue(InteractionType.INTERACTING_WITH_NPC, _SET_VAL, _DEFAULT_VAL);
            _targetNodeIds.Clear();
        }

        // private string[] choiceFlags; // DEBUG
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

            if (interactionType == InteractionType.FINISHING_INTERACTION)
            {
                _targetNodeIds.Clear();
                return;
            }

            //TODO: Check if a new quest is available

            DialogueData dialogueData;
            string tempString;
            int nextDgIndex, nextChIndex;
            bool showChoices = false;

            // Only the player will have choices to select from. For NPC->NPC, the dialogue flow will be in
            // one direction only
            if (interactionType == InteractionType.MADE_CHOICE)
            {
                //Get the current Player Dialogue Node
                // _npcObjUID = PLAYER_ID;         //Player ID
                int.TryParse(_targetNodeIds[uid].Substring(0, 3), out _currCharUID);
                int.TryParse(_targetNodeIds[uid].Substring(6, 3), out nextDgIndex);

                // As the player has answered, there will be only 1 dialogue from the NPC side everytime, as they 
                // do not have choices to make. 
                // TODO: Maybe NPC can choose what to say based on some conditions

                //For quest just store the current ID until the player has chosen whether to accept or not
                if (_dialogueTemplate.characters[_currCharUID].dialogues_list[nextDgIndex].type
                    == (int)DialogueType.QUEST)
                {
                    _dialogueTracker[PLAYER_ID] = nextDgIndex;              //FIXME: Is this really needed?
                }
                else
                {
                    TestDialogueMainManager.Instance.CurrPlayerStatus &= ~PlayerStatus.MAKING_CHOICE;
                    tempString = _dialogueTemplate.characters[_currCharUID].dialogues_list[nextDgIndex].ports[0].target_uid;
                    // Debug.Log($"_targetNodeIds: {_targetNodeIds[uid]} | _charUID: {_charUID} | _npcObjUID: {_npcObjUID}");

                    // Extract the Target Dialogue Node to display
                    int.TryParse(tempString.Substring(0, 3), out _currCharUID);
                    int.TryParse(tempString.Substring(6, 3), out nextDgIndex);
                    _dialogueTracker[PLAYER_ID] = nextDgIndex;              //FIXME: Is this really needed?
#if DEBUG_1
                Debug.Log($"_targetNodeIds: {_targetNodeIds[uid]} | _charUID: {_charUID} | _npcObjUID: {PLAYER_ID}");
#endif

                    //Clear everything as strings will be replaced
                    _targetNodeIds.Clear();
                }
                dialogueData = _dialogueTemplate.characters[_currCharUID].dialogues_list[_dialogueTracker[PLAYER_ID]];
            }
            else
            {
                if (interactionType != InteractionType.INTERACTING_WITH_NPC || uid == _DEFAULT_VAL) return;
                //Set the Character ID tracker for the first time
                else if (_currNpcObjUID == _DEFAULT_VAL)
                {
                    _currCharUID = uid;
                    _invokedNpcObjUID = npcID + _PLAYER_OFFSET;
                    _currNpcObjUID = _invokedNpcObjUID;
                }
                //Turn off Dialogue once finished
                else if (_currCharUID == _DEFAULT_VAL)
                {
                    _currNpcObjUID = _DEFAULT_VAL;

                    TestDialogueMainManager.Instance.OnPlayerInteraction?
                        .Invoke(InteractionType.FINISHING_INTERACTION, _SET_VAL, _DEFAULT_VAL);
                    return;
                }

                //FIXME: _currNpcObjUID is to track multiple instances of the same character
                // When using goons to attack player | They will have same set of dialogues
                dialogueData = _dialogueTemplate.characters[_currCharUID].dialogues_list[_dialogueTracker[_currNpcObjUID]];
            }

            //TODO: Configure this in the future if needed

            //Get the dialogue
            tempString = dialogueData.dialogue;
            // TestDialogueMainManager.Instance.OnShowDialogue?.Invoke(tempString, showChoices);
#if DEBUG_1
            Debug.Log($"Shwoing Dialogue | tempString: {tempString} | Ports: {dialogueData.ports.Count}");
#endif

            //Check dialogue type
            int dialogueType = dialogueData.type;
            switch (dialogueType)
            {
                case (int)DialogueType.SPEECH:
                case (int)DialogueType.QUESTION:
                case (int)DialogueType.ANSWER:
                    TestDialogueMainManager.Instance.OnShowDialogue?.Invoke(tempString, showChoices);
                    break;

                //We would have to iterate over every choice and check if the requirements are met or not
                case (int)DialogueType.CHOICE:
                    // TestDialogueMainManager.Instance.OnPlayerInteraction?
                    //     .Invoke(InteractionType.INTERACTING_WITH_NPC, UNSET_VAL, -(int)DialogueType.CHOICE);
                    // TestDialogueMainManager.Instance.OnShowDialogue?.Invoke(tempString, showChoices);

                    int choicesCount = dialogueData.ports.Count;
                    TestDialogueMainManager.Instance.CurrPlayerStatus |= PlayerStatus.MAKING_CHOICE;

                    string[] choiceFlags;
                    string choiceString;
                    int choiceIndex = 0, chFlagsIndex;
                    //Iterate over each choice and display buttons
                    for (; choiceIndex < choicesCount; choiceIndex++)
                    {
                        choiceFlags = null;
                        choiceString = dialogueData.ports[choiceIndex].target_uid;
                        int.TryParse(choiceString.Substring(0, 3), out nextChIndex);
                        int.TryParse(choiceString.Substring(6, 3), out nextDgIndex);

                        if (_dialogueTemplate.characters[nextChIndex].dialogues_list[nextDgIndex]
                                .type == (int)DialogueType.QUEST_INFO)
                        {
                            //Check if the NPC has any quest available to give to Player

                        }
                        // Check if the flags are empty or not
                        else if (!_dialogueTemplate.characters[nextChIndex].dialogues_list[nextDgIndex]
                                .flags.Equals(_EMPTY_STR))
                        {
                            //Get all the flags for the choice
                            choiceFlags = _dialogueTemplate.characters[nextChIndex].dialogues_list[nextDgIndex]
                                .flags.Split('|');

                            //Check for the condition here
                            for (chFlagsIndex = 0; chFlagsIndex < choiceFlags.Length; chFlagsIndex++)
                            {
                                if (!TestDialogueMainManager.Instance.CompletedFlags.Contains(choiceFlags[chFlagsIndex]))
                                    break;
                            }
#if DEBUG_2
                            Debug.Log($"choiceIndex: {choiceIndex} | chFlagsIndex: {chFlagsIndex}");
#endif
                            //If all flags are not present then (1) Skip the dialogue? or (2) Grey out dialogue
                            if (chFlagsIndex != choiceFlags.Length) continue;
                        }

                        _targetNodeIds.Add(choiceString);
                        TestDialogueMainManager.Instance.OnShowDialogue
                            ?.Invoke(_dialogueTemplate.characters[nextChIndex].dialogues_list[nextDgIndex].dialogue, true);

                    }
                    //showChoices should not matter as the main dialogue should be shown no matter what along with the choices so something can be done here
                    TestDialogueMainManager.Instance.OnShowDialogue?.Invoke(tempString, false);
                    return;

                // Accepting quest from NPC
                case (int)DialogueType.QUEST:
                    //get quest id and send it back as it is
                    TestDialogueMainManager.Instance.OnQuestUpdate?.Invoke(dialogueData.quest_uid,
                        QuestStatus.REQUESTED, _DEFAULT_VAL);
                    return;

                //Shifting End logic here
                case (int)DialogueType.END:
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
                        *           <~> The Player has multiple choices to select from to enquire from NPC
                        *               [;] Same as above, the player just selects a choice but the flow will be the same
                        *           <~> The dialogueIndex should land at the position where it started from at the point of muliple choices
                        *               [IMP] Need to update index once the player has made progress
                        */

                        _currCharUID = _DEFAULT_VAL;

                        // Normal COnversation and Casual Conversation should be equal
                        // The target-id can be defined in the JSON only
                        tempString = dialogueData.ports[0].target_uid;
                        int.TryParse(tempString.Substring(6, 3), out nextDgIndex);

                        // Quest Conversation Logic


                        // Update tracker value
                        _dialogueTracker[_currNpcObjUID] = nextDgIndex;

                        return;
                    }

                case _SHOW_MAIN_DIALOGUE:
                    TestDialogueMainManager.Instance.OnShowDialogue?.Invoke(tempString, showChoices);

                    break;

                case 420:
                    // Evaluate the conditions to show correct dialogue
                    // int flagToCheck = dialogueData.flags;

                    // //Check if the flag is set for the current dialogue
                    // if (((int)TestDialogueMainManager.Instance.flag1 & flagToCheck) == 0)
                    // {
                    //     //Check if there are any alternatives to the current dialogue
                    //     return;
                    // }
                    break;
            }

            //Get the Next Node ID
            //FIXME: JsonUtility does not returns null | Newtonsoft.Json will return null
            // if (dialogueData.ports == null)     //Reached End Of Conversation
            // if (dialogueData.ports.Count == 0)     //Reached End Of Conversation
            // {

            //     _charUID = UNSET_VAL;
            //     // TestDialogueMainManager.Instance.OnPlayerInteraction?
            //     //     .Invoke(InteractionType.INTERACTING_WITH_NPC, UNSET_VAL, UNSET_VAL);
            //     return;
            // }

            tempString = dialogueData.ports[0].target_uid;
            int.TryParse(tempString.Substring(0, 3), out _currCharUID);
            int.TryParse(tempString.Substring(6, 3), out nextDgIndex);

            // Update tracker value
            _dialogueTracker[_currNpcObjUID] = nextDgIndex;
        }
    }
}