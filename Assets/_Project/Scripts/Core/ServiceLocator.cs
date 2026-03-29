using UnityEngine;

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
    }

    public interface ILiraelService
    {
        void AddTrust(float amount);
        void ConductChildrenChoir();
        void RememberOrphanTrain();
    }

    public interface ICampaignService
    {
        int CurrentMoonIndex { get; }
    }

    public interface IZoneTransitionService
    {
        void TransitionToZone(int zoneIndex);
    }
}
