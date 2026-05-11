namespace Tartaria.Integration
{
    /// <summary>
    /// Steam SDK bridge stub: provides interface for achievements, rich presence, cloud saves.
    /// Real Steamworks.NET integration TODO (requires Steamworks package + App ID).
    /// </summary>
    public static class SteamBridge
    {
        static bool _initialized = false;

        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;
            UnityEngine.Debug.Log("[SteamBridge] Initialized (stub mode). Real Steamworks.NET integration pending.");
        }

        public static void UnlockAchievement(string achievementId)
        {
            UnityEngine.Debug.Log($"[SteamBridge] Achievement unlocked (stub): {achievementId}");
            // TODO: Steamworks.SteamUserStats.SetAchievement(achievementId);
            // TODO: Steamworks.SteamUserStats.StoreStats();
        }

        public static void SetRichPresence(string key, string value)
        {
            UnityEngine.Debug.Log($"[SteamBridge] Rich presence (stub): {key}={value}");
            // TODO: Steamworks.SteamFriends.SetRichPresence(key, value);
        }

        public static void SyncCloudSave(string filename, byte[] data)
        {
            UnityEngine.Debug.Log($"[SteamBridge] Cloud save sync (stub): {filename}, {data.Length} bytes");
            // TODO: Steamworks.SteamRemoteStorage.FileWrite(filename, data, data.Length);
        }

        public static byte[] LoadCloudSave(string filename)
        {
            UnityEngine.Debug.Log($"[SteamBridge] Cloud save load (stub): {filename}");
            // TODO: return Steamworks.SteamRemoteStorage.FileRead(filename, ...);
            return null;
        }
    }
}
