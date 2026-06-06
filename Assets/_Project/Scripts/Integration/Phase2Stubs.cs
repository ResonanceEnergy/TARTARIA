// PHASE 2 STUBS — Minimal implementations to satisfy Integration layer compilation
// These will be replaced with full implementations in Phase 2
// Rebuilt 2026-05-30 by Claude after Edit-tool CRLF truncation incident
using System.Collections.Generic;
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
        public static void FireCombatStarted() => OnCombatStarted?.Invoke();
        public static void FireCombatEnded() => OnCombatEnded?.Invoke();
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

    // BossType / BossResult / BossDefinition moved to BossEncounterSystem.cs (real impl).
    // Stubs removed 2026-06-01.

    // WaveEncounterDef moved to CombatWaveManager.cs (real impl). Duplicate stub removed 2026-06-01.

    // TutorialStep moved to TutorialSystem.cs (real impl). Duplicate stub removed 2026-06-01.

    public class AchievementSystem : MonoBehaviour
    {
        public static AchievementSystem Instance { get; private set; }
        void Awake() { if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); } }

        public class AchievementDef
        {
            public string id;
            public string title;
            public string description;
        }

        readonly HashSet<string> _unlocked = new HashSet<string>();

        public void Unlock(string id)
        {
            if (string.IsNullOrEmpty(id) || !_unlocked.Add(id)) return;
            Debug.Log($"[Achievement] Unlocked: {id}");
        }

        public void CheckBuildingRestored(int totalRestored, bool allPerfect)
        {
            if (totalRestored >= 1) Unlock("first_restoration");
            if (totalRestored >= 3) Unlock("three_buildings_restored");
            if (totalRestored >= 12) Unlock("echohaven_fully_restored");
            if (allPerfect) Unlock("perfect_restoration");
        }

        public void CheckMoonCompleted(int moonIndex)
        {
            Unlock($"moon_{moonIndex + 1}_complete");
        }

        public void CheckEnemyDefeated(string enemyId)
        {
            if (string.IsNullOrEmpty(enemyId)) return;
            Unlock($"defeat_{enemyId}");
        }

        public void CheckEnemyDefeated(int waveIndex, string enemyTag, bool wasBoss)
        {
            Unlock($"wave_{waveIndex}_{enemyTag}{(wasBoss ? "_boss" : "")}");
        }

        public void CheckSolidification() => Unlock("solidification");
        public void CheckDayOutOfTime() => Unlock("day_out_of_time");
        public void CheckZerethRedeemed() => Unlock("zereth_redeemed");
    }

    // CampaignFlowController moved to its own file (real impl). Duplicate stub removed 2026-06-01.

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

        float _currentRS;

        public void AwardRS(float amount, string reason = "stub")
        {
            _currentRS += amount;
            Debug.Log($"[GameLoopController STUB] AwardRS({amount}, {reason}) total={_currentRS}");
            GameEvents.FireRSChange(amount);
        }

        public void QueueRSReward(float amount, string reason = "stub")
        {
            _currentRS += amount;
            Debug.Log($"[GameLoopController STUB] QueueRSReward({amount}, {reason}) total={_currentRS}");
            GameEvents.FireRSChange(amount);
        }

        public float GetCurrentRS() => _currentRS;
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

        public static event System.Action<int> OnMoonCleared;

        readonly HashSet<string> _clearedBeats = new HashSet<string>();
        bool _moon3ContinentalRailUnlocked;

        void Awake() { if (Instance == null) Instance = this; }

        public bool IsCleared(int moonIndex) => false;
        public void ResetAll() { ClearedCount = 0; _clearedBeats.Clear(); _moon3ContinentalRailUnlocked = false; Debug.Log("[MoonProgressTracker STUB] ResetAll"); }

        public void MarkMoonCleared(int moonIndex)
        {
            ClearedCount++;
            Debug.Log($"[MoonProgressTracker STUB] Cleared moon {moonIndex}");
            OnMoonCleared?.Invoke(moonIndex);
        }

        public void MarkBeatCleared(int moonIndex, int beatIndex)
        {
            string key = $"{moonIndex}:{beatIndex}";
            if (_clearedBeats.Add(key)) Debug.Log($"[MoonProgressTracker STUB] Beat cleared moon={moonIndex} beat={beatIndex}");
        }

        public bool IsBeatCleared(int moonIndex, int beatIndex) =>
            _clearedBeats.Contains($"{moonIndex}:{beatIndex}");

        public void MarkMoon3ContinentalRailUnlocked()
        {
            if (_moon3ContinentalRailUnlocked) return;
            _moon3ContinentalRailUnlocked = true;
            Debug.Log("[MoonProgressTracker STUB] Moon3 Continental Rail unlocked");
        }
    }

    // VFXManager STUB
    public class VFXManager : MonoBehaviour
    {
        public static VFXManager Instance { get; private set; }
        void Awake() { if (Instance == null) Instance = this; }
    }

}
