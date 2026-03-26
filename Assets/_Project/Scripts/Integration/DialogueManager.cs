using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;
using Tartaria.UI;

namespace Tartaria.Integration
{
    /// <summary>
    /// Dialogue Manager — loads and plays context-sensitive dialogue lines.
    /// Phase 1 focuses on Milo (companion) with contextual quips for
    /// discovery, tuning, combat, and exploration.
    ///
    /// Lines are loaded from a built-in database. Future versions will
    /// support external JSON and branching dialogue trees.
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] float minTimeBetweenLines = 8f;
        [SerializeField] float autoCloseDelay = 5f;

        float _lastLineTime = -999f;
        readonly Dictionary<string, List<DialogueLine>> _contextLines = new();
        readonly HashSet<string> _playedOneShots = new();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            BuildDatabase();
        }

        // ─── Public API ──────────────────────────────

        /// <summary>
        /// Play a context-appropriate dialogue line.
        /// Contexts: discovery, tuning_start, tuning_fail, tuning_success,
        ///           restoration, combat_start, combat_victory, exploration_idle,
        ///           aether_wake, zone_shift, zone_complete
        /// </summary>
        public void PlayContextDialogue(string context)
        {
            if (Time.time - _lastLineTime < minTimeBetweenLines) return;

            if (!_contextLines.TryGetValue(context, out var lines) || lines.Count == 0)
                return;

            // Pick a random unplayed line, or any if all played
            DialogueLine chosen = null;
            for (int i = 0; i < lines.Count; i++)
            {
                int idx = Random.Range(0, lines.Count);
                if (!lines[idx].oneShot || !_playedOneShots.Contains(lines[idx].id))
                {
                    chosen = lines[idx];
                    break;
                }
            }

            if (chosen == null)
            {
                // All one-shots played; pick any non-oneshot
                foreach (var l in lines)
                {
                    if (!l.oneShot) { chosen = l; break; }
                }
            }

            if (chosen == null) return;

            ShowLine(chosen);
        }

        /// <summary>
        /// Play a specific line by ID.
        /// </summary>
        public void PlayLineById(string lineId)
        {
            foreach (var contextPair in _contextLines)
            {
                foreach (var line in contextPair.Value)
                {
                    if (line.id == lineId)
                    {
                        ShowLine(line);
                        return;
                    }
                }
            }
        }

        // ─── Display ─────────────────────────────────

        void ShowLine(DialogueLine line)
        {
            _lastLineTime = Time.time;

            if (line.oneShot)
                _playedOneShots.Add(line.id);

            UIManager.Instance?.ShowDialogue(line.speaker, line.text);

            // Auto-close after delay
            CancelInvoke(nameof(HideLine));
            Invoke(nameof(HideLine), autoCloseDelay);

            Debug.Log($"[Dialogue] {line.speaker}: {line.text}");
        }

        void HideLine()
        {
            UIManager.Instance?.HideDialogue();
        }

        // ─── Database ────────────────────────────────

        void BuildDatabase()
        {
            // ── Milo: Discovery lines ──
            AddLine("discovery", "milo_disc_01", "Milo",
                "Do you feel that? The ground is humming... Something is buried here!", true);
            AddLine("discovery", "milo_disc_02", "Milo",
                "Look at these proportions — the golden ratio! This was built with purpose.");
            AddLine("discovery", "milo_disc_03", "Milo",
                "*ears perk up* I can hear it resonating beneath the mud. A Tartarian structure!");
            AddLine("discovery", "milo_disc_04", "Milo",
                "The Aether field is stronger here. Ancient builders knew where to place their cities.");

            // ── Milo: Tuning lines ──
            AddLine("tuning_start", "milo_tune_01", "Milo",
                "Match the frequency — 432 Hz. The universal tone of healing.", true);
            AddLine("tuning_start", "milo_tune_02", "Milo",
                "Feel the vibration? When it resonates through your whole body, you're close.");
            AddLine("tuning_success", "milo_tunesuc_01", "Milo",
                "Beautiful! The harmonic alignment is perfect. The building remembers!");
            AddLine("tuning_success", "milo_tunesuc_02", "Milo",
                "Did you feel that cascade? PHI times the base frequency. Pure resonance.");
            AddLine("tuning_fail", "milo_tunefail_01", "Milo",
                "The frequency drifted. Take a breath, find center, and try again.");
            AddLine("tuning_fail", "milo_tunefail_02", "Milo",
                "Almost! Remember — 432 Hz. Not sharper, not flatter. The universe has one key.");

            // ── Milo: Restoration lines ──
            AddLine("restoration", "milo_rest_01", "Milo",
                "*tail wagging furiously* Look at it RISE! A thousand years of mud, gone! This is what we're fighting for!", true);
            AddLine("restoration", "milo_rest_02", "Milo",
                "The building is generating Aether again. Can you feel the field strengthening?");
            AddLine("restoration", "milo_rest_03", "Milo",
                "They tried to bury this civilization. But resonance doesn't die. It waits.");

            // ── Milo: Combat lines ──
            AddLine("combat_start", "milo_cbt_01", "Milo",
                "*growls* Mud Golem! The corruption's fighting back. Use your Resonance Pulse!", true);
            AddLine("combat_start", "milo_cbt_02", "Milo",
                "This creature is pure dissonance. Retune it — don't destroy it. Purify the frequency!");
            AddLine("combat_victory", "milo_cbtvic_01", "Milo",
                "The Golem dissolved back into clean earth. That's not destruction — that's restoration.");
            AddLine("combat_victory", "milo_cbtvic_02", "Milo",
                "*happy bark* The corruption weakens. Every Golem purified makes the Aether clearer.");

            // ── Milo: Exploration / idle ──
            AddLine("exploration_idle", "milo_idle_01", "Milo",
                "This place was grand once. The World Fairs showed it to millions, then they buried it.");
            AddLine("exploration_idle", "milo_idle_02", "Milo",
                "Can you feel the ley lines beneath us? 3-6-9... Tesla was onto something.");
            AddLine("exploration_idle", "milo_idle_03", "Milo",
                "Echohaven. Echo of haven. Even the name tells you what was lost.");
            AddLine("exploration_idle", "milo_idle_04", "Milo",
                "The old world ran on free energy. No wires, no bills. Just Aether.");

            // ── Milo: Threshold events ──
            AddLine("aether_wake", "milo_aw_01", "Milo",
                "The Aether is flowing freely now! Look — golden mist! The field is waking up!", true);
            AddLine("zone_shift", "milo_zs_01", "Milo",
                "The whole zone is transforming! Color is returning! This is what Echohaven looked like!", true);
            AddLine("zone_complete", "milo_zc_01", "Milo",
                "*howling with joy* Echohaven is ALIVE again! Every building singing in harmony!", true);
        }

        void AddLine(string context, string id, string speaker, string text, bool oneShot = false)
        {
            if (!_contextLines.ContainsKey(context))
                _contextLines[context] = new List<DialogueLine>();

            _contextLines[context].Add(new DialogueLine
            {
                id = id,
                speaker = speaker,
                text = text,
                context = context,
                oneShot = oneShot
            });
        }

        // ─── Data Types ──────────────────────────────

        class DialogueLine
        {
            public string id;
            public string speaker;
            public string text;
            public string context;
            public bool oneShot;
        }
    }
}
