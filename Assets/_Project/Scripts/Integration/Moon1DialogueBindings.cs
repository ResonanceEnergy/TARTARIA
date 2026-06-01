using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon1DialogueBindings — bridges in-game events to DialogueManager context
    /// dialogue. Each of the 3 .yarn files under Assets/_Project/Dialogue/Moon1/
    /// has nodes whose names match these PlayContextDialogue("...") keys:
    ///
    ///   milo_intro.yarn:
    ///     - milo_intro              (triggered by GameEvents.OnBuildingDiscovered, first time)
    ///     - milo_warming_up         (triggered by GameEvents.OnBuildingRestoredTyped after 1st)
    ///     - milo_sincere            (triggered by GameEvents.OnBuildingRestoredTyped after 2nd)
    ///
    ///   lore_whispers.yarn:
    ///     - lore_listener_hall      (triggered by LoreStoneInteraction with id=lore_stone_listener)
    ///     - lore_first_note         (lore_stone_spire)
    ///     - lore_thread_memory      (lore_stone_fountain)
    ///     - lore_old_well           (lore_stone_well)
    ///     - lore_broken_gate        (lore_stone_gate)
    ///     - lore_root_chamber       (lore_stone_root)
    ///
    ///   anastasia_greeting.yarn:
    ///     - anastasia_greeting           (triggered by NPCConditionalSpawn reveal for Anastasia)
    ///     - anastasia_dome_restored      (triggered by OnBuildingRestoredTyped for echohaven_stardome)
    ///     - anastasia_fountain_restored  (triggered by OnBuildingRestoredTyped for echohaven_harmonicfountain)
    ///
    /// This MonoBehaviour is auto-attached by Moon1MasterBootstrap so dialogue
    /// triggers fire automatically without per-NPC wiring.
    /// </summary>
    public class Moon1DialogueBindings : MonoBehaviour
    {
        private int _restoredCount;
        private bool _miloIntroFired;

        void OnEnable()
        {
            GameEvents.OnBuildingDiscoveredTyped += OnBuildingDiscovered;
            GameEvents.OnBuildingRestoredTyped += OnBuildingRestored;
        }

        void OnDisable()
        {
            GameEvents.OnBuildingDiscoveredTyped -= OnBuildingDiscovered;
            GameEvents.OnBuildingRestoredTyped -= OnBuildingRestored;
        }

        void OnBuildingDiscovered(BuildingDiscoveredEventArgs args)
        {
            if (args == null) return;
            if (!_miloIntroFired)
            {
                _miloIntroFired = true;
                Play("milo_intro");
            }
        }

        void OnBuildingRestored(BuildingRestoredEventArgs args)
        {
            if (args == null) return;
            _restoredCount++;

            // Building-specific Anastasia reactions
            switch (args.buildingId)
            {
                case "echohaven_crystalspire":
                    // Anastasia's reveal happens via NPCConditionalSpawn; we fire her
                    // greeting on a slight delay so the reveal banner shows first.
                    Invoke(nameof(PlayAnastasiaGreeting), 2.0f);
                    break;
                case "echohaven_stardome":
                    Play("anastasia_dome_restored");
                    break;
                case "echohaven_harmonicfountain":
                    Play("anastasia_fountain_restored");
                    break;
            }

            // Milo trust beats based on count
            if (_restoredCount == 1) Play("milo_warming_up");
            else if (_restoredCount == 2) Play("milo_sincere");
        }

        void PlayAnastasiaGreeting() => Play("anastasia_greeting");

        /// <summary>
        /// Public entry for LoreStoneInteraction to call when a stone is consumed.
        /// Routed via static event so we don't need a hard reference.
        /// </summary>
        public static void PlayLoreContext(string contextKey)
        {
            DialogueManager.Instance?.PlayContextDialogue(contextKey);
            Debug.Log($"[Moon1DialogueBindings] LoreStone → {contextKey}");
        }

        static void Play(string contextKey)
        {
            if (DialogueManager.Instance == null)
            {
                Debug.LogWarning($"[Moon1DialogueBindings] DialogueManager.Instance null — context {contextKey} skipped");
                return;
            }
            DialogueManager.Instance.PlayContextDialogue(contextKey);
            Debug.Log($"[Moon1DialogueBindings] → {contextKey}");
        }
    }
}
