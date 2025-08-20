using System.Collections.Generic;

namespace CurseOfNaga.QuestSystem
{
    [System.Serializable]
    public class Quest
    {
        public string uid, name, description;
        public QuestStatus status;
        public QuestType type;
        public List<QuestObjective> objectives;
        public List<string> prerequisites;
        public Reward reward;
    }

    public enum QuestType { FREE_ROAM, MAIN_QUEST, SUB_MAIN_QUEST, SIDE_QUEST }

    public enum QuestStatus { NOT_STARTED, IN_PROGRESS, COMPLETED, FAILED, REQUESTED, ACCEPTED, REQUESTED_INFO }

    [System.Serializable]
    public class QuestObjective
    {
        public ObjectiveType type;
        public string description;
        public string targetID;
        public int requiredCount;
        public int currentCount;
        public bool isOptional;
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
    public class QuestTemplate
    {
        public List<Quest> quests_data;
    }
}