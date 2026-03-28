using System;
using UnityEngine;

namespace Tartaria.Save
{
    /// <summary>
    /// Save Data schema — serialized to JSON at Application.persistentDataPath.
    /// Schema v2 — adds Anastasia, quest, workshop, and zone persistence.
    /// Forward-compatible: v1.0 saves must load in v5.0.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public SaveHeader header = new();
        public PlayerSaveData player = new();
        public WorldSaveData world = new();
        public AnastasiaSaveBlock anastasia = new();
        public QuestSaveBlock quests = new();
        public WorkshopSaveBlock workshop = new();
        public ZoneSaveBlock zone = new();
        public CorruptionSaveBlock corruption = new();
        public CampaignSaveBlock campaign = new();
        public SkillTreeSaveBlock skillTree = new();
        public CassianSaveBlock cassian = new();

        // v3 save blocks
        public EconomySaveBlock economy = new();
        public CodexSaveBlock codex = new();
        public ThorneSaveBlock thorne = new();
        public KorathSaveBlock korath = new();
        public TutorialSaveBlock tutorial = new();
        public DialogueTreeSaveBlock dialogueTree = new();
    }

    [Serializable]
    public class SaveHeader
    {
        public int schemaVersion = 3;
        public string gameVersion = "0.3.0";
        public string platform = "windows";
        public int saveSlot;
        public string createdUtc;
        public string modifiedUtc;
        public float playTimeSeconds;
        public string checksum;
    }

    [Serializable]
    public class PlayerSaveData
    {
        public SerializableVector3 position;
        public string currentZone = "echohaven";
        public float aetherCharge;
    }

    [Serializable]
    public class WorldSaveData
    {
        public float resonanceScore;
        public BuildingSaveState[] buildings = Array.Empty<BuildingSaveState>();
        public bool[] discoveredPOIs = Array.Empty<bool>();
        public string[] playedDialogueIds = Array.Empty<string>();
        public EnemySpawnState[] enemySpawns = Array.Empty<EnemySpawnState>();
    }

    [Serializable]
    public class BuildingSaveState
    {
        public string buildingId;
        public int state;          // 0=buried, 1=revealed, 2=tuning, 3=emerging, 4=active
        public bool[] nodesComplete = new bool[3];
        public float[] nodeAccuracy = new float[3];
        public float restorationProgress;
    }

    [Serializable]
    public class EnemySpawnState
    {
        public float rsThreshold;
        public bool hasSpawned;
    }

    /// <summary>
    /// JSON-serializable Vector3 (Unity's Vector3 isn't serializable to JSON).
    /// </summary>
    [Serializable]
    public struct SerializableVector3
    {
        public float x, y, z;

        public SerializableVector3(float x, float y, float z)
        {
            this.x = x; this.y = y; this.z = z;
        }

        public SerializableVector3(Vector3 v)
        {
            x = v.x; y = v.y; z = v.z;
        }

        public Vector3 ToVector3() => new(x, y, z);
    }

    // ─── v2 Save Blocks ──────────────────────────

    [Serializable]
    public class AnastasiaSaveBlock
    {
        public ulong bitmaskLow;
        public ulong bitmaskHigh;
        public ushort motesCollected;
        public int currentMoon = 1;
        public bool hasManifested;
        public bool postSolidWarmGlow;
        public int solidPhase;
    }

    [Serializable]
    public class QuestSaveBlock
    {
        public QuestSaveEntry[] entries = Array.Empty<QuestSaveEntry>();
    }

    [Serializable]
    public class QuestSaveEntry
    {
        public string questId;
        public int status; // QuestStatus cast to int
        public int[] objectiveProgress = Array.Empty<int>();
    }

    [Serializable]
    public class WorkshopSaveBlock
    {
        public WorkshopSaveEntry[] entries = Array.Empty<WorkshopSaveEntry>();
    }

    [Serializable]
    public class WorkshopSaveEntry
    {
        public string buildingId;
        public int tier;
    }

    [Serializable]
    public class ZoneSaveBlock
    {
        public int currentZoneIndex;
        public int highestZoneUnlocked;
        public ZoneBuildingSnapshot[] zoneSnapshots = Array.Empty<ZoneBuildingSnapshot>();
    }

    /// <summary>
    /// Per-zone snapshot of building states -- saved on zone exit, restored on zone re-entry.
    /// </summary>
    [Serializable]
    public class ZoneBuildingSnapshot
    {
        public int zoneIndex;
        public BuildingSaveState[] buildings = Array.Empty<BuildingSaveState>();
    }

    // ─── v3 Save Blocks (corruption, campaign, skills, cassian) ──

    [Serializable]
    public class CorruptionSaveBlock
    {
        public CorruptionSaveEntry[] entries = Array.Empty<CorruptionSaveEntry>();
    }

    [Serializable]
    public class CorruptionSaveEntry
    {
        public string buildingId;
        public float corruptionLevel;
        public int stage;     // PurgeStage cast to int
        public bool identified;
        public bool isolated;
        public bool purged;
    }

    [Serializable]
    public class CampaignSaveBlock
    {
        public int currentMoon;
        public int[] completedMoons = Array.Empty<int>();
    }

    [Serializable]
    public class SkillTreeSaveBlock
    {
        public int[] unlockedSkillIds = Array.Empty<int>();
    }

    [Serializable]
    public class CassianSaveBlock
    {
        public float trustLevel;
        public int interactionCount;
        public bool introduced;
        public string[] sharedIntelIds = Array.Empty<string>();
    }

    // ─── v3 Save Blocks (economy, codex, thorne, korath, tutorial, dialogue) ──

    [Serializable]
    public class EconomySaveBlock
    {
        public int aetherShards;
        public int resonanceCrystals;
        public int starFragments;
        public EconomyBuildingEntry[] buildings = Array.Empty<EconomyBuildingEntry>();
    }

    [Serializable]
    public class EconomyBuildingEntry
    {
        public string buildingId;
        public int baseIncome;
        public int outputType;
        public int level;
        public bool active;
    }

    [Serializable]
    public class CodexSaveBlock
    {
        public string[] unlockedEntryIds = Array.Empty<string>();
    }

    [Serializable]
    public class ThorneSaveBlock
    {
        public float trust;
        public bool introduced;
        public bool militiaActive;
        public int combatBriefingsGiven;
        public int zonesSecuredTogether;
    }

    [Serializable]
    public class KorathSaveBlock
    {
        public float trust;
        public bool introduced;
        public bool dayOutOfTimeRevealed;
        public int teachingsGiven;
        public int revelationsUnlocked;
        public float highestPlayerRS;
    }

    [Serializable]
    public class TutorialSaveBlock
    {
        public string[] completedStepIds = Array.Empty<string>();
        public bool tutorialFinished;
    }

    [Serializable]
    public class DialogueTreeSaveBlock
    {
        public string[] seenDialogueIds = Array.Empty<string>();
        public string[] chosenBranchIds = Array.Empty<string>();
    }
}
