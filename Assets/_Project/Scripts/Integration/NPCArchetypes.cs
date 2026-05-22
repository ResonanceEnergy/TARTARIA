using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.UI;

namespace Tartaria.Integration
{
    /// <summary>
    /// Generic Merchant NPC — sells resonance items, upgrades, cosmetics.
    /// Ambient banter, quest items, personality quirks.
    /// </summary>
    [DisallowMultipleComponent]
    public class MerchantNPC : MonoBehaviour, IInteractable
    {
        [Header("Merchant Identity")]
        [SerializeField] string merchantName = "Helena the Trader";
        [SerializeField, TextArea(2, 4)] string greetingLine = "Back again? Good. The Aether flows where coin does.";
        [SerializeField] MerchantType merchantType = MerchantType.General;

        [Header("Inventory")]
        [SerializeField] string[] itemsForSale = { "ResonanceCrystal", "TuningFork", "RepairKit" };
        [SerializeField] bool sellsQuestItems = false;

        enum MerchantType { General, Blacksmith, Alchemist, Librarian }

        float _lastInteractionTime;

        public string GetInteractPrompt() => $"[E] Trade with {merchantName}";

        public void Interact(GameObject player)
        {
            // Greeting dialogue (varies by time since last interaction)
            float timeSinceLastVisit = Time.time - _lastInteractionTime;
            string greeting = timeSinceLastVisit > 300f 
                ? "Well, well! Haven't seen you in a while. Found anything... resonant?" 
                : greetingLine;

            DialogueManager.Instance?.PlayLineById($"merchant_{merchantType.ToString().ToLower()}_greeting");
            
            // Open shop UI
            UI.ShopUI.Instance?.OpenShop(merchantName, itemsForSale);

            _lastInteractionTime = Time.time;
            Debug.Log($"[MerchantNPC] {merchantName} shop opened.");
        }

        public void OnPlayerNearby(float distance)
        {
            // Ambient banter when player is close but hasn't interacted
            if (distance < 8f && Time.time - _lastInteractionTime > 60f)
            {
                if (Random.value < 0.1f) // 10% chance per check
                {
                    DialogueManager.Instance?.PlayContextDialogue($"merchant_{merchantType.ToString().ToLower()}_ambient");
                    _lastInteractionTime = Time.time - 30f; // Delay next banter
                }
            }
        }
    }

    /// <summary>
    /// Echo Citizen NPC — restored Tartarian citizens (non-interactive background population).
    /// Provides ambient life, contextual reactions to player actions (restoration, combat).
    /// </summary>
    [DisallowMultipleComponent]
    public class EchoCitizenNPC : MonoBehaviour
    {
        [Header("Citizen Identity")]
        [SerializeField] string citizenName = "Echo of Marcus";
        [SerializeField] CitizenRole role = CitizenRole.Builder;
        [SerializeField, Range(0f, 1f)] float echoOpacity = 0.7f;

        [Header("Behavior")]
        [SerializeField] bool reactsToRestoration = true;
        [SerializeField] bool celebratesOnZoneComplete = true;

        enum CitizenRole { Builder, Musician, Scholar, Child, Elder }

        MeshRenderer _renderer;
        bool _hasCelebrated;

        void Start()
        {
            _renderer = GetComponentInChildren<MeshRenderer>();
            if (_renderer != null)
            {
                var mat = _renderer.material;
                var color = mat.color;
                color.a = echoOpacity;
                mat.color = color;
            }

            // Subscribe to zone events
            if (reactsToRestoration)
                RestorableBuilding.OnAnyBuildingRestored += OnBuildingRestored;
            
            if (celebratesOnZoneComplete)
                MoonContentManager.OnZoneComplete += OnZoneComplete;
        }

        void OnDestroy()
        {
            RestorableBuilding.OnAnyBuildingRestored -= OnBuildingRestored;
            MoonContentManager.OnZoneComplete -= OnZoneComplete;
        }

        void OnBuildingRestored(string buildingId)
        {
            // React with joy — play animation, emit particles, ambient cheer
            if (Random.value < 0.3f) // 30% of citizens react
            {
                // Play celebration animation (if available)
                AudioManager.Instance?.PlaySFX3D("EchoCelebration", transform.position);
                
                // Gradually increase opacity (echo becomes more solid)
                if (_renderer != null)
                {
                    echoOpacity = Mathf.Min(1f, echoOpacity + 0.1f);
                    var mat = _renderer.material;
                    var color = mat.color;
                    color.a = echoOpacity;
                    mat.color = color;
                }

                Debug.Log($"[EchoCitizenNPC] {citizenName} celebrates building restoration.");
            }
        }

        void OnZoneComplete()
        {
            if (_hasCelebrated) return;
            _hasCelebrated = true;

            // Full celebration — dance, sing, gather in groups
            AudioManager.Instance?.PlaySFX3D("EchoJubilation", transform.position);
            
            // Become fully solid
            if (_renderer != null)
            {
                echoOpacity = 1f;
                var mat = _renderer.material;
                var color = mat.color;
                color.a = 1f;
                mat.color = color;
            }

            Debug.Log($"[EchoCitizenNPC] {citizenName} celebrates zone completion — fully solidified!");
        }
    }

    /// <summary>
    /// Quest Giver NPC — specialized interactive NPCs with unique dialogue trees.
    /// Extends QuestGiverInteractable with personality, backstory, emotional arcs.
    /// </summary>
    [DisallowMultipleComponent]
    public class NamedQuestGiverNPC : MonoBehaviour, IInteractable
    {
        [Header("NPC Identity")]
        [SerializeField] string npcName = "Elder Thaddeus";
        [SerializeField, TextArea(3, 6)] string backstory = "A survivor of the Mud Flood, Thaddeus has spent decades searching for his family's ancestral home.";
        [SerializeField] string personalityArchetype = "Wise Elder";

        [Header("Quest Chain")]
        [SerializeField] string[] questSequence = { "quest_find_home", "quest_restore_home", "quest_family_reunion" };
        [SerializeField, TextArea(2, 4)] string[] questIntros;
        [SerializeField, TextArea(2, 4)] string[] questCompletions;

        int _currentQuestIndex;
        QuestGiverInteractable _questGiver;

        void Awake()
        {
            _questGiver = GetComponent<QuestGiverInteractable>();
            if (_questGiver == null)
                _questGiver = gameObject.AddComponent<QuestGiverInteractable>();
        }

        public string GetInteractPrompt()
        {
            if (_currentQuestIndex >= questSequence.Length)
                return $"[E] Thank {npcName}"; // Quest chain complete

            return _questGiver.GetInteractPrompt() ?? $"[E] Talk to {npcName}";
        }

        public void Interact(GameObject player)
        {
            if (_currentQuestIndex >= questSequence.Length)
            {
                // All quests complete — heartfelt thank you
                string finalLine = $"<b>{npcName}:</b> You've given me back my home, my history, my family. The resonance you carry... it's the same frequency as hope.";
                HUDController.Instance?.ShowLorePopup($"{npcName} — Gratitude", finalLine);
                AudioManager.Instance?.PlaySFX2D("NPCHeartfelt");
                return;
            }

            // Show quest-specific intro dialogue
            if (_currentQuestIndex < questIntros.Length && !string.IsNullOrEmpty(questIntros[_currentQuestIndex]))
            {
                DialogueManager.Instance?.PlayLineById($"{npcName.ToLower()}_quest_{_currentQuestIndex}_intro");
            }

            // Delegate to quest system
            _questGiver.questId = questSequence[_currentQuestIndex];
            _questGiver.giverName = npcName;
            _questGiver.Interact(player);

            // Check if quest was just completed
            var state = QuestManager.Instance?.GetQuestState(questSequence[_currentQuestIndex]);
            if (state.HasValue && state.Value.status == QuestStatus.Completed)
            {
                _currentQuestIndex++;
                
                // Show completion dialogue
                if (_currentQuestIndex - 1 < questCompletions.Length && !string.IsNullOrEmpty(questCompletions[_currentQuestIndex - 1]))
                {
                    DialogueManager.Instance?.PlayLineById($"{npcName.ToLower()}_quest_{_currentQuestIndex - 1}_complete");
                }
            }
        }
    }

    /// <summary>
    /// Lore Keeper NPC — non-quest NPCs who provide historical context, lore dumps, prophecies.
    /// Optional dialogue, enriches world-building, unlocks codex entries.
    /// </summary>
    [DisallowMultipleComponent]
    public class LoreKeeperNPC : MonoBehaviour, IInteractable
    {
        [Header("Lore Keeper")]
        [SerializeField] string keeperName = "Cassandra the Seer";
        [SerializeField] LoreCategory category = LoreCategory.Prophecy;
        [SerializeField, TextArea(4, 12)] string[] loreLines;
        [SerializeField] string[] codexUnlocks;

        enum LoreCategory { Prophecy, History, Architecture, Giants, Corruption }

        int _currentLineIndex;

        public string GetInteractPrompt() => _currentLineIndex < loreLines.Length 
            ? $"[E] Speak with {keeperName}" 
            : $"[E] {keeperName} (All lore shared)";

        public void Interact(GameObject player)
        {
            if (_currentLineIndex >= loreLines.Length)
            {
                // All lore exhausted — repeat final line or generic farewell
                HUDController.Instance?.ShowLorePopup(keeperName, "The old stories have all been told. Go now, and make your own.");
                return;
            }

            // Show lore dialogue
            string currentLore = loreLines[_currentLineIndex];
            HUDController.Instance?.ShowLorePopup($"{keeperName} — {category}", currentLore);
            AudioManager.Instance?.PlaySFX2D("LoreKeeperSpeak");

            // Unlock codex entry
            if (_currentLineIndex < codexUnlocks.Length && !string.IsNullOrEmpty(codexUnlocks[_currentLineIndex]))
                UI.CodexSystem.Instance?.UnlockEntry(codexUnlocks[_currentLineIndex]);

            _currentLineIndex++;
            Save.SaveManager.Instance?.MarkDirty();

            Debug.Log($"[LoreKeeperNPC] {keeperName} shared lore line {_currentLineIndex}/{loreLines.Length}");
        }
    }
}
