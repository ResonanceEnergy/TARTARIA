using System;
using UnityEngine;

namespace Tartaria.Core
{
    /// <summary>
    /// Quest objective types for tracking player progress across 13 Moons campaign.
    /// Each type corresponds to a different gameplay mechanic or progression milestone.
    /// </summary>
    public enum QuestObjectiveType
    {
        // Social/dialogue objectives
        TalkToNPC,
        DiscoverLocation,
        
        // Combat objectives
        DefeatEnemies,
        DefeatBoss,
        SurviveWaves,
        
        // Exploration objectives
        ReachDestination,
        ActivateLandmark,
        CollectItems,
        
        // Puzzle/restoration objectives
        TuneResonator,
        RestoreBuilding,
        AlignBastions,
        RepairPipes,
        FloodMoats,
        TuneCrystals,
        
        // Moon-specific objectives
        PurgeCrystals,       // Moon 2: Dissonance crystal purge
        FreeOrphans,         // Moon 3: Orphan train escort
        ActivateFountains,   // Moon 6: Hydraulic fountain network
        ThawKorath,          // Moon 7: Multi-session ice thaw
        TuneAirships,        // Moon 8: Mercury-orb airship tuning
        CollectProphecyStones, // Moon 9: 6 prophecy stone collection
        
        // Multi-step chain objectives
        ChainObjective,
        AnyOf,
        AllOf,
        
        // Generic fallback
        Custom
    }

    /// <summary>
    /// Quest objective state for tracking completion.
    /// </summary>
    public enum QuestObjectiveState
    {
        NotStarted,
        InProgress,
        Completed,
        Failed,
        Optional
    }

    /// <summary>
    /// Quest priority levels for UI sorting and player guidance.
    /// </summary>
    public enum QuestPriority
    {
        Critical,   // Main storyline, blocks progression
        Major,      // Major side content, significant rewards
        Minor,      // Optional side objectives
        Hidden      // Secret/achievement-based quests
    }

    /// <summary>
    /// Data structure for individual quest objective within a quest.
    /// </summary>
    [Serializable]
    public class QuestObjective
    {
        public string ObjectiveID;
        public string Description;
        public QuestObjectiveType Type;
        public QuestObjectiveState State;
        public int CurrentProgress;
        public int RequiredProgress;
        public bool IsOptional;
        
        public float ProgressPercent => RequiredProgress > 0 ? (float)CurrentProgress / RequiredProgress : 0f;
        public bool IsComplete => State == QuestObjectiveState.Completed;
    }

    /// <summary>
    /// Complete quest data structure (used by QuestDefinition and QuestManager).
    /// </summary>
    [Serializable]
    public class QuestData
    {
        public string QuestID;
        public string QuestName;
        public string Description;
        public int MoonNumber; // Which Moon this quest belongs to (1-13)
        public QuestPriority Priority;
        public QuestObjective[] Objectives;
        
        public bool IsComplete => Objectives != null && Objectives.Length > 0 && Array.TrueForAll(Objectives, obj => obj.IsComplete || obj.IsOptional);
        public float TotalProgressPercent
        {
            get
            {
                if (Objectives == null || Objectives.Length == 0) return 0f;
                float sum = 0f;
                foreach (var obj in Objectives)
                {
                    sum += obj.ProgressPercent;
                }
                return sum / Objectives.Length;
            }
        }
    }
}

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
        HiddenDiscovery = 15,
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
