using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.UI;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon Narrative Beat Manager — standardizes intro/mid/outro story beats for all 13 Moons.
    /// Ensures consistent narrative pacing, companion reactions, emotional arc.
    /// Integrates with MoonNarrativeController for beat triggers.
    /// </summary>
    [DisallowMultipleComponent]
    public class MoonNarrativeBeatManager : MonoBehaviour
    {
        public static MoonNarrativeBeatManager Instance { get; private set; }

        [Header("Current Moon")]
        [SerializeField] int moonNumber = 1;
        [SerializeField] string moonName = "Echohaven";

        [Header("Narrative Beats")]
        [SerializeField] MoonBeat introbeat;
        [SerializeField] MoonBeat[] midBeats;
        [SerializeField] MoonBeat outrobeat;

        readonly HashSet<string> _triggeredBeats = new();

        [System.Serializable]
        public struct MoonBeat
        {
            public string beatId;
            public string beatTitle;
            [TextArea(3, 8)] public string narrativeText;
            public string[] companionReactions;
            public string cinematicSequence;
            public string questTrigger;
            public bool showBanner;
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>
        /// Trigger Moon intro beat (called by Moon content spawner on zone load).
        /// </summary>
        public void TriggerIntro()
        {
            if (_triggeredBeats.Contains("intro")) return;
            _triggeredBeats.Add("intro");

            ExecuteBeat(introbeat, "INTRO");
            Debug.Log($"[MoonNarrativeBeat] Moon {moonNumber} — Intro beat triggered.");
        }

        /// <summary>
        /// Trigger specific mid-beat by index.
        /// </summary>
        public void TriggerMidBeat(int index)
        {
            if (index < 0 || index >= midBeats.Length) return;
            
            string beatKey = $"mid_{index}";
            if (_triggeredBeats.Contains(beatKey)) return;
            _triggeredBeats.Add(beatKey);

            ExecuteBeat(midBeats[index], $"MID-BEAT {index + 1}");
            Debug.Log($"[MoonNarrativeBeat] Moon {moonNumber} — Mid-beat {index} triggered.");
        }

        /// <summary>
        /// Trigger Moon outro beat (called on zone completion).
        /// </summary>
        public void TriggerOutro()
        {
            if (_triggeredBeats.Contains("outro")) return;
            _triggeredBeats.Add("outro");

            ExecuteBeat(outrobeat, "OUTRO");
            
            // Unlock next Moon
            MoonNarrativeController.Instance?.UnlockNextMoon();

            Debug.Log($"[MoonNarrativeBeat] Moon {moonNumber} — Outro beat triggered.");
        }

        void ExecuteBeat(MoonBeat beat, string beatType)
        {
            // Show banner
            if (beat.showBanner)
                GameEvents.RaiseHUDShowBanner($"{moonName} — {beatType}", beat.beatTitle);

            // Show narrative text
            if (!string.IsNullOrEmpty(beat.narrativeText))
                HUDController.Instance?.ShowLorePopup($"{moonName} — {beat.beatTitle}", beat.narrativeText);

            // Trigger companion reactions
            if (beat.companionReactions != null)
            {
                foreach (var reactionId in beat.companionReactions)
                {
                    if (!string.IsNullOrEmpty(reactionId))
                        DialogueManager.Instance?.PlayLineById(reactionId);
                }
            }

            // Play cinematic sequence (pending waypoint data integration)
            if (!string.IsNullOrEmpty(beat.cinematicSequence))
            {
                // var cinemaCam = FindFirstObjectByType<Camera.CinematicCameraController>();
                // cinemaCam?.PlaySequence(waypointData);  // TODO: Load from CinematicWaypointSequences
            }

            // Trigger quest
            if (!string.IsNullOrEmpty(beat.questTrigger))
                QuestManager.Instance?.ActivateQuest(beat.questTrigger);

            // Notify MoonNarrativeController
            MoonNarrativeController.Instance?.TriggerBeat($"moon{moonNumber}_{beat.beatId}");

            // Save progress
            Save.SaveManager.Instance?.MarkDirty();
        }

        /// <summary>
        /// Check if a beat has been triggered.
        /// </summary>
        public bool HasTriggeredBeat(string beatKey) => _triggeredBeats.Contains(beatKey);
    }

    /// <summary>
    /// Companion Reaction Enhancer — adds contextual companion reactions to player actions.
    /// Enriches companion personality through ambient comments, warnings, celebrations.
    /// </summary>
    [DisallowMultipleComponent]
    public class CompanionReactionEnhancer : MonoBehaviour
    {
        public static CompanionReactionEnhancer Instance { get; private set; }

        [Header("Reaction Timing")]
        [SerializeField] float minTimeBetweenReactions = 12f;
        [SerializeField, Range(0f, 1f)] float reactionChance = 0.6f;

        float _lastReactionTime;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Start()
        {
            // Subscribe to player action events (pending PlayerCharacter implementation)
            // PlayerCharacter.OnJump += OnPlayerJump;
            // RestorableBuilding.OnAnyBuildingRestored += OnBuildingRestored;
            // PlayerCharacter.OnDamageTaken += OnPlayerHurt;
            // PlayerCharacter.OnResonanceGained += OnResonanceGained;
        }

        void OnPlayerJump()
        {
            // Rare idle comment on excessive jumping
            if (Time.time - _lastReactionTime > 30f && Random.value < 0.05f)
            {
                DialogueManager.Instance?.PlayContextDialogue("companion_jumping_idle");
                _lastReactionTime = Time.time;
            }
        }

        void OnBuildingRestored(string buildingId)
        {
            if (!ShouldReact()) return;

            // Companions celebrate restoration
            DialogueManager.Instance?.PlayContextDialogue(DialogueContext.Restoration);
            
            // Specific companion reactions based on who's present
            var milo = MiloController.Instance;
            var lirael = LiraelController.Instance;

            if (milo != null && CompanionManager.Instance?.IsCompanionActive("milo") == true)
                milo.OnBuildingRestored(buildingId);

            if (lirael != null && CompanionManager.Instance?.IsCompanionActive("lirael") == true)
                lirael.OnBuildingRestored(buildingId);

            _lastReactionTime = Time.time;
        }

        void OnPlayerHurt(float damage)
        {
            if (!ShouldReact()) return;

            // Companions express concern
            DialogueManager.Instance?.PlayContextDialogue("companion_player_hurt");
            _lastReactionTime = Time.time;
        }

        void OnResonanceGained(float amount)
        {
            if (amount < 100f) return; // Only react to significant gains
            if (!ShouldReact()) return;

            // Companions acknowledge progress
            DialogueManager.Instance?.PlayContextDialogue("companion_resonance_milestone");
            _lastReactionTime = Time.time;
        }

        bool ShouldReact()
        {
            return Time.time - _lastReactionTime > minTimeBetweenReactions 
                && Random.value < reactionChance;
        }

        /// <summary>
        /// Trigger companion reaction to specific context.
        /// </summary>
        public void TriggerReaction(string context)
        {
            if (!ShouldReact()) return;

            DialogueManager.Instance?.PlayContextDialogue(context);
            _lastReactionTime = Time.time;

            Debug.Log($"[CompanionReactionEnhancer] Triggered reaction: {context}");
        }
    }

    /// <summary>
    /// Ending Choice Dialogue Manager — handles Moon 13 finale choice UI and companion reactions.
    /// Three endings: Harmony (restore balance), Echo (preserve memories), Reset (restart cycle).
    /// </summary>
    [DisallowMultipleComponent]
    public class EndingChoiceDialogueManager : MonoBehaviour
    {
        [Header("Ending Choices")]
        [SerializeField, TextArea(4, 10)] string harmonyDescription = "Restore the Aether balance. The world will heal, but the giants will never return. A new age begins.";
        [SerializeField, TextArea(4, 10)] string echoDescription = "Preserve the Echo memories. The past lives on in crystal and song. You become the Living Archive.";
        [SerializeField, TextArea(4, 10)] string resetDescription = "Restart the cycle. The Mud Flood reverses. History rewrites. You wake in Echohaven... again.";

        [Header("Companion Reactions")]
        [SerializeField, TextArea(3, 6)] string[] harmonyReactions;
        [SerializeField, TextArea(3, 6)] string[] echoReactions;
        [SerializeField, TextArea(3, 6)] string[] resetReactions;

        public void ShowEndingChoice()
        {
            // Present three-choice UI
            string[] choices = { "Harmony", "Echo", "Reset" };
            UI.ChoiceDialogUI.Instance?.ShowChoices(choices, OnEndingChosen);

            // Play final narrative beat
            MoonNarrativeController.Instance?.TriggerBeat("moon13_final_choice");

            Debug.Log("[EndingChoiceDialogue] Final choice presented to player.");
        }

        void OnEndingChosen(int choiceIndex)
        {
            string ending = choiceIndex == 0 ? "Harmony" : choiceIndex == 1 ? "Echo" : "Reset";

            // Save choice (SaveManager.SaveEndingChoice pending implementation)
            Save.SaveManager.Instance?.MarkDirty();

            // Show ending description
            string description = choiceIndex == 0 ? harmonyDescription : choiceIndex == 1 ? echoDescription : resetDescription;
            HUDController.Instance?.ShowLorePopup($"Ending: {ending}", description);

            // Trigger companion reactions
            string[] reactions = choiceIndex == 0 ? harmonyReactions : choiceIndex == 1 ? echoReactions : resetReactions;
            foreach (var reactionLine in reactions)
            {
                if (!string.IsNullOrEmpty(reactionLine))
                    DialogueManager.Instance?.PlayLineById(reactionLine);
            }

            // Play ending cinematic (pending waypoint data integration)
            // var cinemaCam = FindFirstObjectByType<Camera.CinematicCameraController>();
            // cinemaCam?.PlaySequence(waypointData);  // TODO: Load from CinematicWaypointSequences

            // Achievement unlock (AchievementToastUI pending implementation)
            Debug.Log($"[EndingChoiceDialogue] Achievement: Ending {ending}");

            Debug.Log($"[EndingChoiceDialogue] Player chose: {ending}");
        }
    }
}
