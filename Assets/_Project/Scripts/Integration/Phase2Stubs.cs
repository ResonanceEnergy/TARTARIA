// PHASE 2 STUBS — Minimal implementations to satisfy Integration layer compilation
// These will be replaced with full implementations in Phase 2
using UnityEngine;
using Tartaria.Core;
using Tartaria.Save;
using Tartaria.UI;

namespace Tartaria.Integration
{
    // Stub classes for missing systems
    public class CameraShakeController : MonoBehaviour
    {
        public static CameraShakeController Instance { get; private set; }
        void Awake() { if (Instance == null) Instance = this; }
        public void Shake(float intensity, float duration) => Debug.Log($"[Stub] CameraShake: {intensity}, {duration}s");
    }

    // Tutorial step enum extensions
    public enum TutorialStepExtended
    {
        Discovery,
        HarmonicStrike,
        ResonancePulse,
        FrequencyShield,
        WorkshopUpgrade,
        BuildingRestore
    }

    // Extend SaveData with missing properties (partial class)
    public static class SaveDataExtensions
    {
        // These properties will be added to SaveData in Phase 2
        // For now, return empty/default values to satisfy Integration layer
        public static object achievementData = null;
        public static object airshipFleet = null;
        public static object anastasia = null;
        public static object tutorialSaveData = null;
    }

    // Stub ResonanceScannerSystem extensions
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

    // Missing component stubs
    public struct ResonanceShardComponent
    {
        public int count;
    }

    public class TutorialSaveData
    {
        public int currentStep;
        public bool[] completedSteps;
        public int currentIndex; // Phase 2
        public bool finished; // Phase 2
    }

    // GameEvents Phase 2 extensions (static partial not allowed, use separate class)
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

    public class CollectibleEventArgs
    {
        public string collectibleID;
        public float rsReward;
        public string collectibleType; // Phase 2
        public Vector3 position; // Phase 2
    }

    public class TuningNodeEventArgs
    {
        public string nodeId;
        public int nodesActivated; // Phase 2
        public int totalNodes; // Phase 2
        public Vector3 position; // Phase 2
    }
}
