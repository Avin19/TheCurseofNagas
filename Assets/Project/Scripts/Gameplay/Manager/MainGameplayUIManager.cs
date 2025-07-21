using System;
using UnityEngine;

using CurseOfNaga.Global;
using static CurseOfNaga.Global.UniversalConstant;

namespace CurseOfNaga.Gameplay.Managers
{
    public class MainGameplayUIManager : MonoBehaviour
    {
        [SerializeField] private TMPro.TMP_Text _promptTxt;

        private const string _PRESS_PROMPT = "Press \'E\' to interact.";


        private void OnDestroy()
        {
            MainGameplayManager.Instance.OnPlayerInteraction -= UpdateUIForInteraction;
        }

        private void Start()
        {
            MainGameplayManager.Instance.OnPlayerInteraction += UpdateUIForInteraction;
        }

        private void UpdateUIForInteraction(InteractionType interactionType, int value = 1)
        {
            switch (interactionType)
            {
                case InteractionType.PROMPT_TRIGGERED:
                    ShowPrompt(value);

                    break;
            }
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