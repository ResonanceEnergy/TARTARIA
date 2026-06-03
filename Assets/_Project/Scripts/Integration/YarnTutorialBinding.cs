using System;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;
using Yarn.Unity;

namespace Tartaria.Integration
{
    /// <summary>
    /// Sprint 7 Lane 6 — Yarn Tutorial Binding (Sprint 11 L7 fix, origin cec511a9).
    ///
    /// Bridges <see cref="GameEvents.OnHUDShowDialogue"/> (Action&lt;string speaker, string message&gt;,
    /// verified GameEvents.cs:237) to the scene's <see cref="DialogueRunner"/> so that
    /// <see cref="GameEvents.RaiseHUDShowDialogue"/> (GameEvents.cs:617) actually plays the
    /// matching Yarn node.
    ///
    /// HISTORICAL BUG (Sprint 11 L7 audit cec511a9):
    ///   - Old map keyed on display names: "Milo Brightway" / "Lirael" / "Anastasia" / "Cassian".
    ///   - Old map values were PascalCase ("Milo_TutorialIntro", "Lirael_Lullaby", etc.).
    ///   - MiloTutorialFlow (Scripts/AI/MiloTutorialFlow.cs:78 kSpeaker = "Milo") passes speaker="Milo".
    ///   - The actual Yarn node titles on disk are snake_case
    ///     (Assets/_Project/Dialogue/Echohaven/milo_tutorial.yarn lines 1/9/17/25/33/41/49 +
    ///      Moon1/{anastasia_greeting,lirael,cassian,milo_intro}.yarn).
    ///   - DialogueRunner.NodeExists is case-sensitive.
    ///   - Net effect: every tutorial step missed the lookup and died silently.
    ///
    /// FIX:
    ///   1. Primary lookup is (speaker, message) → snake_case node title. The 6 MiloTutorialFlow
    ///      step lines + the skip line map 1:1 to the 7 nodes in milo_tutorial.yarn.
    ///   2. Secondary lookup is speaker → default node, used when the (speaker, message) miss
    ///      and we still want a sane fallback (e.g. NPCs whose dialogue text varies per state).
    ///   3. Every node name in the default tables has been verified to exist on disk; see file
    ///      headers / grep evidence in the PR summary.
    ///
    /// All catches log ex.GetType().Name + ex.Message + the value that broke, per CLAUDE.md
    /// NO-DEBT rule. No stub bodies, per NO-STUBS rule.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class YarnTutorialBinding : MonoBehaviour
    {
        private const string BootstrapObjectName = "[YarnTutorialBinding]";

        // ----------------------------------------------------------------
        // Seeded (speaker, message) -> Yarn node title.
        // Keys are EXACTLY the (speaker, message) strings MiloTutorialFlow passes
        // (Scripts/AI/MiloTutorialFlow.cs:78-87 kSpeaker + kStep1Line..kStep6Line + kSkipLine).
        // Values are the snake_case node titles verified to exist in
        // Assets/_Project/Dialogue/Echohaven/milo_tutorial.yarn (lines 1/9/17/25/33/41/49).
        // ----------------------------------------------------------------
        private static readonly Dictionary<SpeakerLine, string> DefaultSpeakerLineToNode =
            new Dictionary<SpeakerLine, string>
            {
                // Milo tutorial 6-step flow + skip
                { new SpeakerLine("Milo", "Look toward the firelight, traveler."),                    "milo_tutorial_step_1_brazier"  },
                { new SpeakerLine("Milo", "Press E to interact with the world."),                     "milo_tutorial_step_2_interact" },
                { new SpeakerLine("Milo", "Follow the arrow. The buried cathedral is closer than it looks."), "milo_tutorial_step_3_waypoint" },
                { new SpeakerLine("Milo", "Press E at the green light."),                             "milo_tutorial_step_4_tune"     },
                { new SpeakerLine("Milo", "One building back from the silence. You've got the knack now."),   "milo_tutorial_step_5_restored" },
                { new SpeakerLine("Milo", "Explore. You have buildings to wake."),                    "milo_tutorial_step_6_free"     },
                { new SpeakerLine("Milo", "Suit yourself. The valley's yours to read."),              "milo_tutorial_skipped"         },
            };

        // ----------------------------------------------------------------
        // Speaker -> default node fallback (used when the (speaker, message) pair
        // doesn't match an explicit entry above). All four nodes verified to exist:
        //   milo_intro            -> Assets/_Project/Dialogue/Moon1/milo_intro.yarn:1
        //   lirael_first_meet     -> Assets/_Project/Dialogue/Moon1/lirael.yarn:1
        //   anastasia_greeting    -> Assets/_Project/Dialogue/Moon1/anastasia_greeting.yarn:1
        //   cassian_first_meet    -> Assets/_Project/Dialogue/Moon1/cassian.yarn:1
        // ----------------------------------------------------------------
        private static readonly Dictionary<string, string> DefaultSpeakerToNode =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "Milo",      "milo_intro"         },
                { "Lirael",    "lirael_first_meet"  },
                { "Anastasia", "anastasia_greeting" },
                { "Cassian",   "cassian_first_meet" },
            };

        // Live runtime copies so RegisterSpeaker / RegisterSpeakerLine can extend without
        // mutating the static defaults.
        private readonly Dictionary<SpeakerLine, string> _speakerLineToNode =
            new Dictionary<SpeakerLine, string>(DefaultSpeakerLineToNode);

        private readonly Dictionary<string, string> _speakerToNode =
            new Dictionary<string, string>(DefaultSpeakerToNode, StringComparer.Ordinal);

        private bool _subscribed;

        /// <summary>
        /// Bootstraps a persistent <see cref="YarnTutorialBinding"/> after every scene load.
        /// Idempotent: bails if an instance already lives in DontDestroyOnLoad.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BootstrapAfterSceneLoad()
        {
            // Unity 6 API: include inactive objects so a disabled bootstrap still counts.
            var existing = UnityEngine.Object.FindFirstObjectByType<YarnTutorialBinding>(FindObjectsInactive.Include);
            if (existing != null)
            {
                return;
            }

            var go = new GameObject(BootstrapObjectName);
            DontDestroyOnLoad(go);
            go.AddComponent<YarnTutorialBinding>();
        }

        private void Awake()
        {
            if (_subscribed)
            {
                return;
            }

            GameEvents.OnHUDShowDialogue += HandleHUDShowDialogue;
            _subscribed = true;
        }

        private void OnDestroy()
        {
            if (!_subscribed)
            {
                return;
            }

            GameEvents.OnHUDShowDialogue -= HandleHUDShowDialogue;
            _subscribed = false;
        }

        /// <summary>
        /// Registers (or overrides) a speaker -> default Yarn node mapping at runtime.
        /// Used by per-Moon binding modules that want to inject the default node without
        /// editing this file. The (speaker, message) overload takes precedence.
        /// </summary>
        public void RegisterSpeaker(string speaker, string nodeName)
        {
            if (string.IsNullOrWhiteSpace(speaker) || string.IsNullOrWhiteSpace(nodeName))
            {
                Debug.LogWarning(
                    $"[YarnTutorialBinding] RegisterSpeaker called with empty speaker or node — ignored (speaker=\"{speaker}\", node=\"{nodeName}\").");
                return;
            }

            _speakerToNode[speaker] = nodeName;
        }

        /// <summary>
        /// Registers (or overrides) a (speaker, message) -> Yarn node mapping at runtime.
        /// Use this when one speaker has multiple beats, each routed to a different node
        /// (e.g. the Milo tutorial's 6 step lines).
        /// </summary>
        public void RegisterSpeakerLine(string speaker, string message, string nodeName)
        {
            if (string.IsNullOrWhiteSpace(speaker) || string.IsNullOrWhiteSpace(message) || string.IsNullOrWhiteSpace(nodeName))
            {
                Debug.LogWarning(
                    $"[YarnTutorialBinding] RegisterSpeakerLine called with empty arg — ignored (speaker=\"{speaker}\", message=\"{message}\", node=\"{nodeName}\").");
                return;
            }

            _speakerLineToNode[new SpeakerLine(speaker, message)] = nodeName;
        }

        private void HandleHUDShowDialogue(string speaker, string message)
        {
            if (string.IsNullOrEmpty(speaker))
            {
                Debug.LogWarning(
                    $"[YarnTutorialBinding] Dialogue event with empty speaker — message=\"{message}\". Skipping.");
                return;
            }

            string nodeName = ResolveNodeName(speaker, message);
            if (string.IsNullOrEmpty(nodeName))
            {
                Debug.LogWarning(
                    $"[YarnTutorialBinding] No Yarn node registered for speaker \"{speaker}\" (message=\"{message}\"). Skipping.");
                return;
            }

            // Unity 6 API: include inactive so a runner on a disabled GameObject is still found.
            var runner = UnityEngine.Object.FindFirstObjectByType<DialogueRunner>(FindObjectsInactive.Include);
            if (runner == null)
            {
                Debug.LogWarning(
                    $"[YarnTutorialBinding] No DialogueRunner in scene — cannot play node \"{nodeName}\" for speaker \"{speaker}\" (message=\"{message}\").");
                return;
            }

            bool nodeExists;
            try
            {
                nodeExists = runner.NodeExists(nodeName);
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[YarnTutorialBinding] DialogueRunner.NodeExists threw {ex.GetType().Name}: {ex.Message} (node=\"{nodeName}\", speaker=\"{speaker}\", message=\"{message}\").");
                return;
            }

            if (!nodeExists)
            {
                Debug.LogWarning(
                    $"[YarnTutorialBinding] DialogueRunner found but node \"{nodeName}\" is not loaded (speaker=\"{speaker}\", message=\"{message}\"). Skipping.");
                return;
            }

            try
            {
                if (runner.IsDialogueRunning)
                {
                    // Stop any in-flight conversation so the new node takes over deterministically.
                    runner.Stop();
                }

                runner.StartDialogue(nodeName);
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[YarnTutorialBinding] DialogueRunner.StartDialogue threw {ex.GetType().Name}: {ex.Message} (node=\"{nodeName}\", speaker=\"{speaker}\", message=\"{message}\").");
            }
        }

        /// <summary>
        /// Two-level lookup: try the (speaker, message) map first, fall back to the
        /// speaker-only map. Returns null if neither hits.
        /// </summary>
        private string ResolveNodeName(string speaker, string message)
        {
            // 1. Exact (speaker, message) match — primary path for MiloTutorialFlow's 6 steps + skip.
            if (!string.IsNullOrEmpty(message))
            {
                var key = new SpeakerLine(speaker, message);
                if (_speakerLineToNode.TryGetValue(key, out var node) && !string.IsNullOrEmpty(node))
                {
                    return node;
                }
            }

            // 2. Speaker-only fallback — generic NPC greeting node.
            if (_speakerToNode.TryGetValue(speaker, out var fallback) && !string.IsNullOrEmpty(fallback))
            {
                return fallback;
            }

            return null;
        }

        /// <summary>
        /// (speaker, message) composite key used as the primary lookup. Ordinal equality so
        /// matches behave exactly like Yarn's case-sensitive NodeExists comparison.
        /// </summary>
        private readonly struct SpeakerLine : IEquatable<SpeakerLine>
        {
            public readonly string Speaker;
            public readonly string Message;

            public SpeakerLine(string speaker, string message)
            {
                Speaker = speaker ?? string.Empty;
                Message = message ?? string.Empty;
            }

            public bool Equals(SpeakerLine other)
            {
                return string.Equals(Speaker, other.Speaker, StringComparison.Ordinal)
                    && string.Equals(Message, other.Message, StringComparison.Ordinal);
            }

            public override bool Equals(object obj) => obj is SpeakerLine other && Equals(other);

            public override int GetHashCode()
            {
                unchecked
                {
                    int h = Speaker != null ? StringComparer.Ordinal.GetHashCode(Speaker) : 0;
                    h = (h * 397) ^ (Message != null ? StringComparer.Ordinal.GetHashCode(Message) : 0);
                    return h;
                }
            }
        }
    }
}
