// SettingsPersistence.cs
// Sprint 6 Lane 2 — agent/ui/settings-menu-real
// Owner: UI agent. Path: Assets/_Project/Scripts/UI/SettingsPersistence.cs
//
// Versioned PlayerPrefs store for the real settings menu. All keys are
// prefixed TARTARIA_SET_<name> per the lane prompt (distinct from the legacy
// TARTARIA_* keys used by SettingsOverlay.cs, which kept short names without
// the SET_ infix). Schema bumps invalidate stale on-disk values.
//
// API_CONTRACT.md compliance:
// - No banned namespace name (UI is not in the banned list).
// - No FindObjectOfType / no deprecated Unity 6 calls.
// - No silent catches: every load logs the resolved value (no-debt rule 4).
// - No silent fallback: when a key is missing we log "default" with the value.

using UnityEngine;

namespace Tartaria.UI
{
    /// <summary>
    /// Versioned PlayerPrefs read/write for SettingsMenu.
    /// All keys carry the TARTARIA_SET_ prefix so they never collide with the
    /// older TARTARIA_* keys used by SettingsOverlay (legacy IMGUI panel) or by
    /// the Camera / Difficulty systems.
    /// </summary>
    public static class SettingsPersistence
    {
        // Schema version. Bump this when a key changes meaning so old saves
        // are discarded rather than silently misinterpreted.
        public const int SchemaVersion = 1;
        const string K_SCHEMA = "TARTARIA_SET_SchemaVersion";

        // === Keys (versioned) =========================================
        public const string K_ResolutionIdx   = "TARTARIA_SET_ResolutionIdx";
        public const string K_Fullscreen      = "TARTARIA_SET_Fullscreen";
        public const string K_MasterVolume    = "TARTARIA_SET_MasterVolume";
        public const string K_MusicVolume     = "TARTARIA_SET_MusicVolume";
        public const string K_SFXVolume       = "TARTARIA_SET_SFXVolume";
        public const string K_InvertY         = "TARTARIA_SET_InvertY";
        public const string K_Rumble          = "TARTARIA_SET_Rumble";
        public const string K_LanguageCode    = "TARTARIA_SET_LanguageCode";

        // === Defaults =================================================
        public const float DefaultMasterVolume = 0.8f;
        public const float DefaultMusicVolume  = 0.7f;
        public const float DefaultSFXVolume    = 0.85f;
        public const bool  DefaultFullscreen   = true;
        public const bool  DefaultInvertY      = false;
        public const bool  DefaultRumble       = true;
        public const string DefaultLanguageCode = "en";

        // === Lifecycle ================================================

        /// <summary>
        /// Ensure schema is recorded; discard incompatible older schemas.
        /// Logs the migration explicitly (no silent fallback).
        /// </summary>
        public static void EnsureSchema()
        {
            int stored = PlayerPrefs.GetInt(K_SCHEMA, 0);
            if (stored == SchemaVersion)
            {
                Debug.Log($"[SettingsMenu] Loaded {K_SCHEMA}={stored}");
                return;
            }
            if (stored == 0)
            {
                PlayerPrefs.SetInt(K_SCHEMA, SchemaVersion);
                PlayerPrefs.Save();
                Debug.Log($"[SettingsMenu] Loaded {K_SCHEMA}=default ({SchemaVersion}) — initialized");
                return;
            }
            Debug.LogWarning(
                $"[SettingsMenu] Stored schema {stored} != current {SchemaVersion}. " +
                "Clearing TARTARIA_SET_* keys and re-initializing to defaults.");
            ClearAllSettingsKeys();
            PlayerPrefs.SetInt(K_SCHEMA, SchemaVersion);
            PlayerPrefs.Save();
        }

        static void ClearAllSettingsKeys()
        {
            string[] keys =
            {
                K_ResolutionIdx, K_Fullscreen, K_MasterVolume, K_MusicVolume,
                K_SFXVolume, K_InvertY, K_Rumble, K_LanguageCode
            };
            for (int i = 0; i < keys.Length; i++)
            {
                if (PlayerPrefs.HasKey(keys[i])) PlayerPrefs.DeleteKey(keys[i]);
            }
        }

        // === Typed readers (each logs the resolved value) ==============

        public static int LoadInt(string key, int fallback)
        {
            if (PlayerPrefs.HasKey(key))
            {
                int v = PlayerPrefs.GetInt(key);
                Debug.Log($"[SettingsMenu] Loaded {key}={v}");
                return v;
            }
            Debug.Log($"[SettingsMenu] Loaded {key}=default ({fallback})");
            return fallback;
        }

        public static float LoadFloat(string key, float fallback)
        {
            if (PlayerPrefs.HasKey(key))
            {
                float v = PlayerPrefs.GetFloat(key);
                Debug.Log($"[SettingsMenu] Loaded {key}={v:F3}");
                return v;
            }
            Debug.Log($"[SettingsMenu] Loaded {key}=default ({fallback:F3})");
            return fallback;
        }

        public static bool LoadBool(string key, bool fallback)
        {
            if (PlayerPrefs.HasKey(key))
            {
                bool v = PlayerPrefs.GetInt(key) == 1;
                Debug.Log($"[SettingsMenu] Loaded {key}={v}");
                return v;
            }
            Debug.Log($"[SettingsMenu] Loaded {key}=default ({fallback})");
            return fallback;
        }

        public static string LoadString(string key, string fallback)
        {
            if (PlayerPrefs.HasKey(key))
            {
                string v = PlayerPrefs.GetString(key);
                Debug.Log($"[SettingsMenu] Loaded {key}={v}");
                return v;
            }
            Debug.Log($"[SettingsMenu] Loaded {key}=default ({fallback})");
            return fallback;
        }

        // === Typed writers (no PlayerPrefs.Save until Commit) ==========

        public static void StoreInt(string key, int value)    => PlayerPrefs.SetInt(key, value);
        public static void StoreFloat(string key, float value) => PlayerPrefs.SetFloat(key, value);
        public static void StoreBool(string key, bool value)   => PlayerPrefs.SetInt(key, value ? 1 : 0);
        public static void StoreString(string key, string value) => PlayerPrefs.SetString(key, value);

        /// <summary>Flush staged writes to disk.</summary>
        public static void Commit()
        {
            PlayerPrefs.Save();
            Debug.Log("[SettingsMenu] PlayerPrefs committed.");
        }
    }
}
