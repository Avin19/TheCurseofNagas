using System.Collections.Generic;

namespace CurseOfNaga.QuestSystem
{
    [System.Serializable]
    public class Quest
    {
        public string uid, name, description, next_quest_uid;
        public QuestStatus status;
        public QuestType type;
        public List<QuestObjective> objectives;
        public List<string> prerequisites;
        public Reward reward;
    }

    /*
    *   - SUB_MAIN_QUEST : In order to unlock the Main Quest, mini main quests need to be done first. These are mandoatory
    *   - SIDE_QUEST : Some collectibles/xp/bonuses as rewards. These are optional
    */
    public enum QuestType { FREE_ROAM, MAIN_QUEST, SUB_MAIN_QUEST, SIDE_QUEST }

    public enum QuestStatus
    {
        LOCKED, UNLOCKED, NOT_STARTED, IN_PROGRESS, COMPLETED, FAILED,
        REQUESTED, REQUESTED_INFO, ACCEPTED, DECLINED
    }

    [System.Serializable]
    public class QuestObjective
    {
        public ObjectiveType type;
        public string description;
        public string target_id;
        public int required_count;
        public int current_count;
        public bool is_optional;
    }

    public enum ObjectiveType { KILL, COLLECT, TALK, EXPLORE, PUZZLE, FIND }

    [System.Serializable]
    public class Reward
    {
        public int xp;
        public string description;
        public string item;
        public int gold;
    }

    [System.Serializable]
    public class QuestGroupContent
    {
        public List<Quest> content;
    }

    [System.Serializable]
    public class QuestTemplate
    {
        public List<QuestGroupContent> quest_groups;
    }
}