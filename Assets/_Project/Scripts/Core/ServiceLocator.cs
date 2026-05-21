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
        public static ICampaignService Campaign { get; set; }
        public static IZoneTransitionService ZoneTransition { get; set; }
        public static IAssetService Asset { get; set; }
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
    }

    public interface IHUDService
    {
        void ShowInteractionPrompt(string text);
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
