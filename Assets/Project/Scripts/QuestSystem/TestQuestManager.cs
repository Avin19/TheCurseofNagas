using System.Collections.Generic;
using CurseOfNaga.DialogueSystem.Test;
using CurseOfNaga.Utils;

using UnityEngine;

namespace CurseOfNaga.QuestSystem
{
    //TODO: Non-Monobehaviour
    public class TestQuestManager : MonoBehaviour
    {
        private QuestTemplate _questTemplate;

        //======================= TODO: Need to be saved =============================
        private List<int> _activeQuestIndexes;
        private List<int> _completedQuestIndexes;
        private int _mainQuestIndex;
        //======================= Need to be saved =============================
        private int _requestedQuestIndex;

        private const string _FILENAME = "QuestData.json";
        private const string _MAIN_QUEST_ID = "000_FI_VL";

        private void OnDisable()
        {
            TestDialogueMainManager.Instance.OnQuestUpdate -= UpdateQuestData;
        }

        private void OnEnable()
        {
            Invoke(nameof(Initialize), 4f);
            LoadQuestJson();
        }

        public void Initialize()
        {
            TestDialogueMainManager.Instance.OnQuestUpdate += UpdateQuestData;

            //Load the Saved Data back to game
            LoadSave();
        }

        public void LoadSave()
        {


            // Initialize main quest if the game is new
            _mainQuestIndex = 0;
            _activeQuestIndexes = new List<int>();
            _completedQuestIndexes = new List<int>();
            // UpdateQuestData(_questTemplate.quests_data[0].uid, QuestStatus.REQUESTED);
            UpdateQuestData(_MAIN_QUEST_ID, QuestStatus.REQUESTED);
        }

        public async void LoadQuestJson()
        {
            string pathToJson = System.IO.Path.Join(Application.streamingAssetsPath, _FILENAME);
            Debug.Log($"Loadiing Quest Json from: {pathToJson}");

            if (!System.IO.File.Exists(pathToJson))
            {
                Debug.LogError($"Invalid Quest JSON file path: {pathToJson}");
                return;
            }

            FileDataHelper fileDataHelper = new FileDataHelper();
            string jsonData = await fileDataHelper.GetFileData_Async(pathToJson);
            // Debug.Log($"Quest Json: {jsonData}");

            _questTemplate = JsonUtility.FromJson<QuestTemplate>(jsonData);
        }


        private void UpdateQuestData(string idVal, QuestStatus questStatus)
        {
            switch (questStatus)
            {
                case QuestStatus.IN_PROGRESS:
                    break;

                case QuestStatus.FAILED:
                    break;

                case QuestStatus.REQUESTED:
                    int _requestedQuestIndex;
                    int.TryParse(idVal.Substring(0, 3), out _requestedQuestIndex);

                    //Send Quest Data to UI for showing the player on screen
                    TestDialogueMainManager.Instance.OnQuestUIUpdate?.Invoke(_questTemplate.quests_data[_requestedQuestIndex]);
                    break;

                case QuestStatus.ACCEPTED:
                    break;
            }

            if (questStatus == QuestStatus.REQUESTED) return;

            int objCount = 0;
            int objCompletedCount = 0;
            bool foundObjecttive = false;
            List<QuestObjective> questObjectives;

            //Search through the active quests, which objective is being completed or has been completed
            for (int i = 0; i < _activeQuestIndexes.Count && !foundObjecttive; i++)
            {
                objCount = _questTemplate.quests_data[i].objectives.Count;
                questObjectives = _questTemplate.quests_data[i].objectives;

                for (int j = 0; j < objCount; j++)
                {
                    if (questObjectives[j].currentCount == questObjectives[j].requiredCount)
                        objCompletedCount++;
                    else if (questObjectives[j].Equals(idVal))
                    {
                        foundObjecttive = true;

                        questObjectives[i].currentCount++;
                        //Update UI for objective
                        TestDialogueMainManager.Instance.OnQuestUIUpdate?.Invoke(_questTemplate.quests_data[i]);

                        if (questObjectives[j].currentCount == questObjectives[j].requiredCount)
                            objCompletedCount++;
                    }
                }

                if (objCompletedCount == objCount)
                {
                    _activeQuestIndexes.Remove(i);      //Remove from active Quests

                    //If the quest is a main quest, then mark it complete and proceed to the next quest, add it to the active quest list
                    if (_questTemplate.quests_data[i].type == QuestType.MAIN_QUEST)
                    {
                        _mainQuestIndex++;
                        _activeQuestIndexes.Add(_mainQuestIndex);
                        _questTemplate.quests_data[i].status = QuestStatus.IN_PROGRESS;
                    }
                    else
                        _questTemplate.quests_data[i].status = QuestStatus.COMPLETED;

                    _completedQuestIndexes.Add(i);      //Add to completed Quests

                    TestDialogueMainManager.Instance.OnQuestCompleted?.Invoke(_questTemplate.quests_data[i].reward);
                }
            }
        }

        private void UpdateExistingQuest() { }

        private void LoadQuest() { }
    }
}