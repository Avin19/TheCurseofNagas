/*
*   - Will follow the Diloagues.json setting
*   - Main Quests | Sub-Main Quests
*       [=] [NEW] First 2 digit - Quest Group | Directly/Indirectly Related to Main Quest | Can be unlocked if main-quest of that group is unlocked
*       [=] [NEW] Next 3 digits - Quest Index
*       [=] [NEW] 1st quest will always be the main quest
*   - Skyrim triggers quests through dialogue
*       [=] Same Agree/Disagree | Although everything is explained while speaking, no outright quest info screen
*   - List within List can get confusing
*       [=] Better to k
*   - Keep structure flat?
*       [=] Would still be a headache to add quests in between
*   - JsonUtility cannot handle multi-dimnesional arrays
*/

#define TEST_QUESTS_1
// #define TO_JSON_TEST_1
#define TEST_QUEST_TRIGGERS
#define TEST_ENABLE_UPDATE

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
        // Only need to keep the base/starting level of the SubQuests/SideQuests
        [SerializeField] private int[] _questTracker;                    //For important characters
        //======================= Need to be saved =============================
        private int _requestedQuestIndex;


#if TEST_QUEST_TRIGGERS
        //==============================================> TODO: Optimize <==============================================
        // TESTING VISUAL TRIGGERS | Code from MainGameplayManager
        //TODO: Transfer this logic to some other system

        [System.Serializable]
        internal class ObjectiveInfo
        {
            public int type;          //TODO: UniversalConstant alternative
            public Transform transform;
        }

        [Space(10), Header("Test Objective Logic")]
        [SerializeField] private List<ObjectiveInfo> _questObjectives;
        // [SerializeField] private List<ObjectiveInfo> _exploreObjectives;
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private Transform[] _npcTransforms;        //TODO: This list should be in some other Script
        [Tooltip("Make sure to group same explore transforms together one-after-another")]
        [SerializeField] private Transform[] _exploreTransforms;
        [SerializeField] private Transform _playerTransform;
        private const int _OBJ_TYPE_MOD = 1000;
        private const float _MAGNITUDE_MIN_DIFF = 1f;
        //==============================================> TODO: Optimize <==============================================
#endif

#if TEST_QUESTS_1
        [Space(10), Header("Test Quest Logic")]
        [SerializeField] private string _testQuestVal;
        [SerializeField] private QuestStatus _testQuestStatus;
        [SerializeField] private int _testQuestIndex;
        [SerializeField] private bool _executeTestUpdateQuest;
#endif

        private const string _FILENAME = "QuestData.json";
        private const string _MAIN_QUEST_ID = "000_FI_VL";
        private const int _MAIN_QUEST_COMMON_INDEX = 0;
        private const int _PLAYER_OFFSET = 1;

        private void OnDisable()
        {
            TestDialogueMainManager.Instance.OnQuestUpdate -= UpdateQuestData;
            // TestDialogueMainManager.Instance.OnQuestInfoRequest -= SendQuestInfo;
        }

        private void OnEnable()
        {
            Invoke(nameof(Initialize), 2f);
            LoadQuestJson();
#if TO_JSON_TEST_1
            ToJsonTest1();
#endif
        }

        public void Initialize()
        {
            TestDialogueMainManager.Instance.OnQuestUpdate += UpdateQuestData;
            // TestDialogueMainManager.Instance.OnQuestInfoRequest += SendQuestInfo;

            //Load the Saved Data back to game
            LoadSave();
        }

        public void Initialize(int questGiverNPCCount)
        {
            _questTracker = new int[questGiverNPCCount + _PLAYER_OFFSET];
        }

        public void LoadSave()
        {
            //TODO: Load Save Data

            // Initialize main quest if the game is new
            _questTracker[_MAIN_QUEST_COMMON_INDEX] = 0;
            _activeQuestIndexes = new List<int>();
            _completedQuestIndexes = new List<int>();
            // UpdateQuestData(_questTemplate.quests_data[0].uid, QuestStatus.REQUESTED);

            UpdateQuestData(_MAIN_QUEST_ID, QuestStatus.REQUESTED);
            CheckForUnlockedQuest();
        }

#if TO_JSON_TEST_1
        [System.Serializable]
        internal class ToJsonTestClass1
        {
            // public ToJsonTestSubClass1[][] TestSubClass;
            public ToJsonTestSubClass1[] TestSubClass;
        }

        [System.Serializable]
        internal class ToJsonTestSubClass1
        {
            public int TestValue1;
        }

        private void ToJsonTest1()
        {
            ToJsonTestClass1 testClass = new ToJsonTestClass1();

            // testClass.TestSubClass = new ToJsonTestSubClass1[2][];
            // for (int i = 0; i < testClass.TestSubClass.Length; i++)
            // {
            //     testClass.TestSubClass[i] = new ToJsonTestSubClass1[] {
            //         new ToJsonTestSubClass1 { TestValue1 = 10 },
            //         new ToJsonTestSubClass1 { TestValue1 = 20 }
            //         };
            // }

            testClass.TestSubClass = new ToJsonTestSubClass1[] {
                    new ToJsonTestSubClass1 { TestValue1 = 10 },
                    new ToJsonTestSubClass1 { TestValue1 = 20 }
                    };

            string tempJsonData = JsonUtility.ToJson(testClass);
            Debug.Log($"tempJsonData: {tempJsonData}");
        }
#endif

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


#if TEST_ENABLE_UPDATE
        private void Update()
        {
#if TEST_QUESTS_1
            if (_executeTestUpdateQuest)
            {
                _executeTestUpdateQuest = false;
                UpdateQuestData(_testQuestVal, _testQuestStatus, _testQuestIndex);
            }
#endif

#if TEST_QUEST_TRIGGERS
            CheckObjectivesConditions();
#endif
        }
#endif

        private void UpdateQuestData(string idVal, QuestStatus questStatus, int questIndex = 0)
        {
            int gpIndex, qtIndex, tempPowerRaised;
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
                        tempPowerRaised = (int)Mathf.Pow(10, QUEST_INDEX_LENGTH);
                        qtIndex = _activeQuestIndexes[i] % tempPowerRaised;
                        gpIndex = _activeQuestIndexes[i] / tempPowerRaised;

                        questObjectives = _questTemplate.quest_groups[gpIndex].content[qtIndex].objectives;
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
                                    .Invoke(_questTemplate.quest_groups[gpIndex].content[qtIndex], _DEFAULT_VAL);

                                if (questObjectives[j].current_count == questObjectives[j].required_count)
                                    objCompletedCount++;

                                // break;
                            }
                        }

                        if (objCompletedCount == objCount)
                        {
                            _completedQuestIndexes.Add(_activeQuestIndexes[i]);      //Add to completed Quests

                            //If the quest is a main quest, then mark it complete and proceed to the next quest, add it to the active quest list
                            if (_questTemplate.quest_groups[gpIndex].content[qtIndex].type == QuestType.MAIN_QUEST)
                            {
                                //Not being update properly
                                _questTracker[_MAIN_QUEST_COMMON_INDEX]++;
                                //IMP | Keep the main Quest always in index 0
                                _activeQuestIndexes[_MAIN_QUEST_COMMON_INDEX] = _questTracker[_MAIN_QUEST_COMMON_INDEX] * tempPowerRaised;
                                _questTemplate.quest_groups[_questTracker[_MAIN_QUEST_COMMON_INDEX]]
                                    .content[_MAIN_QUEST_COMMON_INDEX].status = QuestStatus.IN_PROGRESS;

                                //FIXME: Dont initiate next main_quest | wait for the NPC to be interacted with
                                // TestDialogueMainManager.Instance.OnQuestUIUpdate?
                                //     .Invoke(_questTemplate.quest_groups[_questTracker[_MAIN_QUEST_COMMON_INDEX]]
                                //     .content[_MAIN_QUEST_COMMON_INDEX], _activeQuestIndexes[_MAIN_QUEST_COMMON_INDEX]);
                            }
                            else
                            {
                                _questTemplate.quest_groups[gpIndex].content[qtIndex].status = QuestStatus.COMPLETED;
                                _activeQuestIndexes.RemoveAt(i);      //Remove from active Quests
                            }
                            TestDialogueMainManager.Instance.OnQuestCompleted?.Invoke(_questTemplate.quest_groups[gpIndex].content[qtIndex].reward, i);

                            //Check which quests have been unlocked by completing a main-quest
                            CheckForUnlockedQuest();
                        }
                    }

                    break;

                //Reset progress
                case QuestStatus.FAILED:
                    break;

                case QuestStatus.REQUESTED:
                    // int.TryParse(idVal.Substring(_GROUP_INDEX_START, _GROUP_INDEX_LENGTH), out _requestedQuestIndex);
                    int.TryParse(idVal.Substring(GROUP_INDEX_START, GROUP_INDEX_LENGTH), out gpIndex);
                    int.TryParse(idVal.Substring(QUEST_INDEX_START, QUEST_INDEX_LENGTH), out qtIndex);

                    _requestedQuestIndex = gpIndex * (int)Mathf.Pow(10, QUEST_INDEX_LENGTH);
                    _requestedQuestIndex += qtIndex;

                    //Send Quest Data to UI for showing the player on screen
                    TestDialogueMainManager.Instance.OnQuestUIUpdate?
                        .Invoke(_questTemplate.quest_groups[gpIndex].content[qtIndex], _requestedQuestIndex);

                    break;

                //Player accepts the Sub-Main Quest, Side-Quest and Main Quest
                case QuestStatus.ACCEPTED:
                    _activeQuestIndexes.Add(_requestedQuestIndex);

                    break;

                case QuestStatus.COMPLETED:


                    break;

                case QuestStatus.REQUESTED_INFO:
                    tempPowerRaised = (int)Mathf.Pow(10, QUEST_INDEX_LENGTH);
                    qtIndex = questIndex % tempPowerRaised;
                    gpIndex = questIndex / tempPowerRaised;
                    TestDialogueMainManager.Instance.OnQuestUIUpdate?
                        .Invoke(_questTemplate.quest_groups[gpIndex].content[qtIndex], _DEFAULT_VAL);

                    break;
            }
        }

        private void CheckForUnlockedQuest()
        {
            // Loop through the group to check which content has been unlocked except the main-quest at 0th index
            int contentCount = _questTemplate.quest_groups[_questTracker[_MAIN_QUEST_COMMON_INDEX]].content.Count;
            int objIndex = 0, tIndex = 0;
            List<QuestObjective> questObjectives;

            for (int contentIndex = 0; contentIndex < contentCount; contentIndex++)
            {
                // Make Dialogue choices available for NPCs with new quests unlocked
                TestDialogueMainManager.Instance.OnDialogueUpdateRequested?.Invoke(_questTemplate
                    .quest_groups[_questTracker[_MAIN_QUEST_COMMON_INDEX]].content[contentIndex].uid,
                    _questTemplate.quest_groups[_questTracker[_MAIN_QUEST_COMMON_INDEX]].content[contentIndex].type);

                questObjectives = _questTemplate.quest_groups[_questTracker[_MAIN_QUEST_COMMON_INDEX]]
                    .content[contentIndex].objectives;

                for (objIndex = 0; objIndex < questObjectives.Count; objIndex++)
                {
                    //Get all the objectives that are of FIND type and add to list
                    if (questObjectives[objIndex].type == ObjectiveType.FIND)
                    {
                        ObjectiveInfo objective = new ObjectiveInfo
                        {
                            type = 1 * _OBJ_TYPE_MOD + (int)ObjectiveType.FIND,
                            transform = _npcTransforms[GetTransformIndex(questObjectives[objIndex].target_id)]
                        };
                        _questObjectives.Add(objective);
                    }
                    else if (questObjectives[objIndex].type == ObjectiveType.EXPLORE)
                    {
                        List<int> transformIndexes = GetConcurrentTransformIndexes(questObjectives[objIndex].target_id);
                        for (tIndex = 0; tIndex < transformIndexes.Count; tIndex++)
                        {
                            ObjectiveInfo objective = new ObjectiveInfo
                            {
                                type = 1 * _OBJ_TYPE_MOD + (int)ObjectiveType.EXPLORE,
                                transform = _exploreTransforms[transformIndexes[tIndex]]
                            };
                            _questObjectives.Add(objective);
                        }
                    }
                }
            }
        }

        private int GetTransformIndex(string objID)
        {
            int transformIndex = 0;
            for (; transformIndex < _npcTransforms.Length; transformIndex++)
            {
                // Check which transform is the objective
                if (_npcTransforms[transformIndex].name[^OBJECTIVE_ID_START..].Equals(objID))
                    break;
            }

            return transformIndex;
        }

        private List<int> GetConcurrentTransformIndexes(string objID)
        {
            List<int> tIndexes = new List<int>();
            int transformIndex = 0;
            int foundCount = 0, prevCount = -1;          // To prevent from traversing the whole list, to a certain point
            for (; transformIndex < _exploreTransforms.Length && foundCount != prevCount;
                transformIndex++)
            {
                prevCount = foundCount;
                if (_exploreTransforms[transformIndex].name[^OBJECTIVE_ID_START..].Equals(objID))
                {
                    tIndexes.Add(transformIndex);
                    foundCount++;
                }
            }
            return tIndexes;
        }

#if TEST_QUEST_TRIGGERS
        // This may be here or maybe in a separate System
        private void CheckObjectivesConditions()
        {
            Vector3 viewPointPos;
            int objType = 0;

            for (int i = 0; i < _questObjectives.Count; i++)
            {
                objType = _questObjectives[i].type % _OBJ_TYPE_MOD;

                switch (objType)
                {
                    // Check if the player has explored the area for the objective
                    case (int)ObjectiveType.EXPLORE:
                        float diffMagnitude = Vector3.SqrMagnitude(_questObjectives[i].transform.position - _playerTransform.position);
                        if (diffMagnitude >= _MAGNITUDE_MIN_DIFF)
                            continue;

                        break;

                    case (int)ObjectiveType.FIND:
                        viewPointPos = _mainCamera.WorldToViewportPoint(_questObjectives[i].transform.position);

                        //Skip those out of the view
                        if (Mathf.Min(viewPointPos.x, viewPointPos.y) < 0f          // For Objects out-of-camera and behind
                            || Mathf.Max(viewPointPos.x, viewPointPos.y) > 1f       // For Objects out-of-camera and in-front
                            || viewPointPos.z < 0f)                                 // For Objects behind-camera
                            continue;

                        break;
                }

                UpdateObjective(i);
            }
        }


        private void UpdateObjective(in int index)
        {
            int objStatus = _questObjectives[index].type / _OBJ_TYPE_MOD;
            int objType = _questObjectives[index].type % _OBJ_TYPE_MOD;

            string tempStr;
            switch (objStatus)
            {
                case (int)ObjectiveType.ACTIVE:
                    //Check some conditions and process accordingly
                    {
                        switch (objType)
                        {
                            //This will be on some other System
                            case (int)ObjectiveType.PUZZLE:
                                break;

                            // May need to hit certain points | Can be here
                            case (int)ObjectiveType.EXPLORE:
                                tempStr = _questObjectives[index].transform.name[^OBJECTIVE_ID_START..].ToUpper();
                                Debug.Log($"Explored Objective: {tempStr}");

                                //Inform that objective found
                                TestDialogueMainManager.Instance.OnQuestUpdate?.Invoke(tempStr, QuestStatus.IN_PROGRESS, _DEFAULT_VAL);

                                //Update Objective
                                _questObjectives[index].type = (int)ObjectiveType.COMPLETED * _OBJ_TYPE_MOD + objType;

                                break;

                            // Proximity logic, so here
                            case (int)ObjectiveType.FIND:
                                tempStr = _questObjectives[index].transform.name[^OBJECTIVE_ID_START..].ToUpper();
                                Debug.Log($"Found Objective: {tempStr} | name:{_questObjectives[index].transform.name}");

                                //Inform that objective found
                                TestDialogueMainManager.Instance.OnQuestUpdate?.Invoke(tempStr, QuestStatus.IN_PROGRESS, _DEFAULT_VAL);

                                //Update Objective
                                _questObjectives[index].type = (int)ObjectiveType.COMPLETED * _OBJ_TYPE_MOD + objType;
                                // UpdateObjective(index);      // Qill automatically get removed in the next iteration

                                break;
                        }
                    }

                    break;

                // Do Nothing
                case (int)ObjectiveType.INACTIVE:
                    return;

                // case (int)ObjectiveType.CURRENT:
                //     //Check some conditions and process accordingly
                //     goto case (int)ObjectiveType.COMPLETED;

                // Remove from active
                case (int)ObjectiveType.COMPLETED:
                    _questObjectives[index].type = (int)ObjectiveType.INACTIVE * _OBJ_TYPE_MOD + objStatus;
                    // _inactiveObjectives.Add(_objectives[index]);
                    _questObjectives.RemoveAt(index);

                    return;
            }
        }

#endif

        private void UpdateExistingQuest() { }

        private void LoadQuest() { }
    }
}