using System;
using UnityEngine;

namespace Tartaria.Save
{
    /// <summary>
    /// Save Data schema — serialized to JSON at Application.persistentDataPath.
    /// Schema v8 — see version blocks below for incremental additions.
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

        // v4 save blocks
        public MiloSaveBlock milo = new();
        public LiraelSaveBlock lirael = new();
        public ZerethSaveBlock zereth = new();

        // v5 save blocks
        public VeritasSaveBlock veritas = new();

        // v6 save blocks
        public AirshipFleetSaveBlock airshipFleet = new();
        public LeyLineProphecySaveBlock leyLineProphecy = new();
        public BellTowerSyncSaveBlock bellTowerSync = new();
        public GiantModeSaveBlock giantMode = new();
        public WorldChoiceSaveBlock worldChoice = new();
        public AchievementSaveBlock achievementData = new();
        public DialogueArcSaveBlock dialogueArcs = new();

        // v7 save blocks
        public ExcavationSaveBlock excavation = new();
        public CraftingSaveBlock crafting = new();
        public ScannerSaveBlock scanner = new();
        public RailSaveBlock rail = new();
        public AquiferPurgeSaveBlock aquiferPurge = new();
        public CosmicConvergenceSaveBlock cosmicConvergence = new();
        public DayOutOfTimeSaveBlock dayOutOfTime = new();
        public CompanionManagerSaveBlock companionManager = new();

        // v8 save blocks
        public CombatWaveSaveBlock combatWave = new();

        // v9 save blocks
        public ArchiveSaveBlock archive = new();
    }

    [Serializable]
    public class SaveHeader
    {
        public int schemaVersion = 9;
        public string gameVersion = "0.8.0";
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
        public int harmonicFragments;
        public int echoMemories;
        public int crystallineDust;
        public int forgeTokens;
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
        public int[] completedSteps = Array.Empty<int>(); // Numeric step indices for linear tutorial
    }

    [Serializable]
    public class DialogueTreeSaveBlock
    {
        public string[] seenDialogueIds = Array.Empty<string>();
        public string[] chosenBranchIds = Array.Empty<string>();
    }

    // ─── v4 Save Blocks (Milo, Lirael) ──────────

    [Serializable]
    public class MiloSaveBlock
    {
        public float trust;
        public bool introduced;
        public int artifactsAppraised;
        public int jokesDelivered;
        public int sincereMoments;
        public bool orphanTrainWitnessed;
        public bool whiteCityOutburst;
        public bool korathSacrificeWitnessed;
    }

    [Serializable]
    public class LiraelSaveBlock
    {
        public float trust;
        public bool introduced;
        public float solidity;
        public int songsRemembered;
        public int dissonanceWarningsGiven;
        public bool orphanTrainRemembered;
        public bool childrenChoirConducted;
        public bool korathSongsLearned;
        public bool fountainHealed;
        public bool fullyManifested;
    }

    [Serializable]
    public class ZerethSaveBlock
    {
        public float presenceLevel;
        public int phase;
        public int prophecyStonesTriggered;
        public int voiceResponsesPlayed;
        public bool korathRevelationHeard;
        public bool triggerRoomDiscovered;
        public bool physicallyManifested;
        public bool finalConfrontationStarted;
        public bool redeemed;
    }

    // ─── v5 Save Blocks (Veritas) ────────────────

    [Serializable]
    public class VeritasSaveBlock
    {
        public float trust;
        public bool introduced;
        public int trustTier;             // VeritasTrustTier cast to int
        public int registersRestored;
        public float performanceAccuracy;
        public bool requiemPerformed;
        public bool bellTowerSyncComplete;
        public bool finalNoteDelivered;
    }

    // ─── v6 Save Blocks (AirshipFleet, LeyLine, BellTower, GiantMode, WorldChoice, Achievement) ──

    [Serializable]
    public class AirshipFleetSaveBlock
    {
        public int[] shipStates = Array.Empty<int>();
        public float[] shipHealth = Array.Empty<float>();
        public int[] shipMercuryOrbs = Array.Empty<int>();
        public bool[] shipRestored = Array.Empty<bool>();
        public int formation;
        public int shipsRestored;
        public int totalMercuryOrbsTuned;
        public bool fleetOperational;
    }

    [Serializable]
    public class LeyLineProphecySaveBlock
    {
        public bool[] stonesActivated = Array.Empty<bool>();
        public int stonesCompleted;
        public float dreamspellClock;
        public bool miniGameActive;
    }

    [Serializable]
    public class BellTowerSyncSaveBlock
    {
        public float[] towerFrequencies = Array.Empty<float>();
        public int towersSynced;
        public float resonanceScore;
        public bool miniGameActive;
        public bool cascadeTriggered;
    }

    [Serializable]
    public class GiantModeSaveBlock
    {
        public int totalActivations;
        public int buildingsLifted;
        public int rubbleCleared;
        public float totalTimeAsGiant;
    }

    [Serializable]
    public class WorldChoiceSaveBlock
    {
        public int[] choiceIds = Array.Empty<int>();
        public int[] choiceValues = Array.Empty<int>();
    }

    [Serializable]
    public class AchievementSaveBlock
    {
        public string[] unlockedIds = Array.Empty<string>();
        public string[] progressKeys = Array.Empty<string>();
        public float[] progressValues = Array.Empty<float>();
        public int totalUnlocked;
    }

    [Serializable]
    public class DialogueArcSaveBlock
    {
        public int[] companionIds = Array.Empty<int>();
        public int[] trustLevels = Array.Empty<int>();
        public string[] seenKeys = Array.Empty<string>();
    }

    // ─── v7 Save Blocks (Excavation, Crafting, Scanner, Rail, AquiferPurge, CosmicConvergence, DotT, Companion) ──

    [Serializable]
    public class ExcavationSaveBlock
    {
        public string[] discoveredSiteIds = Array.Empty<string>();
        public int[] siteStages = Array.Empty<int>();
        public float[] siteProgress = Array.Empty<float>();
        public int totalExcavations;
    }

    [Serializable]
    public class CraftingSaveBlock
    {
        public string[] knownRecipeIds = Array.Empty<string>();
        public string[] inventoryItemIds = Array.Empty<string>();
        public int[] inventoryItemCounts = Array.Empty<int>();
        public int totalCrafted;
    }

    [Serializable]
    public class ScannerSaveBlock
    {
        public string[] scannedObjectIds = Array.Empty<string>();
        public float scannerRange;
        public int scannerLevel;
        public int totalScans;
    }

    [Serializable]
    public class RailSaveBlock
    {
        public bool[] segmentRestored = Array.Empty<bool>();
        public bool[] segmentHasBoss = Array.Empty<bool>();
        public float[] segmentCorruption = Array.Empty<float>();
        public bool[] stationsDiscovered = Array.Empty<bool>();
        public int segmentsRestored;
        public bool networkComplete;
        public bool trainActive;
        public int trainCurrentStation;
    }

    [Serializable]
    public class AquiferPurgeSaveBlock
    {
        public int[] layerStates = Array.Empty<int>();
        public float[] layerPurity = Array.Empty<float>();
        public float[] layerAccuracy = Array.Empty<float>();
        public int currentLayer;
    }

    [Serializable]
    public class CosmicConvergenceSaveBlock
    {
        public int currentPhase;
        public bool[] phasesComplete = Array.Empty<bool>();
        public float[] phaseAccuracy = Array.Empty<float>();
        public float convergenceScore;
    }

    [Serializable]
    public class DayOutOfTimeSaveBlock
    {
        public bool eventCompleted;
        public int festivalCurrency;
        public int currentMemoryZone;
        public float bestChallengeScore;
    }

    [Serializable]
    public class CompanionManagerSaveBlock
    {
        public string[] companionIds = Array.Empty<string>();
        public bool[] companionUnlocked = Array.Empty<bool>();
        public float[] companionTrust = Array.Empty<float>();
    }

    // ─── v8 Save Blocks (CombatWave) ─────────────

    [Serializable]
    public class CombatWaveSaveBlock
    {
        public bool encounterActive;
        public int currentWaveIndex;
        public int enemiesRemaining;
        public int totalWaves;
    }

    // ─── v9 Save Blocks (Archive) ─────────────────

    [Serializable]
    public class ArchiveSaveBlock
    {
        // Ids of entries the player has unlocked
        public string[] unlockedEntryIds = System.Array.Empty<string>();
        // Cumulative RS earned (used by ArchiveManager for tier unlock tracking)
        public float cumulativeRS;
    }
}
