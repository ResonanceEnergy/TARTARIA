using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;
using Yarn.Unity;

namespace Tartaria.Integration
{
    /// <summary>
    /// Sprint 7 Lane 6 — Yarn Tutorial Binding.
    ///
    /// Bridges <see cref="GameEvents.OnHUDShowDialogue"/> (Action&lt;string speaker, string message&gt;)
    /// to the scene's <see cref="DialogueRunner"/> so that
    /// <see cref="GameEvents.RaiseHUDShowDialogue"/> actually plays the matching Yarn node.
    ///
    /// Subscriber-side wiring: see GameEvents.cs:617 (RaiseHUDShowDialogue) and GameEvents.cs:237
    /// (the OnHUDShowDialogue event field) per API_CONTRACT §2.
    ///
    /// Lookup table maps a speaker display-name (the first arg of RaiseHUDShowDialogue) to the
    /// Yarn node title to start. Unknown speakers and missing-runner conditions emit a single
    /// warning and skip — no silent fallback per the Sprint 7 spec.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class YarnTutorialBinding : MonoBehaviour
    {
        private const string BootstrapObjectName = "[YarnTutorialBinding]";

        // Seeded speaker -> Yarn node lookup (Sprint 7 Lane 6 spec).
        private static readonly Dictionary<string, string> DefaultSpeakerToNode =
            new Dictionary<string, string>
            {
                { "Milo Brightway", "Milo_TutorialIntro" },
                { "Lirael",         "Lirael_Lullaby"     },
                { "Anastasia",      "Anastasia_Greeting" },
                { "Cassian",        "Cassian_BossIntro"  },
            };

        private readonly Dictionary<string, string> _speakerToNode =
            new Dictionary<string, string>(DefaultSpeakerToNode);

        private bool _subscribed;

        /// <summary>
        /// Bootstraps a persistent <see cref="YarnTutorialBinding"/> after every scene load.
        /// Idempotent: bails if an instance already lives in DontDestroyOnLoad.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BootstrapAfterSceneLoad()
        {
            // Unity 6 API: include inactive objects so a disabled bootstrap still counts.
            var existing = Object.FindFirstObjectByType<YarnTutorialBinding>(FindObjectsInactive.Include);
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
        /// Registers (or overrides) a speaker -> Yarn node mapping at runtime. Useful for
        /// per-Moon binding modules that want to inject additional entries without editing
        /// this file.
        /// </summary>
        public void RegisterSpeaker(string speaker, string nodeName)
        {
            if (string.IsNullOrWhiteSpace(speaker) || string.IsNullOrWhiteSpace(nodeName))
            {
                Debug.LogWarning(
                    "[YarnTutorialBinding] RegisterSpeaker called with empty speaker or node — ignored.");
                return;
            }

            _speakerToNode[speaker] = nodeName;
        }

        private void HandleHUDShowDialogue(string speaker, string message)
        {
            if (string.IsNullOrEmpty(speaker))
            {
                Debug.LogWarning(
                    $"[YarnTutorialBinding] Dialogue event with empty speaker — message=\"{message}\". Skipping.");
                return;
            }

            if (!_speakerToNode.TryGetValue(speaker, out var nodeName) || string.IsNullOrEmpty(nodeName))
            {
                Debug.LogWarning(
                    $"[YarnTutorialBinding] No Yarn node registered for speaker \"{speaker}\" (message=\"{message}\"). Skipping.");
                return;
            }

            // Unity 6 API: include inactive so a runner on a disabled GameObject is still found.
            var runner = Object.FindFirstObjectByType<DialogueRunner>(FindObjectsInactive.Include);
            if (runner == null)
            {
                Debug.LogWarning(
                    $"[YarnTutorialBinding] No DialogueRunner in scene — cannot play node \"{nodeName}\" for speaker \"{speaker}\" (message=\"{message}\").");
                return;
            }

            if (!runner.NodeExists(nodeName))
            {
                Debug.LogWarning(
                    $"[YarnTutorialBinding] DialogueRunner found but node \"{nodeName}\" is not loaded (speaker=\"{speaker}\", message=\"{message}\"). Skipping.");
                return;
            }

            if (runner.IsDialogueRunning)
            {
                // Stop any in-flight conversation so the new node takes over deterministically.
                runner.Stop();
            }

            runner.StartDialogue(nodeName);
        }
    }
}
