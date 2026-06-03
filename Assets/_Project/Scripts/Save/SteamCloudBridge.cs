// Reflection bridge to Tartaria.Integration.SteamBridge.
// Save asmdef cannot reference Integration (Integration → Save). This bridge resolves
// the SteamBridge static API at runtime so SaveManager can drive Steam Cloud sync
// without an asmdef cycle.
using System;
using System.Reflection;
using UnityEngine;

namespace Tartaria.Save
{
    internal static class SteamBridge
    {
        static Type _t;
        static bool _resolved;

        static Type T()
        {
            if (_resolved) return _t;
            _resolved = true;
            try
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var t = asm.GetType("Tartaria.Integration.SteamBridge", false);
                    if (t != null) { _t = t; break; }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Save.SteamBridge] Type resolve failed: {ex.GetType().Name}: {ex.Message}
  context: AppDomain assembly scan for Tartaria.Integration.SteamBridge
{ex.StackTrace}");
                // Fallback: _t stays null, Steam features will report unavailable.
            }
            return _t;
        }

        public static bool IsSteamAvailable
        {
            get
            {
                var t = T(); if (t == null) return false;
                try
                {
                    var p = t.GetProperty("IsSteamAvailable", BindingFlags.Public | BindingFlags.Static);
                    if (p != null) return (bool)p.GetValue(null);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[Save.SteamBridge] IsSteamAvailable reflection failed: {ex.GetType().Name}: {ex.Message}
  context: type={t?.FullName}
{ex.StackTrace}");
                    // Fallback: report Steam unavailable on reflection failure.
                }
                return false;
            }
        }

        public static bool SyncCloudSave(string filename, byte[] bytes)
        {
            var t = T(); if (t == null) return false;
            try
            {
                var m = t.GetMethod("SyncCloudSave", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string), typeof(byte[]) }, null);
                if (m != null) return (bool)m.Invoke(null, new object[] { filename, bytes });
            }
            catch (Exception ex) { Debug.LogWarning("[Save.SteamBridge] SyncCloudSave reflection error: " + ex.Message); }
            return false;
        }

        public static byte[] LoadCloudSave(string filename)
        {
            var t = T(); if (t == null) return null;
            try
            {
                var m = t.GetMethod("LoadCloudSave", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
                if (m != null) return (byte[])m.Invoke(null, new object[] { filename });
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Save.SteamBridge] LoadCloudSave reflection error: {ex.GetType().Name}: {ex.Message}
  context: filename={filename}
{ex.StackTrace}");
                // Fallback: return null (caller treats as not-in-cloud).
            }
            return null;
        }

        public static bool DeleteCloudFile(string filename)
        {
            var t = T(); if (t == null) return false;
            try
            {
                var m = t.GetMethod("DeleteCloudFile", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
                if (m != null) return (bool)m.Invoke(null, new object[] { filename });
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Save.SteamBridge] DeleteCloudFile reflection error: {ex.GetType().Name}: {ex.Message}
  context: filename={filename}
{ex.StackTrace}");
                // Fallback: return false (delete reported as failed; caller may retry).
            }
            return false;
        }

        public static bool IsCloudEnabledAndHasSpace(int bytes)
        {
            var t = T(); if (t == null) return false;
            try
            {
                var m = t.GetMethod("IsCloudEnabledAndHasSpace", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(int) }, null);
                if (m != null) return (bool)m.Invoke(null, new object[] { bytes });
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Save.SteamBridge] IsCloudEnabledAndHasSpace reflection error: {ex.GetType().Name}: {ex.Message}
  context: bytes={bytes}
{ex.StackTrace}");
                // Fallback: return false (caller will skip cloud upload).
            }
            return false;
        }
    }
}
