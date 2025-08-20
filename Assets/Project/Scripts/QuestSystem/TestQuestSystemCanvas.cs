using CurseOfNaga.DialogueSystem.Test;
using UnityEngine;
using UnityEngine.UI;

using static CurseOfNaga.Global.UniversalConstant;

namespace CurseOfNaga.QuestSystem.Test
{
    public class TestQuestSystemCanvas : MonoBehaviour
    {
        [SerializeField] private GameObject _questRect, _rewardRect;
        [SerializeField] private Button _acceptRewardBt, _acceptQuestBt;
        [SerializeField] private TMPro.TMP_Text[] _questRewardsTxt;
        [SerializeField] private TMPro.TMP_Text _questTitleTxt, _questDescTxt, _questObjectivesTxt, _questRewardDescTxt;

        private int _currDialogueIndex;
        private const int _ACTIVE = 1, _INACTIVE = 0, _DEFAULT_VALUE = -1;

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

            _acceptRewardBt.onClick.AddListener(AcceptReward);
            _acceptRewardBt.onClick.AddListener(AcceptReward);
        }

        private void AcceptReward()
        {
            Debug.Log($"Accepted Reward");
            _questRect.SetActive(false);
            _rewardRect.SetActive(false);
        }

        private void UpdateQuestUI(Quest questInfo)
        {
            Debug.Log($"UpdateQuestUI | questInfo: {questInfo}");
            _questRect.SetActive(true);

            _questTitleTxt.text = questInfo.name;
            _questDescTxt.text = questInfo.description;

            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            for (int i = 0; i < questInfo.objectives.Count; i++)
            {
                stringBuilder.Append($"[-] ");
                stringBuilder.Append(questInfo.objectives[i].description);
                stringBuilder.Append("\n");
            }

            _questObjectivesTxt.text = stringBuilder.ToString();
        }

        private void TestUpdateRewardTexts()
        {
            Reward testReward = new Reward()
            {
                xp = 10,
                description = "You got test reward",
                item = "Test 00001",
                gold = 0
            };

            UpdateRewardTexts(testReward);
        }

        private void UpdateRewardTexts(Reward questReward)
        {
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