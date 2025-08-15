using System;
using UnityEngine;

using CurseOfNaga.Global;
using static CurseOfNaga.Global.UniversalConstant;

namespace CurseOfNaga.Gameplay.Managers
{
    public class MainGameplayUIManager : MonoBehaviour
    {
        [SerializeField] private TMPro.TMP_Text _promptTxt;

        [SerializeField] private GameObject _dialogueRect;
        [SerializeField] private TMPro.TMP_Text _dialogueTxt;

        private const string _PRESS_PROMPT = "Press \'E\' to interact.";


        private void OnDestroy()
        {
            MainGameplayManager.Instance.OnPlayerInteraction -= UpdateUIForInteraction;
            MainGameplayManager.Instance.OnShowDialogue -= UpdateDialogueText;
        }

        private void Start()
        {
            MainGameplayManager.Instance.OnPlayerInteraction += UpdateUIForInteraction;
            MainGameplayManager.Instance.OnShowDialogue += UpdateDialogueText;
        }

        private void UpdateUIForInteraction(InteractionType interactionType, int value = 1, int dummyVal = -1)
        {
            switch (interactionType)
            {
                case InteractionType.PROMPT_TRIGGERED:
                    ShowPrompt(value);

                    break;

                case InteractionType.INTERACTING_WITH_NPC:
                    if (value >= 1)
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

        private void ShowPrompt(int promptStatus)
        {
            if (promptStatus >= 1)
            {
                _promptTxt.gameObject.SetActive(true);
                _promptTxt.text = _PRESS_PROMPT;
            }
            else
            {
                _promptTxt.gameObject.SetActive(false);
                _promptTxt.text = "";
            }
        }
    }
}