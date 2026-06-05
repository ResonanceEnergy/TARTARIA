using UnityEngine;
using System.Threading.Tasks;

namespace Tartaria.Core
{
    /// <summary>
    /// Cross-assembly service interfaces and locator.
    /// Gameplay calls these interfaces; Integration/UI classes register implementations.
    /// Breaks circular asmdef dependencies (Gameplay cannot reference Integration or UI).
    /// </summary>
    public static class ServiceLocator
    {
        public static IGameLoopService GameLoop { get; set; }
        public static IVFXService VFX { get; set; }
        public static IHUDService HUD { get; set; }
        public static IMiloService Milo { get; set; }
        public static ILiraelService Lirael { get; set; }
        public static ICassianService Cassian { get; set; }
        public static ICampaignService Campaign { get; set; }
        public static IZoneTransitionService ZoneTransition { get; set; }
        public static IAssetService Asset { get; set; }
        public static ICameraShakeService CameraShake { get; set; }
        public static ICombatService Combat { get; set; }
        public static IQuestService Quest { get; set; }
        public static IMoonMechanicService MoonMechanic { get; set; }
        public static ISaveService Save { get; set; }
        public static ICompanionService Companion { get; set; }
        public static IMoonProgressService MoonProgress { get; set; }
        public static IMoon2ProgressionService Moon2Progression { get; set; }
    }

    public interface IGameLoopService
    {
        void OnMiniGameCompleted(float rsReward, string miniGameType);
        void OnBuildingDiscovered(string buildingName, Vector3 position);
        void QueueRSReward(float amount, string source);
    }

    public interface IVFXService
    {
        void PlayEffect(VFXEffect effect, Vector3 position);
        // Moon 3 train escort visual hooks (rebuilt with the VFXController re-implementation).
        void SpawnMoon3TrainTrail(Vector3 position, float scale);
        void SpawnLeviathanPhaseVFX(Vector3 position, int phaseIndex);
        void SpawnGiantEchoRelease(Vector3 position);
        // Moon 2 cavern visual hooks (crystal resonance, ley line restoration, discovery flourish).
        void PlayResonancePulse(Vector3 position, float radius);
        void PlayLeyLineRestore(Vector3 start, Vector3 end);
        void PlayDiscoveryBurst(Vector3 position);
        void SpawnAuroraFountain(Vector3 origin, float height);
        void TriggerOvertoneThread(Vector3 from, Vector3 to, float intensity);

        // ─── Moon 3 Rail Escort "Compassion & Rails" VFX (R7 full integration) ───
        // Spectral orphans' lullaby glow + particles when singing (tells the compassion story: children 's voices calm the storm)
        void SpawnOrphanLullabyGlow(Vector3 position, int childCount, float intensity);
        // Train damage state sparks / cracks synced to low health (protection fantasy)
        void SpawnRailDamageSparks(Vector3 position, float damageSeverity);
        // Wind + electric atmosphere reacting to lullaby success (bright golden wind) vs failure (dark electric)
        void SpawnWindElectricReaction(Vector3 position, bool success, float intensity);
        // Post-victory permanent world transformation: golden rails across highlands + calmed wind particles + fast travel visual
        void TriggerPermanentGoldenRailsAndCalm(Vector3 railStart, Vector3 railEnd);
    }

    public interface IHUDService
    {
        void ShowInteractionPrompt(string text);
        void ShowContextPrompt(string text);
        void HideContextPrompt();
        void ShowPurgeHoldPrompt(string actionLabel, float progress01);
        void HidePurgeHoldPrompt();
        void ShowObjective(string objective);
        void ShowBanner(string title, string body, float duration = 4f);
        void ShowAchievementToast(string title, string subtitle = "");
    }

    public interface IMiloService
    {
        void AddTrust(float amount);
        void WitnessOrphanTrain();
        // Moon 3 vertical slice additions
        void WitnessFirstOrphan(string orphanName);
        void BoardTrain(Vector3 positionOnTrain);
    }

    public interface ILiraelService
    {
        void AddTrust(float amount);
        void ConductChildrenChoir();
        void RememberOrphanTrain();
        // Moon 3 vertical slice additions: lullaby rhythm support + adoption truth + physical escort positioning
        void RememberFirstOrphan(string orphanName);
        void BeginLullabySupport();
        void BoardTrain(Vector3 positionOnTrain);
        // Moon 3 R7 escort variant — board the train at escort start with optional roof position.
        void BoardTrainLiraelEscort(Vector3 positionOnTrain, bool onRoof);
        // Moon 2 first purge emotional anchor — called from Gameplay without cross-asm ref
        void ReactToFirstPurge();
    }

    /// <summary>
    /// Cassian companion service — Moon 3 R7 escort + later moons.
    /// </summary>
    public interface ICassianService
    {
        void AddTrust(float amount);
        void BoardTrain(Vector3 positionOnTrain);
    }

    /// <summary>
    /// Camera shake hook — registered by CameraController so Gameplay can trigger feedback without an asmdef dep.
    /// </summary>
    public interface ICameraShakeService
    {
        void TriggerShake(float intensity, float duration);
    }

    /// <summary>
    /// Combat read service — registered by CombatBridge.
    /// </summary>
    public interface ICombatService
    {
        float GetPlayerCurrentFrequency();
    }

    /// <summary>
    /// Quest progression service — registered by QuestManager.
    /// </summary>
    public interface IQuestService
    {
        void ProgressByType(QuestObjectiveType type, string targetId = null, int amount = 1);
    }

    /// <summary>
    /// Moon mechanic activator probe — registered by Integration; Gameplay queries to detect external mechanic ownership.
    /// </summary>
    public interface IMoonMechanicService
    {
        bool HasActivator(GameObject target);
    }

    public interface ICampaignService
    {
        int CurrentMoonIndex { get; }
    }

    public interface IZoneTransitionService
    {
        void TransitionToZone(int zoneIndex);
    }

    /// <summary>
    /// Save service abstraction — registered by SaveManager so UI tier (PauseMenu) can save without an asmdef ref to Save (Save references UI, would create cycle).
    /// </summary>
    public interface ISaveService
    {
        void Save();
        void MarkDirty();
        int GetCurrentSlot();
        bool HasAnySave();
        /// <summary>Returns a brief "Slot N • MM/dd HH:mm" label for the current slot, or empty if none.</summary>
        string GetCurrentSaveLabel();
    }

    /// <summary>Companion trust/state service — registered by CompanionManager.</summary>
    public interface ICompanionService
    {
        void AddTrust(string companionId, float amount);
    }

    /// <summary>Moon beat/clear tracking — registered by MoonProgressTracker.</summary>
    public interface IMoonProgressService
    {
        void MarkBeatCleared(int moonNum, int beatIndex);
        void MarkCleared(int moonNum);
        int ClearedCount { get; }
    }

    /// <summary>Moon 2 progression persistence — registered by Moon2ProgressionSystem.</summary>
    public interface IMoon2ProgressionService
    {
        bool IsSitePurged(string siteId);
        void RegisterFirstPurge(string siteId);
        void OnFirstVeinPurgedEvent();
    }

    /// <summary>
    /// Asset streaming / loading abstraction (Addressables backed).
    /// Allows Gameplay and other to load without direct asmdef dependency on Addressables.
    /// </summary>
    public interface IAssetService
    {
        Task<GameObject> LoadPrefabAsync(string keyOrLabel, bool useLabel = false);
        Task<GameObject> InstantiateAsync(string keyOrLabel, Vector3 position, Quaternion rotation, Transform parent = null, bool useLabel = false);
        void Release(string keyOrLabel, bool useLabel = false);
        void ReleaseGroup(string label);
        AddressableAssetLoader.StreamingRingState GetRingState(Vector3 playerPos, Vector3 zoneCenter);
    }
}
