using System;
using UnityEngine;

namespace Tartaria.Core
{
    /// <summary>
    /// Static event bus for cross-assembly decoupling.
    /// Avoids circular asmdef references between Input, UI, and Integration.
    /// </summary>
    public static class GameEvents
    {
        public static event Action OnToggleAetherVision;
        public static event Action OnTogglePause;
        public static event Action<string, float> OnRequestPurgeCorruption;
        public static event Action OnRequestActivateRSBuff;
        public static event Action<float> OnRSChanged;
        public static event Action<string> OnBuildingRestored;   // buildingId
        public static event Action<string, Vector3> OnBuildingDiscovered; // buildingName, position

        // Round 4 Performance: auto-fallback UI feedback event (tierName, reason)
        public static event System.Action<string, string> OnPerformanceFallback;

        // Phase 3 R5 Save & Cloud: Critical auto-save triggers (fountain restore, Moon 3 adoption, boss defeat, etc.)
        // SaveManager subscribes and forces immediate Save() + cloud queue + toast for these.
        public static event System.Action<string> OnCriticalSaveTrigger;   // e.g. "fountain_restored", "moon3_adopted", "boss_defeated"

        // Player-facing cloud conflict: fired when auto-merge is insufficient and UI choice needed.
        public static event System.Action<SaveConflictInfo> OnCloudConflictDetected;

        // R6 Save & Cloud: Push notification arrival hook (Firebase/Steam remote wake for conflicts, 17th Hour events, cross-device sync)
        public static event System.Action<string> OnRemotePushNotificationReceived;

        public static void FireToggleAetherVision() => OnToggleAetherVision?.Invoke();
        public static void FireTogglePause() => OnTogglePause?.Invoke();
        public static void FireRequestPurgeCorruption(string buildingId, float amount) => OnRequestPurgeCorruption?.Invoke(buildingId, amount);
        public static void FireRequestActivateRSBuff() => OnRequestActivateRSBuff?.Invoke();
        public static void FireRSChange(float amount) => OnRSChanged?.Invoke(amount);
        public static void FireBuildingRestored(string buildingId) => OnBuildingRestored?.Invoke(buildingId);
        public static void FireBuildingDiscovered(string buildingName, Vector3 position) => OnBuildingDiscovered?.Invoke(buildingName, position);

        public static void FirePerformanceFallback(string tierName, string reason) => OnPerformanceFallback?.Invoke(tierName, reason);

        // R5 Save triggers
        public static void FireCriticalSaveTrigger(string reason) => OnCriticalSaveTrigger?.Invoke(reason);

        public static void FireCloudConflictDetected(SaveConflictInfo info) => OnCloudConflictDetected?.Invoke(info);

        // R6: Push / remote event (deeper offline + production cloud polish)
        public static void FireRemotePushNotification(string payload) => OnRemotePushNotificationReceived?.Invoke(payload);

        // Moon 3 payoff: continental rail fast travel unlocked (gameplay → integration decoupling)
        public static event System.Action OnMoon3FastTravelUnlocked;
        public static void FireMoon3FastTravelUnlocked() => OnMoon3FastTravelUnlocked?.Invoke();

        // Moon progress — fired by MoonProgressTracker (Integration) to notify UI/HUD without direct dep
        public static event System.Action<int>  OnMoonCleared;           // moonNum 1-13
        public static void FireMoonCleared(int moonNum) => OnMoonCleared?.Invoke(moonNum);

        // HUD toast / cloud toast — fired by Save assembly so UI can display without a direct Save→UI dep
        public static event System.Action<string>               OnHUDAchievementToast;
        public static event System.Action<string>               OnHUDCloudQueueToast;
        public static event System.Action<string, string, string> OnHUDSaveConflictPrompt;  // (localSummary, cloudSummary, action)
        public static void FireHUDAchievementToast(string msg)                                             => OnHUDAchievementToast?.Invoke(msg);
        public static void FireHUDCloudQueueToast(string msg)                                              => OnHUDCloudQueueToast?.Invoke(msg);
        public static void FireHUDSaveConflictPrompt(string localSummary, string cloudSummary, string act) => OnHUDSaveConflictPrompt?.Invoke(localSummary, cloudSummary, act);
    }

    /// <summary>
    /// Payload for player-facing save conflict UI (Phase 3 R5). Contains summary stats for "This Device vs Cloud" dialog.
    /// </summary>
    [Serializable]
    public class SaveConflictInfo
    {
        public string localModified;
        public string cloudModified;
        public float localPlayTime;
        public float cloudPlayTime;
        public int localBuildingsRestored;
        public int cloudBuildingsRestored;
        public int localMoon;
        public int cloudMoon;
        public string recommendedAction; // "merge", "local", "cloud"
        public string details;
    }
}
