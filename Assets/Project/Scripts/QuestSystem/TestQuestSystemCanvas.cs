using CurseOfNaga.DialogueSystem.Test;
using UnityEngine;
using UnityEngine.UI;

using static CurseOfNaga.Global.UniversalConstant;

namespace CurseOfNaga.QuestSystem.Test
{
    public class TestQuestSystemCanvas : MonoBehaviour
    {
        [SerializeField] private GameObject _questRect;
        [SerializeField] private Button _acceptRewardBt;
        [SerializeField] private TMPro.TMP_Text[] _questRewardsTxt;

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
        }

        private void Initialize()
        {
            TestDialogueMainManager.Instance.OnQuestCompleted += UpdateRewardTexts;
            TestDialogueMainManager.Instance.OnQuestUIUpdate += UpdateQuestUI;

            _acceptRewardBt.onClick.AddListener(AcceptReward);

            for (int i = 0; i < _questRewardsTxt.Length; i++)
            {
            }
        }

        private void AcceptReward()
        {
            Debug.Log($"Accepted Reward");
            _questRect.SetActive(false);
        }

        private void UpdateQuestUI(Quest questInfo)
        {
            Debug.Log($"UpdateQuestUI | questInfo: {questInfo}");
        }

        private void UpdateRewardTexts(Reward questReward)
        {
            _questRewardsTxt[0].text = questReward.xp.ToString();
            _questRewardsTxt[1].text = questReward.gold.ToString();
            _questRewardsTxt[2].text = questReward.itemID.ToString();
        }
    }
}