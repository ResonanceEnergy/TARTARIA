using UnityEngine;
using System;
using System.Collections.Generic;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Campaign Flow Controller -- manages the 13-Moon campaign progression.
    ///
    /// Each Moon (chapter) has:
    ///   - A primary zone with unique architecture and enemies
    ///   - Gate conditions (quests + RS threshold) to unlock the next Moon
    ///   - An introduction sequence with narrative context
    ///   - Companion unlocks and key story beats
    ///
    /// Progression:
    ///   Moon 1: Echohaven -- tutorial zone, meet Milo
    ///   Moon 2: Solara    -- corruption mechanics, meet Cassian/Lirael
    ///   Moon 3: Resonara  -- Bell Tower tuning, first boss
    ///   ...through Moon 13: The Hidden Moon
    /// </summary>
    [DisallowMultipleComponent]
    public class CampaignFlowController : MonoBehaviour, ICampaignService
    {
        public static CampaignFlowController Instance { get; private set; }

        [Header("Moon Definitions")]
        [SerializeField] MoonDefinition[] moons;

        int _currentMoon;
        readonly Dictionary<int, MoonProgress> _moonProgress = new();

        public int CurrentMoon => _currentMoon;
        public int CurrentMoonIndex => _currentMoon;
        public MoonDefinition CurrentMoonDef =>
            _currentMoon >= 0 && _currentMoon < (moons?.Length ?? 0)
                ? moons[_currentMoon] : null;

        public event Action<int> OnMoonStarted;
        public event Action<int> OnMoonCompleted;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Campaign = this;
        }

        void Start()
        {
            if (moons == null || moons.Length == 0)
            {
                moons = BuildDefaultMoons();
            }

            // Initialize progress for all moons
            for (int i = 0; i < moons.Length; i++)
            {
                if (!_moonProgress.ContainsKey(i))
                    _moonProgress[i] = new MoonProgress();
            }

            // Subscribe to quest completions
            if (QuestManager.Instance != null)
                QuestManager.Instance.OnQuestStatusChanged += OnQuestStatusChanged;

            if (_currentMoon == 0)
                BeginMoon(0);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (QuestManager.Instance != null)
                QuestManager.Instance.OnQuestStatusChanged -= OnQuestStatusChanged;
        }

        // ─── Public API ──────────────────────────────

        /// <summary>
        /// Check if the player can advance to the next Moon.
        /// </summary>
        public bool CanAdvance()
        {
            if (moons == null || _currentMoon >= moons.Length - 1) return false;
            var moon = moons[_currentMoon];

            // Check RS threshold
            float rs = AetherFieldManager.Instance?.ResonanceScore ?? 0f;
            if (rs < moon.rsThresholdToAdvance) return false;

            // Check required quests are complete
            if (moon.requiredQuestIds != null)
            {
                foreach (string qid in moon.requiredQuestIds)
                {
                    if (QuestManager.Instance == null) return false;
                    if (!QuestManager.Instance.IsQuestComplete(qid)) return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Advance to the next Moon. Call after CanAdvance() returns true.
        /// </summary>
        public void AdvanceToNextMoon()
        {
            if (!CanAdvance()) return;

            CompleteMoon(_currentMoon);
            BeginMoon(_currentMoon + 1);
        }

        /// <summary>
        /// Get progress summary for the current Moon.
        /// </summary>
        public string GetProgressSummary()
        {
            if (moons == null || _currentMoon >= moons.Length) return "No campaign data.";
            var moon = moons[_currentMoon];
            float rs = AetherFieldManager.Instance?.ResonanceScore ?? 0f;

            return $"Moon {_currentMoon + 1}: {moon.moonName}\n" +
                   $"RS: {rs:F0} / {moon.rsThresholdToAdvance}\n" +
                   $"Quests: {GetCompletedQuestCount(moon)} / {moon.requiredQuestIds?.Length ?? 0}";
        }

        // ─── Save / Restore ─────────────────────────

        public CampaignSaveData GetSaveData()
        {
            var data = new CampaignSaveData
            {
                currentMoon = _currentMoon,
                completedMoons = new List<int>()
            };
            foreach (var kvp in _moonProgress)
                if (kvp.Value.completed)
                    data.completedMoons.Add(kvp.Key);
            return data;
        }

        public void RestoreFromSave(CampaignSaveData data)
        {
            if (data == null) return;
            _currentMoon = data.currentMoon;
            foreach (var kvp in _moonProgress)
                kvp.Value.completed = false;
            if (data.completedMoons != null)
                foreach (int m in data.completedMoons)
                    if (_moonProgress.ContainsKey(m))
                        _moonProgress[m].completed = true;

            // Reapply Moon modifiers and zone state for the current moon
            if (_currentMoon >= 0 && _currentMoon < moons.Length)
            {
                var moon = moons[_currentMoon];
                MoonModifierProvider.Apply(moon.modifiers);
                Debug.Log($"[Campaign] Restored to Moon {_currentMoon + 1}: {moon.moonName}");
            }
        }

        // ─── Internal ────────────────────────────────

        void BeginMoon(int index)
        {
            if (index < 0 || index >= moons.Length) return;
            _currentMoon = index;
            var moon = moons[index];

            Debug.Log($"[Campaign] Moon {index + 1} begins: {moon.moonName}");

            // Apply Moon-specific modifiers
            MoonModifierProvider.Apply(moon.modifiers);

            // Activate moon quests
            if (moon.questIdsToActivate != null)
                foreach (string qid in moon.questIdsToActivate)
                    QuestManager.Instance?.ActivateQuest(qid);

            // Transition to moon zone
            if (ZoneTransitionSystem.Instance != null && moon.zoneIndex >= 0)
                ZoneTransitionSystem.Instance.TransitionToZone(moon.zoneIndex);

            // Show intro dialogue
            DialogueManager.Instance?.PlayContextDialogue($"moon_{index + 1}_intro");
            UI.HUDController.Instance?.ShowInteractionPrompt($"Moon {index + 1}: {moon.moonName}");
            Audio.AdaptiveMusicController.Instance?.SetZone(index);
            Input.HapticFeedbackManager.Instance?.PlayBuildingEmergence();

            OnMoonStarted?.Invoke(index);
        }

        void CompleteMoon(int index)
        {
            if (_moonProgress.ContainsKey(index))
                _moonProgress[index].completed = true;

            // Remove Moon modifiers
            MoonModifierProvider.Reset();

            Debug.Log($"[Campaign] Moon {index + 1} completed: {moons[index].moonName}");
            DialogueManager.Instance?.PlayContextDialogue($"moon_{index + 1}_complete");
            OnMoonCompleted?.Invoke(index);
            AchievementSystem.Instance?.CheckMoonCompleted(index);
            Save.SaveManager.Instance?.MarkDirty();
            Audio.AdaptiveMusicController.Instance?.PlayZoneShift();
        }

        void OnQuestStatusChanged(string questId, QuestStatus status)
        {
            if (status != QuestStatus.Completed) return;

            // Check if this completion enables Moon advancement
            if (CanAdvance())
            {
                UI.HUDController.Instance?.ShowInteractionPrompt(
                    "The way forward is open. Seek the Ley Line portal.");
            }
        }

        int GetCompletedQuestCount(MoonDefinition moon)
        {
            if (moon.requiredQuestIds == null) return 0;
            int count = 0;
            foreach (string qid in moon.requiredQuestIds)
                if (QuestManager.Instance != null && QuestManager.Instance.IsQuestComplete(qid))
                    count++;
            return count;
        }

        // ─── Default Moon Data ───────────────────────

        MoonDefinition[] BuildDefaultMoons()
        {
            return new[]
            {
                new MoonDefinition {
                    moonName = "Echohaven", zoneIndex = 0,
                    rsThresholdToAdvance = 100f,
                    requiredQuestIds = new[] { "echohaven_main" },
                    questIdsToActivate = new[] { "echohaven_main", "milo_companion" },
                    modifiers = new MoonModifiers { rsGainMultiplier = 1.2f, tuningDifficultyMultiplier = 0.8f }
                },
                new MoonDefinition {
                    moonName = "Shadow & Purge", zoneIndex = 1,
                    rsThresholdToAdvance = 250f,
                    requiredQuestIds = new[] { "shadow_purge_main", "corruption_bloom" },
                    questIdsToActivate = new[] { "shadow_purge_main", "corruption_bloom", "cassian_companion" },
                    modifiers = new MoonModifiers { enableCorruption = true, enableMicroMode = true, corruptionSpreadRate = 0.8f, enemyHealthMultiplier = 1.1f, specialMechanic = "dissonance_lens" }
                },
                new MoonDefinition {
                    moonName = "Windswept Highlands", zoneIndex = 2,
                    rsThresholdToAdvance = 450f,
                    requiredQuestIds = new[] { "highlands_main", "orphan_train_escort" },
                    questIdsToActivate = new[] { "highlands_main", "orphan_train_escort", "rail_network" },
                    modifiers = new MoonModifiers { enableCorruption = true, enableMicroMode = true, tuningDifficultyMultiplier = 1.1f, enemyHealthMultiplier = 1.2f, specialMechanic = "rail_network" }
                },
                new MoonDefinition {
                    moonName = "Star Fort Bastion", zoneIndex = 3,
                    rsThresholdToAdvance = 700f,
                    requiredQuestIds = new[] { "star_fort_main", "korath_echo" },
                    questIdsToActivate = new[] { "star_fort_main", "korath_echo", "precision_cutting" },
                    modifiers = new MoonModifiers { enableCorruption = true, enableLeyLines = true, enableMicroMode = true, enemyHealthMultiplier = 1.3f, enemyDamageMultiplier = 1.2f, specialMechanic = "golden_ratio" }
                },
                new MoonDefinition {
                    moonName = "White City", zoneIndex = 4,
                    rsThresholdToAdvance = 1000f,
                    requiredQuestIds = new[] { "white_city_main" },
                    questIdsToActivate = new[] { "white_city_main", "overtone_networks" },
                    modifiers = new MoonModifiers { enableCorruption = true, enableLeyLines = true, enableGiantMode = true, enableMicroMode = true, rsGainMultiplier = 1.3f, corruptionSpreadRate = 1.2f, enemyHealthMultiplier = 1.4f, specialMechanic = "overtone_resonance" }
                },
                new MoonDefinition {
                    moonName = "Sunken Cathedral", zoneIndex = 5,
                    rsThresholdToAdvance = 1400f,
                    requiredQuestIds = new[] { "cathedral_main", "cymatic_requiem" },
                    questIdsToActivate = new[] { "cathedral_main", "cymatic_requiem", "veritas_companion" },
                    modifiers = new MoonModifiers { enableCorruption = true, enableLeyLines = true, enableGiantMode = true, enableMicroMode = true, tuningDifficultyMultiplier = 1.3f, enemyHealthMultiplier = 1.5f, enemyDamageMultiplier = 1.3f, specialMechanic = "pipe_organ" }
                },
                new MoonDefinition {
                    moonName = "Giant's Awakening", zoneIndex = 6,
                    rsThresholdToAdvance = 1900f,
                    requiredQuestIds = new[] { "giants_awakening_main", "korath_thaw" },
                    questIdsToActivate = new[] { "giants_awakening_main", "korath_thaw", "cassian_confrontation" },
                    modifiers = new MoonModifiers { enableCorruption = true, enableLeyLines = true, enableGiantMode = true, specialMechanic = "giant_stasis_thaw", enemyHealthMultiplier = 1.6f, enemyDamageMultiplier = 1.4f, corruptionSpreadRate = 1.5f }
                },
                new MoonDefinition {
                    moonName = "The Airship Armada", zoneIndex = 7,
                    rsThresholdToAdvance = 2500f,
                    requiredQuestIds = new[] { "airship_armada_main", "fleet_restoration" },
                    questIdsToActivate = new[] { "airship_armada_main", "fleet_restoration", "thorne_companion" },
                    modifiers = new MoonModifiers { enableCorruption = true, enableLeyLines = true, enableGiantMode = true, enableMicroMode = true, specialMechanic = "mercury_orb_tuning", enemyHealthMultiplier = 1.7f, enemyDamageMultiplier = 1.5f, aetherDrainMultiplier = 1.5f }
                },
                new MoonDefinition {
                    moonName = "The Ley Line Prophecy", zoneIndex = 8,
                    rsThresholdToAdvance = 3200f,
                    requiredQuestIds = new[] { "ley_line_prophecy_main", "prophecy_stones" },
                    questIdsToActivate = new[] { "ley_line_prophecy_main", "prophecy_stones", "zereth_whispers" },
                    modifiers = new MoonModifiers { enableCorruption = true, enableLeyLines = true, enableGiantMode = true, enableMicroMode = true, specialMechanic = "prophecy_stones", enemyHealthMultiplier = 1.8f, enemyDamageMultiplier = 1.6f, rsGainMultiplier = 0.9f }
                },
                new MoonDefinition {
                    moonName = "The Living Grid", zoneIndex = 9,
                    rsThresholdToAdvance = 4000f,
                    requiredQuestIds = new[] { "living_grid_main", "continental_rail" },
                    questIdsToActivate = new[] { "living_grid_main", "continental_rail", "trigger_room" },
                    modifiers = new MoonModifiers { enableCorruption = true, enableLeyLines = true, enableGiantMode = true, enableMicroMode = true, specialMechanic = "continental_train", enemyHealthMultiplier = 2.0f, enemyDamageMultiplier = 1.7f, tuningDifficultyMultiplier = 1.4f }
                },
                new MoonDefinition {
                    moonName = "Veil Between Worlds", zoneIndex = 10,
                    rsThresholdToAdvance = 5000f,
                    requiredQuestIds = new[] { "veil_main", "aquifer_purification" },
                    questIdsToActivate = new[] { "veil_main", "aquifer_purification", "fountain_chain" },
                    modifiers = new MoonModifiers { enableCorruption = true, enableLeyLines = true, enableGiantMode = true, enableMicroMode = true, specialMechanic = "spectral_solidification", enemyHealthMultiplier = 2.2f, enemyDamageMultiplier = 1.8f, corruptionSpreadRate = 2.0f }
                },
                new MoonDefinition {
                    moonName = "Planetary Bell-Tower Ring", zoneIndex = 11,
                    rsThresholdToAdvance = 6500f,
                    requiredQuestIds = new[] { "bell_tower_ring_main", "tower_synchronization" },
                    questIdsToActivate = new[] { "bell_tower_ring_main", "tower_synchronization", "final_prophecy_stone" },
                    modifiers = new MoonModifiers { enableCorruption = true, enableLeyLines = true, enableGiantMode = true, enableMicroMode = true, specialMechanic = "bell_tower_sync", enemyHealthMultiplier = 2.5f, enemyDamageMultiplier = 2.0f, tuningDifficultyMultiplier = 1.5f, aetherDrainMultiplier = 2.0f }
                },
                new MoonDefinition {
                    moonName = "True Timeline Convergence", zoneIndex = 12,
                    rsThresholdToAdvance = float.MaxValue, // Final moon
                    requiredQuestIds = new[] { "convergence_final", "anastasia_finale" },
                    questIdsToActivate = new[] { "convergence_final", "anastasia_finale", "echo_realm_travel" },
                    modifiers = new MoonModifiers { enableCorruption = true, enableLeyLines = true, enableGiantMode = true, enableMicroMode = true, specialMechanic = "true_history", enemyHealthMultiplier = 3.0f, enemyDamageMultiplier = 2.5f, rsGainMultiplier = 1.5f, corruptionSpreadRate = 2.5f, tuningDifficultyMultiplier = 1.8f, aetherDrainMultiplier = 2.5f }
                }
            };
        }

        // ─── Ending Path ─────────────────────────────

        public enum EndingPath : byte { None = 0, Restoration = 1, Transcendence = 2 }

        EndingPath _ending = EndingPath.None;
        public EndingPath CurrentEnding => _ending;
        public event Action<EndingPath> OnEndingChosen;

        public void SetEnding(EndingPath path)
        {
            _ending = path;
            OnEndingChosen?.Invoke(path);
            Debug.Log($"[Campaign] Ending path set: {path}");
        }
    }

    // ─── Campaign Data Types ─────────────────────

    [Serializable]
    public class MoonDefinition
    {
        public string moonName;
        public int zoneIndex;
        public float rsThresholdToAdvance;
        public string[] requiredQuestIds;
        public string[] questIdsToActivate;
        public MoonModifiers modifiers;
    }

    /// <summary>
    /// Per-Moon gameplay modifiers that customize difficulty, pacing, and
    /// special mechanics for each campaign chapter.
    ///
    /// Applied on BeginMoon(), removed on CompleteMoon().
    /// </summary>
    [Serializable]
    public class MoonModifiers
    {
        public float enemyHealthMultiplier = 1f;
        public float enemyDamageMultiplier = 1f;
        public float rsGainMultiplier = 1f;
        public float corruptionSpreadRate = 1f;
        public float tuningDifficultyMultiplier = 1f;
        public float aetherDrainMultiplier = 1f;
        public bool enableCorruption;
        public bool enableLeyLines;
        public bool enableGiantMode;
        public bool enableMicroMode;
        public string specialMechanic;  // e.g. "bell_tower", "mirror_maze", "crystal_caves"

        public static MoonModifiers Default => new();
    }

    /// <summary>
    /// Provides active Moon modifiers to all gameplay systems.
    /// Queried by CombatWaveManager, CorruptionSystem, TuningMiniGame, etc.
    /// </summary>
    public static class MoonModifierProvider
    {
        static MoonModifiers _active = MoonModifiers.Default;

        public static MoonModifiers Active => _active;

        public static void Apply(MoonModifiers mods)
        {
            _active = mods ?? MoonModifiers.Default;
            Debug.Log($"[MoonMods] Applied: HP ×{_active.enemyHealthMultiplier:F1}, " +
                      $"DMG ×{_active.enemyDamageMultiplier:F1}, RS ×{_active.rsGainMultiplier:F1}, " +
                      $"Corruption={_active.enableCorruption}, LeyLines={_active.enableLeyLines}");
        }

        public static void Reset() => _active = MoonModifiers.Default;
    }

    class MoonProgress
    {
        public bool completed;
    }

    [Serializable]
    public class CampaignSaveData
    {
        public int currentMoon;
        public List<int> completedMoons;
    }
}
