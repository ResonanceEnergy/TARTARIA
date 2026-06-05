using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tartaria.Core
{
    /// <summary>
    /// Lightweight localization system. Loads string tables from
    /// Resources/Localization/{lang}.json, falls back to English.
    /// Usage: LocalizationManager.Get("hud_wave_counter", wave, total)
    /// </summary>
    public static class LocalizationManager
    {
        public enum Language { English, Spanish, French, German, Portuguese, Russian, Chinese, Japanese, Korean, Turkish, Arabic }

        static Language _current = Language.English;
        static readonly Dictionary<string, string> _table = new();
        static bool _loaded;

        /// <summary>Currently active language.</summary>
        public static Language CurrentLanguage => _current;

        /// <summary>Fires when language changes. Subscribers should refresh their UI text.</summary>
        public static event Action<Language> OnLanguageChanged;

        /// <summary>Set active language and reload string table.</summary>
        public static void SetLanguage(Language lang)
        {
            _current = lang;
            LoadTable(lang);
            OnLanguageChanged?.Invoke(lang);
        }

        /// <summary>
        /// Look up a localized string by key. Supports string.Format args.
        /// Returns the key itself if not found (visible debug marker).
        /// </summary>
        public static string Get(string key, params object[] args)
        {
            if (!_loaded) LoadTable(_current);

            if (_table.TryGetValue(key, out var pattern))
            {
                return args.Length > 0 ? string.Format(pattern, args) : pattern;
            }

            // Fallback: return key bracketed so missing strings are visible in-game
            return $"[{key}]";
        }

        /// <summary>
        /// Register a string at runtime (used by BuildDatabase, factories, etc.).
        /// Does not overwrite existing entries unless force=true.
        /// </summary>
        public static void Register(string key, string value, bool force = false)
        {
            if (force || !_table.ContainsKey(key))
                _table[key] = value;
        }

        /// <summary>Bulk register from a dictionary (used by JSON import).</summary>
        public static void RegisterAll(Dictionary<string, string> entries, bool force = false)
        {
            foreach (var kv in entries)
                Register(kv.Key, kv.Value, force);
        }

        /// <summary>Returns true if the key exists in the current table.</summary>
        public static bool Has(string key) => _table.ContainsKey(key);

        /// <summary>Total number of loaded strings.</summary>
        public static int Count => _table.Count;

        // ─── Internal ────────────────────────────────

        static void LoadTable(Language lang)
        {
            _table.Clear();
            string langCode = GetLangCode(lang);
            var asset = Resources.Load<TextAsset>($"Localization/{langCode}");
            if (asset != null)
            {
                var wrapper = JsonUtility.FromJson<StringTableWrapper>(asset.text);
                if (wrapper?.entries != null)
                {
                    foreach (var e in wrapper.entries)
                        _table[e.key] = e.value;
                }
            }

            // Always load English as fallback layer (don't overwrite existing)
            if (lang != Language.English)
            {
                var fallback = Resources.Load<TextAsset>("Localization/en");
                if (fallback != null)
                {
                    var wrapper = JsonUtility.FromJson<StringTableWrapper>(fallback.text);
                    if (wrapper?.entries != null)
                    {
                        foreach (var e in wrapper.entries)
                        {
                            if (!_table.ContainsKey(e.key))
                                _table[e.key] = e.value;
                        }
                    }
                }
            }

            // Register built-in defaults for core HUD strings
            RegisterDefaults();
            _loaded = true;
            Debug.Log($"[Tartaria] Localization loaded: {langCode} ({_table.Count} strings)");
        }

        static void RegisterDefaults()
        {
            // HUD
            Register("hud_wave", "Wave {0}/{1}");
            Register("hud_enemies_remaining", "{0} remaining");
            Register("hud_zone_name", "{0}");
            Register("hud_rs_value", "RS: {0:F0}");
            Register("hud_aether_value", "Aether: {0:F0}%");
            Register("hud_interact", "Press E to interact");
            Register("hud_achievement", "Achievement Unlocked!");
            Register("hud_moon_complete", "MOON COMPLETE");

            // Menus
            Register("menu_resume", "Resume");
            Register("menu_settings", "Settings");
            Register("menu_quit", "Quit to Menu");
            Register("menu_save", "Save Game");
            Register("menu_load", "Load Game");

            // Workshop
            Register("workshop_title", "WORKSHOP");
            Register("workshop_upgrade", "UPGRADE");
            Register("workshop_tier", "Tier {0}");
            Register("workshop_max_tier", "MAX TIER");
            Register("workshop_rs_cost", "RS Required: {0}");

            // Skill Tree
            Register("skill_tree_title", "SKILL TREE");
            Register("skill_unlock", "UNLOCK");
            Register("skill_unlocked", "UNLOCKED");
            Register("skill_locked", "LOCKED");
            Register("skill_cost", "Cost: {0} RS");

            // Quest Log
            Register("quest_log_title", "QUEST LOG");
            Register("quest_tab_active", "Active");
            Register("quest_tab_completed", "Completed");
            Register("quest_tab_all", "All");
            Register("quest_reward", "Reward: {0}");

            // World Map
            Register("map_title", "WORLD MAP");
            Register("map_travel", "TRAVEL");
            Register("map_locked", "Locked");
            Register("map_available", "Available");
            Register("map_active", "Active");
            Register("map_completed", "Completed");

            // Combat
            Register("combat_victory", "Victory!");
            Register("combat_defeated", "Defeated...");
            Register("combat_boss_appear", "A corruption entity approaches!");

            // Tuning
            Register("tuning_success", "Resonance Achieved!");
            Register("tuning_fail", "Dissonance... try again.");

            // Zones
            Register("zone_echohaven", "Echohaven");
            Register("zone_solara", "Solara");
            Register("zone_crystalmere", "Crystalmere");
            Register("zone_voidharbor", "Void Harbor");
            Register("zone_locked_rs", "Zone locked. Requires RS {0}");
            Register("zone_quest_required", "Complete all quests in this zone to continue");
            Register("zone_transition_complete", "{0} Unlocked!");

            // Companions
            Register("companion_milo", "Milo");
            Register("companion_lirael", "Lirael");
            Register("companion_cassian", "Cassian");
            Register("companion_anastasia", "Anastasia");
            Register("companion_trust_label", "{0}: Trust {1:F0}/100");
            Register("companion_trust_increase", "{0} trusts you more");
            Register("companion_joined", "{0} has joined your journey!");

            // Mini-games
            Register("minigame_tuning_title", "TUNING");
            Register("minigame_organ_title", "PIPE ORGAN");
            Register("minigame_rockcut_title", "ROCK CUTTING");
            Register("minigame_choir_title", "CHOIR HARMONICS");
            Register("minigame_aquifer_title", "AQUIFER PURGE");
            Register("minigame_time_remaining", "{0:F0}s");
            Register("minigame_accuracy", "Accuracy: {0:P0}");
            Register("minigame_complete", "Complete!");
            Register("minigame_failed", "Failed!");

            // Economy
            Register("currency_aether_shards", "Aether Shards");
            Register("currency_resonance_crystals", "Resonance Crystals");
            Register("currency_star_fragments", "Star Fragments");
            Register("currency_gained", "+{0} {1}");
            Register("loot_drop", "+{0}");

            // Boss / Combat
            Register("boss_spawn", "{0} awakens!");
            Register("boss_defeated", "{0} vanquished! RS +{1:F0}");
            Register("boss_phase", "Phase {0}!");
            Register("wave_incoming", "Incoming wave {0}/{1}");

            // Quest objectives
            Register("quest_defeat_enemies", "Defeat {0} enemies");
            Register("quest_restore_buildings", "Restore {0} buildings");
            Register("quest_reach_rs", "Reach RS {0}");
            Register("quest_find_companion", "Find {0}");
            Register("quest_complete_minigame", "Complete the {0}");

            // General
            Register("loading_tip_default", "The golden ratio appears in all Tartarian architecture...");
            Register("save_indicator", "Saving...");
        }

        static string GetLangCode(Language lang) => lang switch
        {
            Language.English    => "en",
            Language.Spanish    => "es",
            Language.French     => "fr",
            Language.German     => "de",
            Language.Portuguese => "pt",
            Language.Russian    => "ru",
            Language.Chinese    => "zh",
            Language.Japanese   => "ja",
            Language.Korean     => "ko",
            Language.Turkish    => "tr",
            Language.Arabic     => "ar",
            _                   => "en"
        };

        [Serializable]
        class StringTableWrapper
        {
            public StringEntry[] entries;
        }

        [Serializable]
        class StringEntry
        {
            public string key;
            public string value;
        }
    }
}
