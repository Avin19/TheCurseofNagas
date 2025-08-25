using System;
using System.Collections.Generic;
using CurseOfNaga.QuestSystem;
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

            CompletedFlags = new HashSet<string>();
            ActiveQuestFlags = new HashSet<string>();
        }
        #endregion Singleton

        public PlayerStatus CurrPlayerStatus;

        public TestConditionFlags flag1 = TestConditionFlags.NOT_SET;
        public HashSet<string> CompletedFlags, ActiveQuestFlags;

        public Action<InteractionType, int, int> OnPlayerInteraction;
        public Action<string, bool> OnShowDialogue;
        public Action<string> OnDialogueUpdateRequested;

        //================================== QUEST =================================
        /// <summary>
        /// string - Quest ID | Quest Active (null) <br/> QuestStatus - Status of the Quest <br/>
        /// int - Quest Index if quest already active else DefaultValue(-1)
        /// </summary>
        public Action<string, QuestStatus, int> OnQuestUpdate;
        // public Action<int> OnQuestInfoRequest;

        /// <summary>
        /// Quest - Quest Data <br/> int - Button index if the quest is requested for the first time else DefaultValue(-1)
        /// </summary>
        public Action<Quest, int> OnQuestUIUpdate;
        public Action<Reward, int> OnQuestCompleted;
    }
}