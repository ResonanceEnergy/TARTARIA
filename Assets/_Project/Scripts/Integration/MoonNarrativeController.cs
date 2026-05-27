using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Save;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon Narrative Controller — triggers story beats, dialogue, cinematics.
    /// Coordinates Moon content spawners with dialogue/camera/quest systems.
    /// Singleton orchestrator for all 13 Moons narrative flow.
    /// </summary>
    public class MoonNarrativeController : MonoBehaviour, ISaveDataProvider
    {
        public static MoonNarrativeController Instance { get; private set; }

        [Header("Moon Progress")]
        [SerializeField] int currentMoonNumber = 1;  // Echohaven (tutorial)

        [Header("Story Beats")]
        readonly HashSet<string> _triggeredBeats = new();

        public int CurrentMoon => currentMoonNumber;
        public bool IsMoonUnlocked(int moonNumber) => moonNumber <= currentMoonNumber;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("MoonNarrativeController");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<MoonNarrativeController>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            LoadNarrativeStateFromSave();
        }

        // === Public API ===

        /// <summary>
        /// Trigger a named story beat (only once per save).
        /// </summary>
        public void TriggerBeat(string beatId)
        {
            if (_triggeredBeats.Contains(beatId)) return;

            _triggeredBeats.Add(beatId);
            Debug.Log($"[MoonNarrative] Triggered beat: {beatId}");

            // Route to appropriate systems
            switch (beatId)
            {
                case "moon2_intro":
                    DialogueManager.Instance?.PlayContextDialogue("cassian_moon2_intro");
                    break;

                case "moon3_orphan_reveal":
                    DialogueManager.Instance?.PlayContextDialogue("helena_orphan_train");
                    // Trigger cinematic camera sequence (CinematicCameraController integration pending)
                    // var cinemaCam = FindObjectOfType<Camera.CinematicCameraController>();
                    // cinemaCam?.PlaySequence(waypointData);  // TODO: Load waypoint data from CinematicWaypointSequences
                    break;

                case "moon4_guardian_awakening":
                    DialogueManager.Instance?.PlayContextDialogue("narrator_guardian_golem");
                    // Play boss intro cinematic (timeline or camera sequence)
                    Debug.Log($"[MoonNarrative] Playing boss intro for Moon {currentMoonNumber}");
                    break;

                case "moon10_zereth_revelation":
                    DialogueManager.Instance?.PlayContextDialogue("narrator_zereth_victim");
                    GameEvents.RaiseHUDShowBanner("REVELATION", "Zereth was not the villain...");
                    break;

                case "moon13_final_choice":
                    DialogueManager.Instance?.PlayContextDialogue("narrator_final_choice");
                    // Show ending choice UI (Harmony/Echo/Reset)
                    UI.ChoiceDialogUI.Instance?.ShowChoices(new string[] { "Harmony", "Echo", "Reset" }, OnEndingChosen);
                    break;

                default:
                    Debug.LogWarning($"[MoonNarrative] Unknown beat: {beatId}");
                    break;
            }
        }

        /// <summary>
        /// Check if a beat has already triggered.
        /// </summary>
        public bool HasTriggeredBeat(string beatId)
        {
            return _triggeredBeats.Contains(beatId);
        }

        /// <summary>
        /// Advance to next Moon (called by Moon spawners on completion).
        /// </summary>
        public void UnlockNextMoon()
        {
            if (currentMoonNumber >= 13) return;  // Already at final Moon

            currentMoonNumber++;
            Debug.Log($"[MoonNarrative] Unlocked Moon {currentMoonNumber}");

            // Show Moon unlock banner
            GameEvents.RaiseHUDShowBanner($"MOON {currentMoonNumber} UNLOCKED", GetMoonTitle(currentMoonNumber));

            // Save progress
            SaveNarrativeState();
        }

        /// <summary>
        /// Get Moon title for display.
        /// </summary>
        public string GetMoonTitle(int moonNumber)
        {
            switch (moonNumber)
            {
                case 1: return "Echohaven (Tutorial)";
                case 2: return "Crystalline Caverns";
                case 3: return "Windswept Highlands";
                case 4: return "Deep Forge";
                case 5: return "White City Pavilions";
                case 6: return "Living Library";
                case 7: return "Frozen Korath";
                case 8: return "Skyfarer Ports";
                case 9: return "Prophecy Ruins";
                case 10: return "Continental Rail Network";
                case 11: return "Planetary Fountains";
                case 12: return "Bell Tower Network";
                case 13: return "Final Node";
                default: return "Unknown Moon";
            }
        }

        void LoadNarrativeStateFromSave()
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.RegisterProvider(this);
                Debug.Log("[MoonNarrative] Registered with SaveManager");
            }
        }

        public void SaveNarrativeState()
        {
            SaveManager.Instance?.Save();
        }

        // ISaveDataProvider implementation
        string ISaveDataProvider.GetProviderKey() => "MoonNarrative";

        object ISaveDataProvider.GetSaveData()
        {
            return new MoonNarrativeSaveData
            {
                currentMoonNumber = currentMoonNumber,
                triggeredBeats = new List<string>(_triggeredBeats)
            };
        }

        void ISaveDataProvider.RestoreSaveData(object data)
        {
            if (data is string json)
            {
                var saveData = JsonUtility.FromJson<MoonNarrativeSaveData>(json);
                currentMoonNumber = saveData.currentMoonNumber;
                _triggeredBeats.Clear();
                if (saveData.triggeredBeats != null)
                {
                    foreach (var beat in saveData.triggeredBeats)
                    {
                        _triggeredBeats.Add(beat);
                    }
                }
                Debug.Log($"[MoonNarrative] State restored: Moon {currentMoonNumber}, {_triggeredBeats.Count} beats");
            }
        }

        [System.Serializable]
        class MoonNarrativeSaveData
        {
            public int currentMoonNumber;
            public List<string> triggeredBeats;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void OnEndingChosen(int choiceIndex)
        {
            string ending = choiceIndex == 0 ? "Harmony" : choiceIndex == 1 ? "Echo" : "Reset";
            Debug.Log($"[MoonNarrative] Ending chosen: {ending}");
            // Save choice (SaveManager integration pending)
        }
    }
}
