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

        private int _currDialogueIndex;
        private InteractionType _prevInteractionType;
        private const int _ACTIVE = 1, _INACTIVE = 0, _DEFAULT_VALUE = -1;

        private void OnDisable()
        {
            TestDialogueMainManager.Instance.OnPlayerInteraction -= UpdateUIForInteraction;
            TestDialogueMainManager.Instance.OnShowDialogue -= UpdateDialogueText;
        }

        private void OnEnable()
        {
            Invoke(nameof(Initialize), 1.5f);
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
#if DEBUG_1
            Debug.Log($"Player chose dialogue. Index: {btIndex}");
#endif
            // _showChoiceStatus = (byte)ChoiceType.CHOICE_CLICKED;
            TestDialogueMainManager.Instance.OnPlayerInteraction?.Invoke(InteractionType.MADE_CHOICE, btIndex, _INACTIVE);
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

                        for (int i = 0; i < _dialogueChoiceBts.Length; i++)
                            _dialogueChoiceBts[i].gameObject.SetActive(false);
                    }
                    break;

                case InteractionType.INTERACTING_WITH_NPC:

                    if (value != -1 && _prevInteractionType != InteractionType.FINISHING_INTERACTION)
                        _dialogueRect.SetActive(true);
                    else
                        _dialogueRect.SetActive(false);

                    break;

                case InteractionType.FINISHING_INTERACTION:
                    _dialogueRect.SetActive(false);
                    goto case InteractionType.MADE_CHOICE;          //Disable Multiple choices also if enabled
            }
            _prevInteractionType = interactionType;
        }

        private void UpdateDialogueText(string dialogue, bool showChoices)
        {
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