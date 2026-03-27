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

            // ── Lirael: Discovery lines (Moon 2+) ──
            AddLine("discovery", "lirael_disc_01", "Lirael",
                "I can see the original blueprint... faintly. The crystal memory still holds its shape.", true);
            AddLine("discovery", "lirael_disc_02", "Lirael",
                "This structure once channeled frequencies we've forgotten. Let me project what it should look like.");
            AddLine("discovery", "lirael_disc_03", "Lirael",
                "The fractals here are damaged but not destroyed. There's hope in the geometry.");

            // ── Lirael: Corruption lines ──
            AddLine("corruption_detected", "lirael_corr_01", "Lirael",
                "I sense dissonance ahead. Dark fractals -- someone has poisoned the Aether conduits.", true);
            AddLine("corruption_detected", "lirael_corr_02", "Lirael",
                "This corruption follows a pattern. Three stages to purge it: identify, isolate, purify.");
            AddLine("corruption_purged", "lirael_purge_01", "Lirael",
                "The crystal lattice is clean again. I'll remember this corruption pattern for next time.");
            AddLine("corruption_purged", "lirael_purge_02", "Lirael",
                "Beautiful. The Aether flows freely once more. Another memory preserved.");

            // ── Lirael: Tuning lines ──
            AddLine("tuning_start", "lirael_tune_01", "Lirael",
                "Listen for the crystal harmonic beneath the noise. I can amplify it for you.");
            AddLine("tuning_success", "lirael_tunesuc_01", "Lirael",
                "Perfect resonance. The crystals remember this frequency from before the burial.");
            AddLine("tuning_fail", "lirael_tunefail_01", "Lirael",
                "The frequency scattered. Don't force it -- let the crystal guide your hand.");

            // ── Lirael: Combat lines ──
            AddLine("combat_start", "lirael_cbt_01", "Lirael",
                "Fractal Wraiths -- twisted echoes of what these buildings once projected. Be careful, they drain Aether.", true);
            AddLine("combat_start", "lirael_cbt_02", "Lirael",
                "Wait for the materialise window. They're vulnerable for 1.5 seconds between phases.");
            AddLine("combat_victory", "lirael_cbtvic_01", "Lirael",
                "The wraith has dissolved. Its stolen Aether returns to the field. Nothing is truly lost.");

            // ── Lirael: Exploration / idle ──
            AddLine("exploration_idle", "lirael_idle_01", "Lirael",
                "Every crystal in Tartaria held a memory. The World Fairs displayed them as 'exhibits' then demolished them...");
            AddLine("exploration_idle", "lirael_idle_02", "Lirael",
                "I sang to these walls once, in another age. The resonance still answers.");
            AddLine("exploration_idle", "lirael_idle_03", "Lirael",
                "The Crystalline Caverns where I was born... they connected to every dome via harmonic tunnels.");

            // ── Lirael: Companion join ──
            AddLine("companion_join_lirael", "lirael_join_01", "Lirael",
                "I have waited centuries in crystal silence for someone who could hear the old frequencies. I will walk with you.", true);

            // ── Lirael: Blueprint projection ──
            AddLine("blueprint_projection", "lirael_blueprint_01", "Lirael",
                "Close your eyes and listen. I'll project the original blueprint into the Aether field.");
            AddLine("blueprint_projection", "lirael_blueprint_02", "Lirael",
                "This building's proportions follow the golden spiral perfectly. Look at the harmonic symmetry.");

            // ══════════════ CASSIAN DIALOGUE ══════════════

            // ── Cassian: Introduction ──
            AddLine("cassian_intro", "cassian_intro_01", "Cassian",
                "Another restorer? I didn't think anyone still remembered how to read the frequency maps. I'm Cassian. I've been... studying these ruins for some time.", true);

            // ── Cassian: Low trust ──
            AddLine("cassian_guarded", "cassian_low_trust_01", "Cassian",
                "My background? Let's just say I have my reasons for being here. Same as you, I imagine.");
            AddLine("cassian_guarded", "cassian_low_trust_02", "Cassian",
                "I wouldn't touch those crystals without proper calibration. But you seem confident enough.");
            AddLine("cassian_guarded", "cassian_low_trust_03", "Cassian",
                "The demolition records were thorough. Almost too thorough. Someone wanted these buildings forgotten.");

            // ── Cassian: Mid trust ──
            AddLine("cassian_intel", "cassian_mid_trust_01", "Cassian",
                "I've mapped the corruption vectors in this zone. The spread follows phi-spiral patterns -- predictable, if you know where to look.");
            AddLine("cassian_intel", "cassian_mid_trust_02", "Cassian",
                "Here, take this frequency signature. It should help you locate buried foundations. Consider it... professional courtesy.");

            // ── Cassian: High trust ──
            AddLine("cassian_confession", "cassian_high_trust_01", "Cassian",
                "I'll be honest with you. Not everything I've told you has been... entirely accurate. I had my orders. But watching what you've restored -- it changes things.");
            AddLine("cassian_confession", "cassian_high_trust_02", "Cassian",
                "They sent me to monitor you. The same faction that buried Tartaria the first time. They're afraid the technology will surface again.", true);
            AddLine("cassian_confession", "cassian_high_trust_03", "Cassian",
                "I've seen what Aether energy can do in the right hands. Maybe it's time I stopped reporting back and started helping properly.");

            // ── Cassian: Idle ──
            AddLine("cassian_idle", "cassian_idle_01", "Cassian",
                "These floor mosaics aren't decorative. They're circuit diagrams. Took me years to realise that.");
            AddLine("cassian_idle", "cassian_idle_02", "Cassian",
                "Interesting. The corruption density has shifted since yesterday. Something is accelerating the decay.");
            AddLine("cassian_idle", "cassian_idle_03", "Cassian",
                "Don't mind me. Just... taking measurements. For my own records.");
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
