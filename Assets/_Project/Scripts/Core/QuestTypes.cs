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
}
