// PHASE 2 STUBS — Minimal implementations to satisfy Integration layer compilation
// These will be replaced with full implementations in Phase 2
// Rebuilt 2026-05-30 by Claude after Edit-tool CRLF truncation incident
using UnityEngine;
using Tartaria.Core;
using Tartaria.Save;
using Tartaria.UI;
using Tartaria.Gameplay; // For ResonanceScannerSystem (vs disabled Integration stub)
using Unity.Entities;

namespace Tartaria.Integration
{
    // ─────────────────────────────────────────────────────────────────────
    // Stub MonoBehaviour singletons
    // ─────────────────────────────────────────────────────────────────────

    public class CameraShakeController : MonoBehaviour
    {
        public static CameraShakeController Instance { get; private set; }
        void Awake() { if (Instance == null) Instance = this; }
        public void Shake(float intensity, float duration) => Debug.Log($"[Stub] CameraShake: {intensity}, {duration}s");
    }

    // ─────────────────────────────────────────────────────────────────────
    // Tutorial enums
    // ─────────────────────────────────────────────────────────────────────

    public enum TutorialStepExtended
    {
        Discovery,
        HarmonicStrike,
        ResonancePulse,
        FrequencyShield,
        Restoration,
        Combat,
        Complete
    }

    // ─────────────────────────────────────────────────────────────────────
    // ResonanceScannerSystem extension stub (provides RegisterPOI/PerformScan
    // signatures the Phase 1 callers use; real impl is in Tartaria.Gameplay)
    // ─────────────────────────────────────────────────────────────────────

    public static class ResonanceScannerSystemStub
    {
        public static void RegisterPOI(this ResonanceScannerSystem scanner, Vector3 position, string id, string label)
        {
            Debug.Log($"[Stub] RegisterPOI: {id} at {position}");
        }

        public static void PerformScan(this ResonanceScannerSystem scanner)
        {
            Debug.Log("[Stub] PerformScan");
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // Missing component / data stubs
    // ─────────────────────────────────────────────────────────────────────

    public struct ResonanceShardComponent : IComponentData
    {
        public int count;
    }

    public class TutorialSaveData
    {
        public int currentStep;
        public bool[] completedSteps;
        public int currentIndex;
        public bool finished;
    }

    public class CollectibleEventArgs
    {
        public string collectibleID;
        public float rsReward;
    }

    public class TuningNodeEventArgs
    {
        public string nodeId;
        public float accuracy;
    }

    public class QuestReward
    {
        public int xp;
        public float rs;
        public string itemId;
    }

    public class ZoneEventArgs
    {
        public string zoneName;
        public Vector3 position;
    }

    public class TuningNodeEventArgsLegacy
    {
        public string nodeId;
    }


    // ─────────────────────────────────────────────────────────────────────
    // GameEvents Phase 2 extensions
    // ─────────────────────────────────────────────────────────────────────

    public static class GameEventsPhase2
    {
        public static event System.Action<CollectibleEventArgs> OnCollectibleGathered;
        public static event System.Action OnCombatStarted;
        public static event System.Action OnCombatEnded;
        public static event System.Action<TuningNodeEventArgs> OnTuningNodeActivated;

        public static void FireCollectibleGathered(string id, float rs) => OnCollectibleGathered?.Invoke(new CollectibleEventArgs { collectibleID = id, rsReward = rs });
        public static void FireAchievementUnlocked(string id) => Debug.Log($"[Stub] Achievement unlocked: {id}");
        public static void FireCompanionTrustChanged(string companionName, int trust) => Debug.Log($"[Stub] {companionName} trust: {trust}");
        public static void FireLeverPulled(string leverId) => Debug.Log($"[Stub] Lever pulled: {leverId}");
        public static void FireMoonProgressUpdate(int moon, float progress) => Debug.Log($"[Stub] Moon {moon} progress: {progress}%");
        public static void FirePlayerEnteredZone(string zone) => Debug.Log($"[Stub] Entered zone: {zone}");
        public static void FireTutorialStep(TutorialStep step) => Debug.Log($"[Stub] Tutorial step: {step}");
        public static void FireTuningNodeActivated(string nodeId) => OnTuningNodeActivated?.Invoke(new TuningNodeEventArgs { nodeId = nodeId });
    }

    // ─────────────────────────────────────────────────────────────────────
    // LeanTween stub (real lib in Phase 2)
    // ─────────────────────────────────────────────────────────────────────

    public static class LeanTweenStub
    {
        // Empty stub for LeanTween calls - will be replaced by actual library in Phase 2
    }

    // ─────────────────────────────────────────────────────────────────────
    // Phase 2 TYPE STUBS for disabled-system references
    // (BossEncounterSystem, AchievementSystem, CampaignFlowController,
    // AirshipFleetManager, CombatWaveManager, TutorialSystem)
    // Handlers in active files compile against these types but never fire
    // because their event sources are .cs.disabled. Delete this block in
    // Phase 2 when real systems are restored.
    // ─────────────────────────────────────────────────────────────────────

    public enum BossType { None, MudGolem, ResetCaptain, Cassian, Korath, Veritas, Zereth, Anastasia }

    public class BossResult
    {
        public string bossName;
        public float performanceScore;
        public int rsAwarded;
    }

    public class BossDefinition
    {
        public string bossName;
        public BossType bossType;
        public int maxHealth;
    }

    public class WaveEncounterDef
    {
        public string encounterId;
        public int waveCount;
        public float rsThreshold;
    }

    public enum TutorialStep
    {
        None,
        Movement,
        Discovery,
        ResonanceVision,
        FirstTuning,
        Combat,
        Restoration,
        Complete
    }

    public class AchievementSystem
    {
        public class AchievementDef
        {
            public string id;
            public string title;
            public string description;
        }
    }

    public class CampaignFlowController
    {
        public enum EndingPath { None, Restoration, Transcendence, Echo, Reset, TrueName }
    }

    public class AirshipFleetManager
    {
        public enum FleetFormation { None, Line, Wedge, Diamond, Cluster, Convoy }
    }



    // ─────────────────────────────────────────────────────────────────────
    // GameLoopController STUB — Moon 1 RS reward routing.
    // Real impl was in Integration/GameLoopController.cs (3,653 lines, disabled
    // due to ~30 cross-Moon dependencies). Phase 2 will restore a leaner version.
    // ─────────────────────────────────────────────────────────────────────

    public class GameLoopController : MonoBehaviour
    {
        public static GameLoopController Instance { get; private set; }
        void Awake() { if (Instance == null) Instance = this; }

        public void AwardRS(float amount, string reason = "stub")
        {
            Debug.Log($"[GameLoopController STUB] AwardRS({amount}, {reason})");
            // Route through Core RS event so HUD/Music/VFX still see it
            GameEvents.FireRSChange(amount);
        }

        public void QueueRSReward(float amount, string reason = "stub")
        {
            Debug.Log($"[GameLoopController STUB] QueueRSReward({amount}, {reason})");
            GameEvents.FireRSChange(amount);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // EchohavenContentSpawner moved to its own file 2026-05-31 hygiene pass:
    //   Assets/_Project/Scripts/Integration/EchohavenContentSpawner.cs
    // (It's not a stub — it has a real Mud Golem spawn impl, so it didn't belong here.)
    // ─────────────────────────────────────────────────────────────────────

    // ─────────────────────────────────────────────────────────────────────
    // MoonProgressTracker STUB — multi-Moon progress (only Moon 1 active).
    // ─────────────────────────────────────────────────────────────────────

    public class MoonProgressTracker : MonoBehaviour
    {
        public static MoonProgressTracker Instance { get; private set; }
        public const int MoonCount = 13;
        public int ClearedCount { get; private set; } = 0;
        void Awake() { if (Instance == null) Instance = this; }

        public bool IsCleared(int moonIndex) => false;
        public void ResetAll() { ClearedCount = 0; Debug.Log("[MoonProgressTracker STUB] ResetAll"); }
    }

    // VFXManager STUB
    public class VFXManager : MonoBehaviour
    {
        public static VFXManager Instance { get; private set; }
        void Awake() { if (Instance == null) Instance = this; }
    }

}
