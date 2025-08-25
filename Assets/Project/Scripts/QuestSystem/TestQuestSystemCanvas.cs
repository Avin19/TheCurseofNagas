#define PAUSE_TEST

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using CurseOfNaga.DialogueSystem.Test;
using static CurseOfNaga.Global.UniversalConstant;

namespace CurseOfNaga.QuestSystem.Test
{
    public class TestQuestSystemCanvas : MonoBehaviour
    {
        [SerializeField] private GameObject _questRect, _rewardRect, _questChoicesRect, _questContentRect;
        [SerializeField] private Button _acceptRewardBt, _acceptQuestBt, _backBt;
        [SerializeField] private TMPro.TMP_Text[] _questRewardsTxt;
        [SerializeField] private TMPro.TMP_Text _questTitleTxt, _questDescTxt, _questObjectivesTxt, _questRewardDescTxt;

        [SerializeField] private Button[] _checkQuestBts;           //Only upto 4 for now | 0 will always be Main Quest
        private TMPro.TMP_Text[] _checkQuestTxts;           //Only upto 4 for now
        private int[] _btQuestIndex;                        // 0 will always be Main Quest
        private int _currentBtIndex;
        private const int _TOTAL_QUEST_BTS = 4;
        private QuestType _currentQuestType;

        private const string _IN_PROGRESS = " [IN PROGRESS]", _COMPLETED = " [COMPLETED]";

        private bool _paused;
        [SerializeField] private GameObject _pauseMenuRect;

        private void OnDisable()
        {
            TestDialogueMainManager.Instance.OnQuestCompleted -= UpdateRewardTexts;
            TestDialogueMainManager.Instance.OnQuestUIUpdate -= UpdateQuestUI;
        }

        private void OnEnable()
        {
            Invoke(nameof(Initialize), 1.5f);
            // Invoke(nameof(TestUpdateRewardTexts), 1.5f);
        }

        private void Initialize()
        {
            TestDialogueMainManager.Instance.OnQuestCompleted += UpdateRewardTexts;
            TestDialogueMainManager.Instance.OnQuestUIUpdate += UpdateQuestUI;

            _acceptQuestBt.onClick.AddListener(() => UpdateQuestStatus(false));
            _acceptRewardBt.onClick.AddListener(() => UpdateQuestStatus(true));
            _backBt.onClick.AddListener(QuestRectClosed);

            _btQuestIndex = new int[_TOTAL_QUEST_BTS];
            _checkQuestTxts = new TMPro.TMP_Text[_TOTAL_QUEST_BTS];
            for (int i = 0; i < _TOTAL_QUEST_BTS; i++)
            {
                int tempIndex = i;
                _checkQuestBts[i].onClick.AddListener(() => CheckQuestCalled(tempIndex));
                _checkQuestBts[i].gameObject.SetActive(false);
                _checkQuestTxts[i] = _checkQuestBts[i].transform.GetChild(0).GetComponent<TMPro.TMP_Text>();
                _btQuestIndex[i] = -1;
            }
        }

#if PAUSE_TEST
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!_paused)
                {
                    _paused = true;

                    _pauseMenuRect.SetActive(true);
                    _questRect.SetActive(true);
                    _questChoicesRect.SetActive(true);
                    _questContentRect.SetActive(false);
                }
                else
                {
                    _paused = false;

                    _questRect.SetActive(false);
                    _pauseMenuRect.SetActive(false);
                    QuestRectClosed();
                }
            }
        }
#endif

        private void CheckQuestCalled(int checkIndex)
        {
            if (_btQuestIndex[checkIndex] == -1) return;

            TestDialogueMainManager.Instance.OnQuestUpdate
                ?.Invoke("", QuestStatus.REQUESTED_INFO, _btQuestIndex[checkIndex]);
            _questChoicesRect.SetActive(false);
            _questContentRect.SetActive(true);
        }

        private void QuestRectClosed()
        {
            //Player has not accpeted the quest presented
            if (_currentQuestType >= QuestType.MAIN_QUEST)
                TestDialogueMainManager.Instance.OnQuestUpdate?.Invoke(null, QuestStatus.DECLINED, _DEFAULT_VAL);

            _questChoicesRect.SetActive(true);
            _questContentRect.SetActive(false);
        }

        private void UpdateQuestStatus(bool rewarded = false)
        {
            Debug.Log($"Accepted Reward/Quest | rewarded: {rewarded}");
            // _questRect.SetActive(false);
            _rewardRect.SetActive(false);

            if (rewarded)
            {
                _questTitleTxt.text += _COMPLETED;
            }
            // Player accepted the Quest
            else
            {
                _checkQuestBts[_currentBtIndex].gameObject.SetActive(true);
                //Move to the next slot of button-indexes
                _currentBtIndex = ((_currentBtIndex + 1) >= _TOTAL_QUEST_BTS) ? 1 : ++_currentBtIndex;
                // _currentBtIndex++;
                _questTitleTxt.text += _IN_PROGRESS;
                TestDialogueMainManager.Instance.OnQuestUpdate?.Invoke(null, QuestStatus.ACCEPTED, _DEFAULT_VAL);
                _acceptQuestBt.gameObject.SetActive(false);
                _backBt.gameObject.SetActive(true);             // For main quest
            }
        }

        //TODO: Check if anything has changed and then change if possible as only objectives shoudl change for active quests
        private void UpdateQuestUI(Quest questInfo, int btIndex)
        {
            Debug.Log($"UpdateQuestUI | questInfo: {questInfo}");
            _paused = true;
            _questChoicesRect.SetActive(false);
            _pauseMenuRect.SetActive(true);
            _questContentRect.SetActive(true);
            _questRect.SetActive(true);

            _questTitleTxt.text = questInfo.name;
            // _questTitleTxt.text += _IN_PROGRESS;
            _questDescTxt.text = questInfo.description;

            if (btIndex != _DEFAULT_VAL)
            {
                _btQuestIndex[_currentBtIndex] = btIndex;
                _checkQuestTxts[_currentBtIndex].text = questInfo.name;            //Update button name
                _acceptQuestBt.gameObject.SetActive(true);

                if (questInfo.type == QuestType.MAIN_QUEST)
                    _backBt.gameObject.SetActive(false);

                _currentQuestType = questInfo.type;
            }
            //For quests already existing
            else
            {
                _currentQuestType = QuestType.FREE_ROAM;
                _questTitleTxt.text += _IN_PROGRESS;
                _acceptQuestBt.gameObject.SetActive(false);
            }

            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            for (int i = 0; i < questInfo.objectives.Count; i++)
            {
                stringBuilder.Append("[-] ");

                // Add a strikethrough if objective completed
                if (questInfo.objectives[i].current_count == questInfo.objectives[i].required_count)
                    stringBuilder.Append("<s>" + questInfo.objectives[i].description + "</s>");
                else
                    stringBuilder.Append(questInfo.objectives[i].description);

                stringBuilder.Append("\n");
            }

            _questObjectivesTxt.text = stringBuilder.ToString();
        }

        private void TestUpdateRewardTexts()
        {
            for (int i = 1; i < _TOTAL_QUEST_BTS; i++)
                _btQuestIndex[i] = i;

            Reward testReward = new Reward()
            {
                xp = 10,
                description = "You got test reward",
                item = "Test 00001",
                gold = 0
            };

            UpdateRewardTexts(testReward, 1);
        }

        private void UpdateRewardTexts(Reward questReward, int btIndex)
        {
            // Avoid if main quest 
            if (btIndex != 0)
            {
                //Disable the respective CheckQuest button
                _checkQuestBts[btIndex].gameObject.SetActive(false);

                //Re-arrange the list to correct the index
                for (int i = btIndex; (i + 1) < _TOTAL_QUEST_BTS; i++)
                {
                    _btQuestIndex[i] = _btQuestIndex[i + 1];
                }

                //Reset last index 
                if (btIndex != _TOTAL_QUEST_BTS - 1)
                    _btQuestIndex[_TOTAL_QUEST_BTS - 1] = -1;
                else
                    _btQuestIndex[btIndex] = -1;
                //Reduce current index
                _currentBtIndex = (_currentBtIndex - 1) == 0 ? _TOTAL_QUEST_BTS - 1 : _currentBtIndex--;
            }

            _questRect.SetActive(true);
            _rewardRect.SetActive(true);

            _questRewardDescTxt.text = questReward.description.ToString();
            _questRewardsTxt[0].text = questReward.xp.ToString();

            if (questReward.gold == 0)
                _questRewardsTxt[1].transform.parent.gameObject.SetActive(false);
            else
            {
                _questRewardsTxt[1].gameObject.SetActive(true);
                _questRewardsTxt[1].text = questReward.gold.ToString();
            }

            if (questReward.item.Equals(""))
                _questRewardsTxt[2].transform.parent.gameObject.SetActive(false);
            else
            {
                string itemStr = questReward.item.Substring(0, questReward.item.Length - 6);
                int itemCount;
                int.TryParse(questReward.item.Substring(questReward.item.Length - 6), out itemCount);
                itemStr += $" x {itemCount}";
                _questRewardsTxt[2].gameObject.SetActive(true);
                _questRewardsTxt[2].text = itemStr;
            }
        }
    }
}