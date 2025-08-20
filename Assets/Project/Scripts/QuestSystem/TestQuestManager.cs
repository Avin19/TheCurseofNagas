using System.Collections.Generic;
using CurseOfNaga.DialogueSystem.Test;
using CurseOfNaga.Utils;

using UnityEngine;

namespace CurseOfNaga.QuestSystem
{
    public class TestQuestManager : MonoBehaviour
    {
        private QuestTemplate _questTemplate;
        private List<int> _activeQuestIndexes;
        private List<int> _completedQuestIndexes;

        private const string _FILENAME = "QuestData.json";

        private void OnDisable()
        {
            TestDialogueMainManager.Instance.OnQuestUpdate -= UpdateQuest;
        }

        private void OnEnable()
        {
            LoadQuestJson();
            TestDialogueMainManager.Instance.OnQuestUpdate += UpdateQuest;
        }

        public async void LoadQuestJson()
        {
            string pathToJson = System.IO.Path.Join(Application.streamingAssetsPath, _FILENAME);
            Debug.Log($"Loadiing Json from: {pathToJson}");

            if (!System.IO.File.Exists(pathToJson))
            {
                Debug.LogError($"Invalid Dialogues JSON file path: {pathToJson}");
                return;
            }

            FileDataHelper fileDataHelper = new FileDataHelper();
            string jsonData = await fileDataHelper.GetFileData_Async(pathToJson);
            Debug.Log($"Quest Json: {jsonData}");

            _questTemplate = JsonUtility.FromJson<QuestTemplate>(jsonData);

        }

        private void UpdateQuest(string targetID)
        {
            int objCount = 0;
            int objCompletedCount = 0;
            List<QuestObjective> questObjectives;

            //Search through the active quests, which objective is being completed or has been completed
            for (int i = 0; i < _activeQuestIndexes.Count; i++)
            {
                objCount = _questTemplate.quests_data[i].objectives.Count;
                questObjectives = _questTemplate.quests_data[i].objectives;

                for (int j = 0; j < objCount; j++)
                {
                    if (questObjectives[j].currentCount == questObjectives[j].requiredCount)
                        objCompletedCount++;
                    else if (questObjectives[j].Equals(targetID))
                    {
                        questObjectives[i].currentCount++;
                        //Update UI for objective
                        TestDialogueMainManager.Instance.OnQuestUIUpdate?.Invoke(_questTemplate.quests_data[i]);

                        if (questObjectives[j].currentCount == questObjectives[j].requiredCount)
                            objCompletedCount++;
                    }
                }

                if (objCompletedCount == objCount)
                {
                    _questTemplate.quests_data[i].status = QuestStatus.COMPLETED;
                    _completedQuestIndexes.Add(i);

                    TestDialogueMainManager.Instance.OnQuestCompleted?.Invoke(_questTemplate.quests_data[i].reward);
                }

            }
        }
    }
}