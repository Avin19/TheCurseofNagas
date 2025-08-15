using UnityEngine;

using static CurseOfNaga.Global.UniversalConstant;

namespace CurseOfNaga.DialogueSystem.Test
{
    public class TestDialogueSystemCanvas : MonoBehaviour
    {
        [SerializeField] private GameObject _dialogueRect;
        [SerializeField] private TMPro.TMP_Text _dialogueTxt;

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
        }

        private void UpdateUIForInteraction(InteractionType interactionType, int value = 1, int otherVal = -1)
        {
            Debug.Log($"UpdateUIForInteraction | value: {value} | otherVal: {otherVal}");
            switch (interactionType)
            {
                case InteractionType.INTERACTING_WITH_NPC:
                    if (otherVal != -1)
                        _dialogueRect.SetActive(true);
                    else
                        _dialogueRect.SetActive(false);

                    break;
            }
        }

        private void UpdateDialogueText(string dialogue)
        {
            _dialogueTxt.text = dialogue;
        }
    }
}