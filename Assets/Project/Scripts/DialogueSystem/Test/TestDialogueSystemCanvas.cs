using UnityEngine;
using UnityEngine.UI;

using static CurseOfNaga.Global.UniversalConstant;

namespace CurseOfNaga.DialogueSystem.Test
{
    public class TestDialogueSystemCanvas : MonoBehaviour
    {
        [SerializeField] private GameObject _dialogueRect;
        [SerializeField] private TMPro.TMP_Text _dialogueTxt;
        [SerializeField] private Button[] _dialogueChoiceBts;
        [SerializeField] private TMPro.TMP_Text[] _dialogueChoicesTxt;

        private bool _showChoice;
        private int _currDialogueIndex;

        private void OnDisable()
        {
            TestDialogueMainManager.Instance.OnPlayerInteraction -= UpdateUIForInteraction;
            TestDialogueMainManager.Instance.OnShowDialogue -= UpdateDialogueText;
        }

        private void OnEnable()
        {
            Invoke(nameof(Initialize), 1f);
        }

        private void Initialize()
        {
            TestDialogueMainManager.Instance.OnPlayerInteraction += UpdateUIForInteraction;
            TestDialogueMainManager.Instance.OnShowDialogue += UpdateDialogueText;

            for (int i = 0; i < _dialogueChoiceBts.Length; i++)
            {
                int tempIndex = i;
                _dialogueChoiceBts[i].onClick.AddListener(() => ChoseDialogue(tempIndex));
            }
        }

        private void ChoseDialogue(int btIndex)
        {
            Debug.Log($"Player chose dialogue. Index: {btIndex}");
        }

        private void UpdateUIForInteraction(InteractionType interactionType, int value = -1, int otherVal = -1)
        {
            Debug.Log($"UpdateUIForInteraction | interactionType: {interactionType} | value: {value} | otherVal: {otherVal}");
            switch (interactionType)
            {
                case InteractionType.INTERACTING_WITH_NPC:

                    if (otherVal == -(int)DialogueType.CHOICE)
                    {
                        _showChoice = true;
                        // _dialogueTxt.gameObject.SetActive(false);

                        for (int i = 0; i < _dialogueChoiceBts.Length; i++)
                            _dialogueChoiceBts[i].gameObject.SetActive(true);

                        break;
                    }
                    else if (_currDialogueIndex > 0)            //Choices have been shown
                    {
                        _currDialogueIndex = 0;
                        _showChoice = false;
                        // _dialogueTxt.gameObject.SetActive(true);

                        for (int i = 0; i < _dialogueChoiceBts.Length; i++)
                            _dialogueChoiceBts[i].gameObject.SetActive(false);
                    }

                    if (value != -1)
                        _dialogueRect.SetActive(true);
                    else
                        _dialogueRect.SetActive(false);

                    break;
            }
        }

        private void UpdateDialogueText(string dialogue)
        {
            if (_showChoice)
                _dialogueChoicesTxt[_currDialogueIndex].text = dialogue;
            else
                _dialogueTxt.text = dialogue;
        }
    }
}