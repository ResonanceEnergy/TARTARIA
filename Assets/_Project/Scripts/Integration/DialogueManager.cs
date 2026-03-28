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

            // ══════════════ THORNE DIALOGUE ══════════════
            // Commander Thorne — grizzled veteran of the Resonance Wars, now leads
            // the Restoration militia. Pragmatic, protective, scarred by betrayal.

            // ── Thorne: Introduction (Moon 3+) ──
            AddLine("thorne_intro", "thorne_intro_01", "Thorne",
                "Stand down. Identify yourself. ...A restorer? Haven't seen one of you in decades. I'm Commander Thorne. I kept the militia alive after the Fall.", true);
            AddLine("thorne_intro", "thorne_intro_02", "Thorne",
                "You've reawakened the Echohaven bell tower. That signal carried farther than you realize. Both hope and danger follow.");

            // ── Thorne: Combat briefing ──
            AddLine("thorne_combat_briefing", "thorne_brief_01", "Thorne",
                "Intel says the corruption is concentrating. Wraith nests, three klicks northeast. We hit them before they harden.");
            AddLine("thorne_combat_briefing", "thorne_brief_02", "Thorne",
                "Rule one: never engage a golem pack without shielding. Rule two: the wraiths phase — wait for the solidify window.");
            AddLine("thorne_combat_briefing", "thorne_brief_03", "Thorne",
                "Your pulse technique is sloppy. You're leaking Aether with every swing. But you've got heart. We can work with that.");

            // ── Thorne: Combat reactions ──
            AddLine("combat_start", "thorne_cbt_01", "Thorne",
                "Contact! Weapons hot. Restorer, pulse formation — I'll draw their fire!");
            AddLine("combat_victory", "thorne_cbtvic_01", "Thorne",
                "Sector clear. Good work. The corruption recedes when you purify at the source.");
            AddLine("combat_victory", "thorne_cbtvic_02", "Thorne",
                "That's how we did it in the old campaigns. Clean, precise, harmonized. You're learning.");

            // ── Thorne: Trust arc ──
            AddLine("thorne_guarded", "thorne_guard_01", "Thorne",
                "I've seen restorers come and go. Most break when they learn how deep the burial goes. Let's see if you're different.");
            AddLine("thorne_guarded", "thorne_guard_02", "Thorne",
                "Trust is earned, not given. Especially after what happened at Chronopolis.");
            AddLine("thorne_trusted", "thorne_trust_01", "Thorne",
                "I lost my entire platoon defending the Celestial Spire. Only building that survived... until they dynamited it in 1904.", true);
            AddLine("thorne_trusted", "thorne_trust_02", "Thorne",
                "You've proven yourself. The militia stands with you. Whatever comes next — the order, the corruption, the truth — we face it together.");

            // ── Thorne: Strategy / exploration ──
            AddLine("exploration_idle", "thorne_idle_01", "Thorne",
                "These fortifications aren't medieval — the angle of deflection is too precise. Somebody understood electromagnetic shielding.");
            AddLine("exploration_idle", "thorne_idle_02", "Thorne",
                "Every zone has a keystone building. Restore it first and the rest fall into alignment. Military doctrine.");
            AddLine("thorne_strategy", "thorne_strat_01", "Thorne",
                "The corruption moves in three patterns: creep, surge, and cascade. Learn to read the field and you'll never be flanked.");

            // ── Thorne: Zone events ──
            AddLine("zone_complete", "thorne_zc_01", "Thorne",
                "Zone secured. The ley lines are humming again. Establish a picket line — the corruption will push back.");
            AddLine("corruption_detected", "thorne_corr_01", "Thorne",
                "Corruption signature detected. Heavy concentration. This isn't natural decay — someone is feeding it.");

            // ══════════════ KORATH DIALOGUE ══════════════
            // Korath — mysterious figure connected to the original Tartarian builders.
            // Speaks in riddles and frequencies. Guardian of the Day Out of Time.

            // ── Korath: Introduction (Moon 5+) ──
            AddLine("korath_intro", "korath_intro_01", "Korath",
                "You hear me because you have reached 528. The frequency of transformation. I have waited 1,296 years for this resonance.", true);
            AddLine("korath_intro", "korath_intro_02", "Korath",
                "I am Korath, keeper of the Harmonic Archives. What you call 'ruins' — I call sleeping instruments. You are learning to play them.");

            // ── Korath: Mystical guidance ──
            AddLine("korath_teaching", "korath_teach_01", "Korath",
                "Three-six-nine. The universe speaks in this pattern. Tesla heard it. The builders encoded it. Now you must feel it.");
            AddLine("korath_teaching", "korath_teach_02", "Korath",
                "The golden ratio is not mathematics — it is the shape of consciousness. When you tune to phi, reality bends toward harmony.");
            AddLine("korath_teaching", "korath_teach_03", "Korath",
                "Every building you restore adds a voice to the great chorus. When all thirteen zones sing together... the veil lifts.");
            AddLine("korath_teaching", "korath_teach_04", "Korath",
                "Aether is not 'energy.' Aether is the medium. Like water carries waves, Aether carries intention.");

            // ── Korath: Frequency revelations ──
            AddLine("korath_revelation", "korath_rev_01", "Korath",
                "7.83 Hz — the Earth's heartbeat. 432 Hz — the universe's key signature. 528 Hz — the frequency of miracles. You carry all three now.", true);
            AddLine("korath_revelation", "korath_rev_02", "Korath",
                "The star maps in the Grand Dome are not decorative. They are navigation charts. The builders came from... elsewhere.");
            AddLine("korath_revelation", "korath_rev_03", "Korath",
                "What was buried was not a civilization. It was a technology. A technology of consciousness itself.");

            // ── Korath: Day Out of Time ──
            AddLine("day_out_of_time", "korath_dot_01", "Korath",
                "The Day Out of Time approaches. When all frequencies align. The thirteenth Moon of the galactic calendar... the day between years.", true);
            AddLine("day_out_of_time", "korath_dot_02", "Korath",
                "On that day, the veil between buried and revealed becomes... thin. What was hidden for centuries can be seen with eyes unclouded.");
            AddLine("day_out_of_time", "korath_dot_03", "Korath",
                "Prepare yourself. The Day Out of Time is both gift and trial. Those who resonate at 1,296 Hz will witness the true history.");

            // ── Korath: Combat (reluctant) ──
            AddLine("combat_start", "korath_cbt_01", "Korath",
                "These creatures are pain made manifest. Do not hate them — retune them. They were harmony once.");
            AddLine("combat_victory", "korath_cbtvic_01", "Korath",
                "Every purification heals a fracture in the great pattern. You are mending what should never have been broken.");

            // ── Korath: Idle / exploration ──
            AddLine("exploration_idle", "korath_idle_01", "Korath",
                "Listen... *eyes close* ...the subsonic hum of this zone is shifting. The buildings are waking faster than I expected.");
            AddLine("exploration_idle", "korath_idle_02", "Korath",
                "In the old language, 'Tartaria' means 'the resonant land.' Not a people. Not a place. A state of being.");
            AddLine("exploration_idle", "korath_idle_03", "Korath",
                "The rose windows were not for light. They were frequency lenses. Each petal tuned to a different harmonic.");

            // ── Korath: Zone/threshold events ──
            AddLine("aether_wake", "korath_aw_01", "Korath",
                "The Aether quickens! The old conduits remember their purpose. Soon the entire network will light.");
            AddLine("zone_complete", "korath_zc_01", "Korath",
                "Another voice joins the chorus. *deep breath* Can you hear it? The harmony of the spheres grows louder.");
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
