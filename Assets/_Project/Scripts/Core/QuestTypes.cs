using System;
using UnityEngine;

namespace Tartaria.Core
{
    public enum QuestStatus : byte
    {
        Locked = 0,
        Active = 1,
        Completed = 2,
        Failed = 3
    }

    public enum QuestObjectiveType : byte
    {
        DiscoverBuilding = 0,
        RestoreBuilding = 1,
        DefeatEnemies = 2,
        ReachRS = 3,
        CompleteZone = 4,
        TalkToNPC = 5,
        CollectItem = 6,
        CompleteTuning = 7,
        CompleteMiniGame = 8,
        DefeatBoss = 9,
        CompanionMilestone = 10,
        CraftItem = 11,
        ExcavateRuin = 12,
        ReachAirshipDestination = 13,
        RaiseCompanionTrust = 14,
    }

    [Serializable]
    public struct QuestState
    {
        public QuestStatus status;
        public int[] objectiveProgress;
    }

    [Serializable]
    public class QuestObjective
    {
        public string description;
        public QuestObjectiveType type;
        public string targetId;
        public int targetCount = 1;
    }

    [CreateAssetMenu(menuName = "Tartaria/Quest Definition")]
    public class QuestDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string questId;
        public string displayName;
        [TextArea(2, 5)]
        public string description;

        [Header("Type")]
        public bool isMainQuest;
        public bool autoActivate;
        public float rsRequirement;

        [Header("Objectives")]
        public QuestObjective[] objectives;

        [Header("Rewards")]
        public float rsReward;

        [Header("Chain")]
        public string[] followUpQuestIds;
    }
}
