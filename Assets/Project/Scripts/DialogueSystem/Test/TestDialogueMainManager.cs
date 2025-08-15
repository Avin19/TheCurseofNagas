using System;
using UnityEngine;
using static CurseOfNaga.Global.UniversalConstant;

namespace CurseOfNaga.DialogueSystem.Test
{
    public class TestDialogueMainManager : MonoBehaviour
    {
        #region Singleton
        private static TestDialogueMainManager _instance;
        public static TestDialogueMainManager Instance { get => _instance; }

        private void Awake()
        {
            if (_instance == null)
                _instance = this;
            else
                Destroy(gameObject);
        }
        #endregion Singleton

        public TestConditionFlags flag1 = TestConditionFlags.NOT_SET;

        public Action<InteractionType, int, int> OnPlayerInteraction;
        public Action<string> OnShowDialogue;
    }
}