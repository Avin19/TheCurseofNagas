using System.Linq;
using CurseOfNaga.QuestSystem;
using UnityEngine;
using UnityEngine.UI;

using static CurseOfNaga.Global.UniversalConstant;

namespace CurseOfNaga.DialogueSystem.Test
{
    public class TestDialogueSystemCanvas : MonoBehaviour
    {
        // internal enum ChoiceType { NO_CHOICE = 0, CHOICE_ACTIVATED = 1, SHOWING_CHOICES = 50, CHOICE_CLICKED = 100 }

        [SerializeField] private GameObject _dialogueRect;
        [SerializeField] private TMPro.TMP_Text _dialogueTxt;
        [SerializeField] private Button[] _dialogueChoiceBts;
        [SerializeField] private TMPro.TMP_Text[] _dialogueChoicesTxt;

        //==============================================> TODO: Optimize <==============================================
        [SerializeField] private GameObject _dialogueChoiceRect, _questChoicesRect;
        [SerializeField] private Button[] _questChoiceBts;              //0: Main Quest | 1: Sub-Main Quest | 2: Side Quest
        [SerializeField] private TMPro.TMP_Text[] _questChoicesTxt;
        private string[] _btQuestChoiceTracker;        // Main | Sub-Main | Side Quests
        private string[] _playerChoicesStr;
        private int _currQtChoiceIndex;
        private const int _PLAYER_CHOICES_COUNT = 3;
        //==============================================> TODO: Optimize <==============================================

        private int _currDialogueIndex;
        private InteractionType _prevInteractionType;
        // private const int _SET_VAL = 1, _UNSET_VAL = 0, _DEFAULT_VALUE = -1;
        private const string _EMPTY_STR = "";

        private void OnDisable()
        {
            TestDialogueMainManager.Instance.OnPlayerInteraction -= UpdateUIForInteraction;
            TestDialogueMainManager.Instance.OnShowDialogue -= UpdateDialogueText;
            TestDialogueMainManager.Instance.OnRequestShowQuestChoiceBt -= ShowQuestChoice;
            TestDialogueMainManager.Instance.OnRequestUpdateQuestChoice -= UpdateQuestChoice;
        }

        private void OnEnable()
        {
            Invoke(nameof(Initialize), 1.5f);
        }

        private void Initialize()
        {
            TestDialogueMainManager.Instance.OnPlayerInteraction += UpdateUIForInteraction;
            TestDialogueMainManager.Instance.OnShowDialogue += UpdateDialogueText;
            TestDialogueMainManager.Instance.OnRequestShowQuestChoiceBt += ShowQuestChoice;
            TestDialogueMainManager.Instance.OnRequestUpdateQuestChoice += UpdateQuestChoice;

            for (int i = 0; i < _dialogueChoiceBts.Length - 1; i++)
            {
                int tempIndex = i;
                _dialogueChoiceBts[i].onClick.AddListener(() => ChoseDialogue(tempIndex));
            }
            _dialogueChoiceBts[3].onClick.AddListener(() => ShowQuestRect(true));

            _btQuestChoiceTracker = new string[_questChoiceBts.Length];
            _currQtChoiceIndex = _DEFAULT_VAL;
            _playerChoicesStr = new string[_PLAYER_CHOICES_COUNT];
            for (int i = 0; i < _questChoiceBts.Length; i++)
            {
                int tempIndex = i;
                _questChoiceBts[i].onClick.AddListener(() => ClickedOnQuestChoice(tempIndex));
            }
        }

        private void ShowQuestChoice()
        {
            _dialogueChoiceBts[3].gameObject.SetActive(true);
        }

        private void ShowQuestRect(bool status)
        {
            _questChoicesRect.SetActive(status);
            _dialogueChoiceRect.SetActive(!status);
        }

        private void UpdateQuestChoice(string dialogueTxt, int questInfo, string baseId)
        {
            //Only update if the choice is requested to show available quests
            // if ((questInfo / _STATUS_OFFSET) != (int)QuestStatus.AVAILABLE) return;

            switch (questInfo / _STATUS_OFFSET)
            {
                //Only update if the choice is requested to show available quests
                case (int)QuestStatus.AVAILABLE:
                    _questChoiceBts[questInfo % _STATUS_OFFSET].gameObject.SetActive(true);
                    _questChoicesTxt[questInfo % _STATUS_OFFSET].text = dialogueTxt;
                    _btQuestChoiceTracker[questInfo % _STATUS_OFFSET] = baseId;

                    break;

                case (int)QuestStatus.LOAD_DEFAULT:
                    _playerChoicesStr[questInfo % _STATUS_OFFSET] = dialogueTxt;

                    break;
            }
        }

        //This can receive Main | Sub-Main | Side Quests
        //TODO: Replace index with constants | Change QuestStatus enum values
        private void ClickedOnQuestChoice(int btIndex)
        {
            //Check whether [Tell More] / [Not Now] is selected
            if (_currQtChoiceIndex != _DEFAULT_VAL)
            {
                int questStatus;

                //FIXME: This is not fully accepted
                if (btIndex == 1)           //Player Accepted to tell more                
                    questStatus = (int)QuestStatus.ACCEPTED * _STATUS_OFFSET;
                else
                    questStatus = (int)QuestStatus.DECLINED * _STATUS_OFFSET;

                TestDialogueMainManager.Instance.OnRequestUpdateQuestChoice?
                    .Invoke(null, questStatus, _btQuestChoiceTracker[_currQtChoiceIndex]);
                ShowQuestRect(false);
                UpdateUIForInteraction(InteractionType.MADE_CHOICE);

                //Disable both Quest-Choice buttons
                _questChoiceBts[1].gameObject.SetActive(false);
                _questChoiceBts[2].gameObject.SetActive(false);
                _currQtChoiceIndex = _DEFAULT_VAL;

                return;
            }

            // This is for the main-quest
            if (btIndex == 0)
            {

            }
            //Show Tell me more about it / Not Now option for Player
            else
            {
                _currQtChoiceIndex = btIndex;

                //TODO: Replace with this FOR loop
                // for (int i = 0; i < 2; i++)
                _questChoiceBts[1].gameObject.SetActive(true);              //For Yes
                _questChoicesTxt[1].text = _playerChoicesStr[0];

                _questChoiceBts[2].gameObject.SetActive(true);              //For No
                _questChoicesTxt[2].text = _playerChoicesStr[2];
            }


            // Send action to show Dialogue

        }

        private void ChoseDialogue(int btIndex)
        {
#if DEBUG_1
            Debug.Log($"Player chose dialogue. Index: {btIndex}");
#endif
            // _showChoiceStatus = (byte)ChoiceType.CHOICE_CLICKED;
            TestDialogueMainManager.Instance.OnPlayerInteraction?.Invoke(InteractionType.MADE_CHOICE, btIndex, _UNSET_VAL);
        }

        private void UpdateUIForInteraction(InteractionType interactionType, int value = -1, int otherVal = -1)
        {
#if DEBUG_1
            Debug.Log($"UpdateUIForInteraction | interactionType: {interactionType} | value: {value} | otherVal: {otherVal}");
#endif
            switch (interactionType)
            {
                case InteractionType.MADE_CHOICE:
                    {
                        _currDialogueIndex = 0;
                        // _dialogueTxt.gameObject.SetActive(true);

                        //Disable every Dialogue Choice
                        for (int i = 0; i < _dialogueChoiceBts.Length; i++)
                            _dialogueChoiceBts[i].gameObject.SetActive(false);

                        //Disable every Quest Choice
                        for (int i = 0; i < _questChoiceBts.Length; i++)
                            _questChoiceBts[i].gameObject.SetActive(false);
                    }
                    break;

                case InteractionType.INTERACTING_WITH_NPC:
                    // if (value != -1 && _prevInteractionType != InteractionType.FINISHING_INTERACTION)
                    _dialogueRect.SetActive(true);
                    // else
                    // _dialogueRect.SetActive(false);

                    break;

                case InteractionType.FINISHING_INTERACTION:
                    _dialogueRect.SetActive(false);
                    ShowQuestRect(false);

                    goto case InteractionType.MADE_CHOICE;          //Disable Multiple choices also if enabled
            }
            _prevInteractionType = interactionType;
        }

        private void UpdateDialogueText(string dialogue, bool showChoices)
        {
            // if (dialogue.Equals(_EMPTY_STR))            
            //     _dialogueRect.SetActive(false);            

            if (showChoices)
            {
                _dialogueChoiceBts[_currDialogueIndex].gameObject.SetActive(true);
                _dialogueChoicesTxt[_currDialogueIndex].text = dialogue;
                _currDialogueIndex++;
            }
            else
                _dialogueTxt.text = dialogue;
        }
    }
}