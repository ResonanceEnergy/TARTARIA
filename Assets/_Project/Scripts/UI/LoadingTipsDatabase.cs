using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace Tartaria.UI
{
    /// <summary>
    /// Loading Tips Database — shows randomized tips during loading screens.
    /// Improves perceived load time and educates players.
    /// Self-bootstraps, callable from any loading context.
    /// </summary>
    public class LoadingTipsDatabase : MonoBehaviour
    {
        public static LoadingTipsDatabase Instance { get; private set; }

        [Header("Tip Display")]
        [SerializeField] TextMeshProUGUI tipText;
        [SerializeField] float tipChangeInterval = 8f; // Change tip every 8s during long loads
        [SerializeField] bool randomizeOnStart = true;

        readonly List<string> _tips = new();
        int _currentTipIndex = -1;
        float _tipChangeTimer;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("[LoadingTipsDatabase]");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<LoadingTipsDatabase>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            LoadTips();
        }

        void LoadTips()
        {
            // Core Gameplay Tips
            _tips.Add("Aether Vision (V) reveals hidden frequency nodes and buried structures.");
            _tips.Add("Perfect-tuned buildings restore faster and grant bonus Resonance Score.");
            _tips.Add("Giant Mode recharges with Resonance Score — use it strategically in boss fights.");
            _tips.Add("Harmonic Strike (Spacebar) deals massive damage when enemy frequency matches yours.");
            _tips.Add("Explore caverns and ruins for rare loot, lore tablets, and hidden quests.");

            // Progression Tips
            _tips.Add("Level up to gain stat points — invest wisely in Vitality, Resonance, Strength, Agility, or Attunement.");
            _tips.Add("Each stat point in Vitality grants +10 HP. Survival is key in the deeper Moons.");
            _tips.Add("Resonance boosts ability power and Aether regeneration — essential for mages.");
            _tips.Add("Strength increases melee damage and carry weight — perfect for warriors.");
            _tips.Add("Agility improves dodge chance and movement speed — stay mobile, stay alive.");

            // Save & Persistence
            _tips.Add("The game autosaves every 10 seconds — your progress is always safe.");
            _tips.Add("Press F5 to quick-save, F9 to quick-load. Master this for tough encounters.");
            _tips.Add("Cloud sync ensures your save is safe even if your local file corrupts.");
            _tips.Add("The world remembers your choices — talk to companions to see how your actions matter.");

            // Combat Tips
            _tips.Add("Match enemy frequency before using Harmonic Strike for 3x damage.");
            _tips.Add("Screen shake and hit-stop indicate successful hits — watch for visual feedback.");
            _tips.Add("Critical hits show yellow damage numbers and deal double damage.");
            _tips.Add("Shield with Q to block incoming attacks — timing is everything.");
            _tips.Add("Dodge with Shift+direction to avoid powerful boss attacks.");

            // World & Exploration
            _tips.Add("Fast travel unlocks after clearing Moon 3 — activate obelisks to add waypoints.");
            _tips.Add("Return portals spawn at the center of each cleared Moon zone.");
            _tips.Add("Companions offer side quests and unique dialogue — get to know them.");
            _tips.Add("Lore tablets reveal the history of Tartaria — collect all 52 for the full story.");

            // Polish & QoL
            _tips.Add("Press F1 for a full list of keyboard and gamepad controls.");
            _tips.Add("Sort inventory by Type, Rarity, Name, or Weight for easy organization.");
            _tips.Add("Use the search field in inventory to quickly find specific items.");
            _tips.Add("Weight capacity is shown at the bottom of the inventory screen — don't carry too much!");

            // Meta Tips
            _tips.Add("This game was crafted with love and golden ratio principles. Enjoy the journey.");
            _tips.Add("Found a bug? Press F12 to open the debug console and report it.");
            _tips.Add("Join the community on Discord to share discoveries and theorize about the lore.");
        }

        void Update()
        {
            if (_tips.Count == 0) return;

            _tipChangeTimer -= Time.unscaledDeltaTime;
            if (_tipChangeTimer <= 0f)
            {
                _tipChangeTimer = tipChangeInterval;
                ShowNextTip();
            }
        }

        /// <summary>
        /// Show a random tip. Call this when loading screen appears.
        /// </summary>
        public static string GetRandomTip()
        {
            if (Instance == null) Bootstrap();
            if (Instance._tips.Count == 0) return "Loading...";
            return Instance._tips[Random.Range(0, Instance._tips.Count)];
        }

        /// <summary>
        /// Start cycling tips for long loads. Call when loading screen appears.
        /// </summary>
        public static void StartTipCycle()
        {
            if (Instance == null) Bootstrap();
            Instance._tipChangeTimer = Instance.tipChangeInterval;
            Instance.ShowNextTip();
        }

        /// <summary>
        /// Stop cycling tips. Call when loading completes.
        /// </summary>
        public static void StopTipCycle()
        {
            if (Instance == null) return;
            Instance._tipChangeTimer = float.MaxValue;
        }

        void ShowNextTip()
        {
            if (_tips.Count == 0) return;

            if (randomizeOnStart)
            {
                _currentTipIndex = Random.Range(0, _tips.Count);
            }
            else
            {
                _currentTipIndex = (_currentTipIndex + 1) % _tips.Count;
            }

            string tip = _tips[_currentTipIndex];

            // Broadcast to any listening UI
            Core.GameEvents.RaiseHUDShowObjective($"<i>{tip}</i>");

            if (tipText != null)
            {
                tipText.text = $"<color=#A0A0A0>TIP:</color> {tip}";
            }
        }

        /// <summary>
        /// Add a custom tip at runtime (e.g., context-specific loading tips).
        /// </summary>
        public static void AddCustomTip(string tip)
        {
            if (Instance == null) Bootstrap();
            if (!string.IsNullOrEmpty(tip))
            {
                Instance._tips.Add(tip);
            }
        }
    }
}
