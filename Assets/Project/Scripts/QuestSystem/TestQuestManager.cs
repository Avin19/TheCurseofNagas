#define TEST_QUESTS_1

using System.Collections.Generic;

using CurseOfNaga.DialogueSystem.Test;
using CurseOfNaga.Utils;
using static CurseOfNaga.Global.UniversalConstant;

using UnityEngine;

namespace CurseOfNaga.QuestSystem
{
    //TODO: Non-Monobehaviour
    public class TestQuestManager : MonoBehaviour
    {
        private QuestTemplate _questTemplate;

        //======================= TODO: Need to be saved =============================
        private List<int> _activeQuestIndexes;          //Main Quest will always be at 0
        private List<int> _completedQuestIndexes;
        private int _mainQuestIndex;
        //======================= Need to be saved =============================
        private int _requestedQuestIndex;

        private const string _FILENAME = "QuestData.json";
        private const string _MAIN_QUEST_ID = "000_FI_VL";

        private void OnDisable()
        {
            TestDialogueMainManager.Instance.OnQuestUpdate -= UpdateQuestData;
            // TestDialogueMainManager.Instance.OnQuestInfoRequest -= SendQuestInfo;
        }

        private void OnEnable()
        {
            Invoke(nameof(Initialize), 2f);
            LoadQuestJson();
        }

        public void Initialize()
        {
            TestDialogueMainManager.Instance.OnQuestUpdate += UpdateQuestData;
            // TestDialogueMainManager.Instance.OnQuestInfoRequest += SendQuestInfo;

            //Load the Saved Data back to game
            LoadSave();
        }

        public void LoadSave()
        {
            //TODO: Load Save Data

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

        // private void SendQuestInfo(int questIndex) { }

#if TEST_QUESTS_1

        [SerializeField] private string _testQuestVal;
        [SerializeField] private QuestStatus _testQuestStatus;
        [SerializeField] private int _testQuestIndex;
        [SerializeField] private bool _executeTestUpdateQuest;

        private void Update()
        {
            if (_executeTestUpdateQuest)
            {
                _executeTestUpdateQuest = false;
                TestUpdateQuestData();
            }
        }

        private void TestUpdateQuestData()
        {
            UpdateQuestData(_testQuestVal, _testQuestStatus, _testQuestIndex);
        }
#endif

        private void UpdateQuestData(string idVal, QuestStatus questStatus, int questIndex = 0)
        {
            Debug.Log($"idVal: {idVal} | questStatus: {questStatus} | questIndex: {questIndex}");
            switch (questStatus)
            {
                case QuestStatus.IN_PROGRESS:
                    int objCount = 0;
                    int objCompletedCount = 0;
                    bool foundObjective = false;
                    List<QuestObjective> questObjectives;

                    //Search through the active quests, which objective is being completed or has been completed
                    for (int i = 0; i < _activeQuestIndexes.Count && !foundObjective; i++)
                    {
                        questObjectives = _questTemplate.quests_data[_activeQuestIndexes[i]].objectives;
                        objCount = questObjectives.Count;

                        for (int j = 0; j < objCount; j++)
                        {
                            if (questObjectives[j].current_count == questObjectives[j].required_count)
                                objCompletedCount++;
                            else if (questObjectives[j].target_id.Equals(idVal))
                            {
                                foundObjective = true;

                                // questObjectives[_activeQuestIndexes[i]].current_count++;
                                questObjectives[j].current_count++;
                                //Update UI for objective
                                TestDialogueMainManager.Instance.OnQuestUIUpdate?
                                    .Invoke(_questTemplate.quests_data[_activeQuestIndexes[i]], _DEFAULT_VAL);

                                if (questObjectives[j].current_count == questObjectives[j].required_count)
                                    objCompletedCount++;

                                break;
                            }
                        }

                        if (objCompletedCount == objCount)
                        {
                            _completedQuestIndexes.Add(_activeQuestIndexes[i]);      //Add to completed Quests

                            //If the quest is a main quest, then mark it complete and proceed to the next quest, add it to the active quest list
                            if (_questTemplate.quests_data[_activeQuestIndexes[i]].type == QuestType.MAIN_QUEST)
                            {
                                _mainQuestIndex++;
                                _activeQuestIndexes[0] = _mainQuestIndex;       //IMP | Keep the main Quest always in index 0
                                _questTemplate.quests_data[_activeQuestIndexes[0]].status = QuestStatus.IN_PROGRESS;

                                TestDialogueMainManager.Instance.OnQuestUIUpdate?.Invoke(_questTemplate.quests_data[_activeQuestIndexes[0]], 0);
                            }
                            else
                            {
                                _questTemplate.quests_data[_activeQuestIndexes[i]].status = QuestStatus.COMPLETED;
                                _activeQuestIndexes.RemoveAt(i);      //Remove from active Quests
                            }
                            TestDialogueMainManager.Instance.OnQuestCompleted?.Invoke(_questTemplate.quests_data[_activeQuestIndexes[i]].reward, i);
                        }
                    }

                    break;

                //Reset progress
                case QuestStatus.FAILED:
                    break;

                case QuestStatus.REQUESTED:
                    int.TryParse(idVal.Substring(0, 3), out _requestedQuestIndex);

                    //Send Quest Data to UI for showing the player on screen
                    TestDialogueMainManager.Instance.OnQuestUIUpdate?
                        .Invoke(_questTemplate.quests_data[_requestedQuestIndex], _requestedQuestIndex);

                    break;

                //Player accepts the Sub-Main Quest, Side-Quest and Main Quest
                case QuestStatus.ACCEPTED:
                    _activeQuestIndexes.Add(_requestedQuestIndex);

                    break;

                case QuestStatus.REQUESTED_INFO:
                    TestDialogueMainManager.Instance.OnQuestUIUpdate?
                        .Invoke(_questTemplate.quests_data[questIndex], _DEFAULT_VAL);

                    break;
            }
        }

        private void UpdateExistingQuest() { }

        private void LoadQuest() { }
    }
}