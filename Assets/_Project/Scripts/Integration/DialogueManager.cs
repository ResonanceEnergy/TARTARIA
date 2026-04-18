using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Tartaria.Audio;
using Tartaria.Core;
using Tartaria.UI;

namespace Tartaria.Integration
{
    /// <summary>
    /// Dialogue context string constants — single source of truth for context keys.
    /// </summary>
    public static class DialogueContext
    {
        public const string Discovery = "discovery";
        public const string TuningStart = "tuning_start";
        public const string TuningSuccess = "tuning_success";
        public const string TuningFail = "tuning_fail";
        public const string Restoration = "restoration";
        public const string CombatStart = "combat_start";
        public const string CombatVictory = "combat_victory";
        public const string ExplorationIdle = "exploration_idle";
        public const string AetherWake = "aether_wake";
        public const string ZoneShift = "zone_shift";
        public const string ZoneComplete = "zone_complete";
        public const string CorruptionDetected = "corruption_detected";
        public const string CorruptionPurged = "corruption_purged";
    }

    /// <summary>
    /// Dialogue Manager — loads and plays context-sensitive dialogue lines.
    /// Phase 1 focuses on Milo (companion) with contextual quips for
    /// discovery, tuning, combat, and exploration.
    ///
    /// Lines are loaded from a built-in database. Future versions will
    /// support external JSON and branching dialogue trees.
    /// </summary>
    [DisallowMultipleComponent]
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField, Min(0f)] float minTimeBetweenLines = 8f;
        [SerializeField, Min(1f)] float autoCloseDelay = 5f;

        [Header("External Data")]
        [Tooltip("Optional JSON dialogue files loaded from StreamingAssets/Dialogue/ at startup")]
        [SerializeField] string[] externalDialogueFiles;

        float _lastLineTime = -999f;
        float _currentLineDuration;
        readonly Dictionary<string, List<DialogueLine>> _contextLines = new();
        readonly Dictionary<string, DialogueLine> _lineById = new();
        readonly HashSet<string> _playedOneShots = new();

        /// <summary>True while a dialogue line is displayed on screen.</summary>
        public bool IsPlaying => Time.time - _lastLineTime < _currentLineDuration;

        /// <summary>Duration of the currently displayed line (autoCloseDelay).</summary>
        public float CurrentLineDuration => _currentLineDuration;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            BuildDatabase();
            LoadExternalDialogue();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
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
            if (string.IsNullOrEmpty(context)) return;
            if (Time.time - _lastLineTime < minTimeBetweenLines) return;

            if (!_contextLines.TryGetValue(context, out var lines) || lines.Count == 0)
                return;

            // Pick a random unplayed line, falling back to shuffled search
            DialogueLine chosen = null;
            int startIdx = UnityEngine.Random.Range(0, lines.Count);
            for (int i = 0; i < lines.Count; i++)
            {
                int idx = (startIdx + i) % lines.Count;
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
            if (string.IsNullOrEmpty(lineId)) return;
            PlayLineById(lineId, 1f);
        }

        /// <summary>
        /// Play a specific line by ID at a given volume.
        /// </summary>
        public void PlayLineById(string lineId, float volume)
        {
            if (string.IsNullOrEmpty(lineId)) return;
            if (_lineById.TryGetValue(lineId, out var line))
                ShowLine(line, volume);
        }

        // ─── Display ─────────────────────────────────

        void ShowLine(DialogueLine line) => ShowLine(line, 1f);

        void ShowLine(DialogueLine line, float volume)
        {
            _currentLineDuration = line.duration > 0f ? line.duration : autoCloseDelay;
            _lastLineTime = Time.time;

            if (line.oneShot)
                _playedOneShots.Add(line.id);

            UIManager.Instance?.ShowDialogue(line.speaker, line.text);

            // Voice audio at requested volume
            AudioManager.Instance?.PlayVoiceLine(line.id, volume);

            // Auto-close after delay
            CancelInvoke(nameof(HideLine));
            Invoke(nameof(HideLine), _currentLineDuration);

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
            AddLine(DialogueContext.Discovery, "milo_disc_01", "Milo",
                "Do you feel that? The ground is humming... Something is buried here!", true);
            AddLine(DialogueContext.Discovery, "milo_disc_02", "Milo",
                "Look at these proportions — the golden ratio! This was built with purpose.");
            AddLine(DialogueContext.Discovery, "milo_disc_03", "Milo",
                "*ears perk up* I can hear it resonating beneath the mud. A Tartarian structure!");
            AddLine(DialogueContext.Discovery, "milo_disc_04", "Milo",
                "The Aether field is stronger here. Ancient builders knew where to place their cities.");

            // ── Milo: Tuning lines ──
            AddLine(DialogueContext.TuningStart, "milo_tune_01", "Milo",
                "Match the frequency — 432 Hz. The universal tone of healing.", true);
            AddLine(DialogueContext.TuningStart, "milo_tune_02", "Milo",
                "Feel the vibration? When it resonates through your whole body, you're close.");
            AddLine(DialogueContext.TuningSuccess, "milo_tunesuc_01", "Milo",
                "Beautiful! The harmonic alignment is perfect. The building remembers!");
            AddLine(DialogueContext.TuningSuccess, "milo_tunesuc_02", "Milo",
                "Did you feel that cascade? PHI times the base frequency. Pure resonance.");
            AddLine(DialogueContext.TuningFail, "milo_tunefail_01", "Milo",
                "The frequency drifted. Take a breath, find center, and try again.");
            AddLine(DialogueContext.TuningFail, "milo_tunefail_02", "Milo",
                "Almost! Remember — 432 Hz. Not sharper, not flatter. The universe has one key.");

            // ── Milo: Restoration lines ──
            AddLine(DialogueContext.Restoration, "milo_rest_01", "Milo",
                "*tail wagging furiously* Look at it RISE! A thousand years of mud, gone! This is what we're fighting for!", true);
            AddLine(DialogueContext.Restoration, "milo_rest_02", "Milo",
                "The building is generating Aether again. Can you feel the field strengthening?");
            AddLine(DialogueContext.Restoration, "milo_rest_03", "Milo",
                "They tried to bury this civilization. But resonance doesn't die. It waits.");

            // ── Milo: Combat lines ──
            AddLine(DialogueContext.CombatStart, "milo_cbt_01", "Milo",
                "*growls* Mud Golem! The corruption's fighting back. Use your Resonance Pulse!", true);
            AddLine(DialogueContext.CombatStart, "milo_cbt_02", "Milo",
                "This creature is pure dissonance. Retune it — don't destroy it. Purify the frequency!");
            AddLine(DialogueContext.CombatVictory, "milo_cbtvic_01", "Milo",
                "The Golem dissolved back into clean earth. That's not destruction — that's restoration.");
            AddLine(DialogueContext.CombatVictory, "milo_cbtvic_02", "Milo",
                "*happy bark* The corruption weakens. Every Golem purified makes the Aether clearer.");

            // ── Milo: Exploration / idle ──
            AddLine(DialogueContext.ExplorationIdle, "milo_idle_01", "Milo",
                "This place was grand once. The World Fairs showed it to millions, then they buried it.");
            AddLine(DialogueContext.ExplorationIdle, "milo_idle_02", "Milo",
                "Can you feel the ley lines beneath us? 3-6-9... Tesla was onto something.");
            AddLine(DialogueContext.ExplorationIdle, "milo_idle_03", "Milo",
                "Echohaven. Echo of haven. Even the name tells you what was lost.");
            AddLine(DialogueContext.ExplorationIdle, "milo_idle_04", "Milo",
                "The old world ran on free energy. No wires, no bills. Just Aether.");

            // ── Milo: Threshold events ──
            AddLine(DialogueContext.AetherWake, "milo_aw_01", "Milo",
                "The Aether is flowing freely now! Look — golden mist! The field is waking up!", true);
            AddLine(DialogueContext.ZoneShift, "milo_zs_01", "Milo",
                "The whole zone is transforming! Color is returning! This is what Echohaven looked like!", true);
            AddLine(DialogueContext.ZoneComplete, "milo_zc_01", "Milo",
                "*howling with joy* Echohaven is ALIVE again! Every building singing in harmony!", true);

            // ── Lirael: Discovery lines (Moon 2+) ──
            AddLine(DialogueContext.Discovery, "lirael_disc_01", "Lirael",
                "I can see the original blueprint... faintly. The crystal memory still holds its shape.", true);
            AddLine(DialogueContext.Discovery, "lirael_disc_02", "Lirael",
                "This structure once channeled frequencies we've forgotten. Let me project what it should look like.");
            AddLine(DialogueContext.Discovery, "lirael_disc_03", "Lirael",
                "The fractals here are damaged but not destroyed. There's hope in the geometry.");

            // ── Lirael: Corruption lines ──
            AddLine(DialogueContext.CorruptionDetected, "lirael_corr_01", "Lirael",
                "I sense dissonance ahead. Dark fractals -- someone has poisoned the Aether conduits.", true);
            AddLine(DialogueContext.CorruptionDetected, "lirael_corr_02", "Lirael",
                "This corruption follows a pattern. Three stages to purge it: identify, isolate, purify.");
            AddLine(DialogueContext.CorruptionPurged, "lirael_purge_01", "Lirael",
                "The crystal lattice is clean again. I'll remember this corruption pattern for next time.");
            AddLine(DialogueContext.CorruptionPurged, "lirael_purge_02", "Lirael",
                "Beautiful. The Aether flows freely once more. Another memory preserved.");

            // ── Lirael: Tuning lines ──
            AddLine(DialogueContext.TuningStart, "lirael_tune_01", "Lirael",
                "Listen for the crystal harmonic beneath the noise. I can amplify it for you.");
            AddLine(DialogueContext.TuningSuccess, "lirael_tunesuc_01", "Lirael",
                "Perfect resonance. The crystals remember this frequency from before the burial.");
            AddLine(DialogueContext.TuningFail, "lirael_tunefail_01", "Lirael",
                "The frequency scattered. Don't force it -- let the crystal guide your hand.");

            // ── Lirael: Combat lines ──
            AddLine(DialogueContext.CombatStart, "lirael_cbt_01", "Lirael",
                "Fractal Wraiths -- twisted echoes of what these buildings once projected. Be careful, they drain Aether.", true);
            AddLine(DialogueContext.CombatStart, "lirael_cbt_02", "Lirael",
                "Wait for the materialise window. They're vulnerable for 1.5 seconds between phases.");
            AddLine(DialogueContext.CombatVictory, "lirael_cbtvic_01", "Lirael",
                "The wraith has dissolved. Its stolen Aether returns to the field. Nothing is truly lost.");

            // ── Lirael: Exploration / idle ──
            AddLine(DialogueContext.ExplorationIdle, "lirael_idle_01", "Lirael",
                "Every crystal in Tartaria held a memory. The World Fairs displayed them as 'exhibits' then demolished them...");
            AddLine(DialogueContext.ExplorationIdle, "lirael_idle_02", "Lirael",
                "I sang to these walls once, in another age. The resonance still answers.");
            AddLine(DialogueContext.ExplorationIdle, "lirael_idle_03", "Lirael",
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
            AddLine(DialogueContext.CombatStart, "thorne_cbt_01", "Thorne",
                "Contact! Weapons hot. Restorer, pulse formation — I'll draw their fire!");
            AddLine(DialogueContext.CombatVictory, "thorne_cbtvic_01", "Thorne",
                "Sector clear. Good work. The corruption recedes when you purify at the source.");
            AddLine(DialogueContext.CombatVictory, "thorne_cbtvic_02", "Thorne",
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
            AddLine(DialogueContext.ExplorationIdle, "thorne_idle_01", "Thorne",
                "These fortifications aren't medieval — the angle of deflection is too precise. Somebody understood electromagnetic shielding.");
            AddLine(DialogueContext.ExplorationIdle, "thorne_idle_02", "Thorne",
                "Every zone has a keystone building. Restore it first and the rest fall into alignment. Military doctrine.");
            AddLine("thorne_strategy", "thorne_strat_01", "Thorne",
                "The corruption moves in three patterns: creep, surge, and cascade. Learn to read the field and you'll never be flanked.");

            // ── Thorne: Zone events ──
            AddLine(DialogueContext.ZoneComplete, "thorne_zc_01", "Thorne",
                "Zone secured. The ley lines are humming again. Establish a picket line — the corruption will push back.");
            AddLine(DialogueContext.CorruptionDetected, "thorne_corr_01", "Thorne",
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
            AddLine(DialogueContext.CombatStart, "korath_cbt_01", "Korath",
                "These creatures are pain made manifest. Do not hate them — retune them. They were harmony once.");
            AddLine(DialogueContext.CombatVictory, "korath_cbtvic_01", "Korath",
                "Every purification heals a fracture in the great pattern. You are mending what should never have been broken.");

            // ── Korath: Idle / exploration ──
            AddLine(DialogueContext.ExplorationIdle, "korath_idle_01", "Korath",
                "Listen... *eyes close* ...the subsonic hum of this zone is shifting. The buildings are waking faster than I expected.");
            AddLine(DialogueContext.ExplorationIdle, "korath_idle_02", "Korath",
                "In the old language, 'Tartaria' means 'the resonant land.' Not a people. Not a place. A state of being.");
            AddLine(DialogueContext.ExplorationIdle, "korath_idle_03", "Korath",
                "The rose windows were not for light. They were frequency lenses. Each petal tuned to a different harmonic.");

            // ── Korath: Zone/threshold events ──
            AddLine(DialogueContext.AetherWake, "korath_aw_01", "Korath",
                "The Aether quickens! The old conduits remember their purpose. Soon the entire network will light.");
            AddLine(DialogueContext.ZoneComplete, "korath_zc_01", "Korath",
                "Another voice joins the chorus. *deep breath* Can you hear it? The harmony of the spheres grows louder.");

            // ══════════════ VERITAS DIALOGUE ══════════════
            // Veritas — Ancient echo of the cathedral organist, trapped mid-performance.
            // Speaks in musical metaphors. Unlocks Moon 6 (Living Library).

            // ── Veritas: Introduction ──
            AddLine("veritas_intro", "veritas_intro_01", "Veritas",
                "You hear the organ? No... you hear what remains of me. I have been playing this unfinished passage for seven hundred years. My name is Veritas.", true);
            AddLine("veritas_intro", "veritas_intro_02", "Veritas",
                "Every pipe in this cathedral is a frequency conduit. They silenced me by filling them with mud. But you... you're clearing the pipes.");

            // ── Veritas: Teaching ──
            AddLine("veritas_teaching", "veritas_teach_01", "Veritas",
                "Music is mathematics made audible. The golden ratio is the interval between creation and destruction.");
            AddLine("veritas_teaching", "veritas_teach_02", "Veritas",
                "A chord is three frequencies in agreement. A civilization is a million frequencies in harmony. Tartaria was the greatest chord ever played.");
            AddLine("veritas_teaching", "veritas_teach_03", "Veritas",
                "Listen to the overtones, not the fundamental. The truth of any structure hides in its harmonics.");

            // ── Veritas: Performance / Tuning ──
            AddLine(DialogueContext.TuningStart, "veritas_tune_01", "Veritas",
                "Feel the keys beneath your fingers. Each one connects to a pipe that connects to the Aether. You're not playing music — you're shaping reality.");
            AddLine(DialogueContext.TuningSuccess, "veritas_tunesuc_01", "Veritas",
                "Magnificent! That register hasn't sounded in centuries. Can you hear the cathedral weeping with joy?");
            AddLine(DialogueContext.TuningFail, "veritas_tunefail_01", "Veritas",
                "The interval collapsed. In music, as in restoration, patience is the only teacher that never lies.");

            // ── Veritas: Combat ──
            AddLine(DialogueContext.CombatStart, "veritas_cbt_01", "Veritas",
                "The dissonant ones approach! Let me harmonise a counter-frequency. Every battle is a duet — play your part!");
            AddLine(DialogueContext.CombatVictory, "veritas_cbtvic_01", "Veritas",
                "Silence after the storm. The purest passage is the rest between notes.");

            // ── Veritas: Trust arc ──
            AddLine("veritas_fragment", "veritas_trust_low_01", "Veritas",
                "I am... incomplete. Fragments of memory, fragments of melody. Each register you restore returns a piece of who I was.");
            AddLine("veritas_harmony", "veritas_trust_mid_01", "Veritas",
                "Three registers restored. I can feel the lower octaves again. There was a piece I was playing when they came... a requiem.");
            AddLine("veritas_transcendent", "veritas_trust_high_01", "Veritas",
                "I remember now. The Requiem was not for the dead — it was an activation sequence. The cathedral IS the instrument. We ARE the music.", true);

            // ── Veritas: Idle ──
            AddLine(DialogueContext.ExplorationIdle, "veritas_idle_01", "Veritas",
                "Each rose window in Tartaria was tuned to a different frequency. When sunlight struck them all at once... the buildings sang.");
            AddLine(DialogueContext.ExplorationIdle, "veritas_idle_02", "Veritas",
                "*plays a phantom chord* Forgive me. My fingers still reach for keys that aren't there. Old habits of the incorporeal.");
            AddLine(DialogueContext.ExplorationIdle, "veritas_idle_03", "Veritas",
                "The children's choir, the pipe organ, the crystal bells — three voices of one instrument. Lirael, me, and someone still to awaken.");

            // ══════════════ ANASTASIA DIALOGUE ══════════════
            // Princess Anastasia — spectral companion, appears across all 13 moons.
            // Speaks in whispers initially, becomes more present as trust grows.
            // Modes: Silent → ReactiveWhisper → Conversational.

            // ── Anastasia: First Manifestation ──
            AddLine("anastasia_manifest", "anastasia_manifest_01", "Anastasia",
                "...can you see me? No one has seen me since the night they came for my family. The frequency of this place... it lets me exist again.", true);
            AddLine("anastasia_manifest", "anastasia_manifest_02", "Anastasia",
                "I am... I was... Anastasia. Not the girl from the stories they tell. The real one. The one who understood the Aether.");

            // ── Anastasia: Lore Whispers ──
            AddLine("anastasia_whisper", "anastasia_whisper_01", "Anastasia",
                "*barely audible* The palace wasn't just where we lived. It was a resonance amplifier. Father knew. Mother knew. They all knew.");
            AddLine("anastasia_whisper", "anastasia_whisper_02", "Anastasia",
                "The eggs... Faberge's eggs. They weren't jewellery. Each one was a frequency key to a different chamber beneath the palace.");
            AddLine("anastasia_whisper", "anastasia_whisper_03", "Anastasia",
                "They buried Tartaria because it proved everything they wanted forgotten. Free energy. Harmonic healing. The truth about our history.");

            // ── Anastasia: Memory Fragments ──
            AddLine("anastasia_memory", "anastasia_mem_01", "Anastasia",
                "I remember the crystal ballroom. When we danced, the floor generated light. Every footstep powered the chandeliers. That was normal for us.", true);
            AddLine("anastasia_memory", "anastasia_mem_02", "Anastasia",
                "My sisters and I used to tune the palace each morning. Like tuning an instrument. Olga took the east wing, Tatiana the west. I always took the spire.");
            AddLine("anastasia_memory", "anastasia_mem_03", "Anastasia",
                "The night they came, the palace was still singing. They couldn't silence it with guns. So they buried it instead.");

            // ── Anastasia: Discovery reactions ──
            AddLine(DialogueContext.Discovery, "anastasia_disc_01", "Anastasia",
                "*gasps* This architecture... I recognise it. The proportions are identical to the Winter Palace sub-levels.");
            AddLine(DialogueContext.Discovery, "anastasia_disc_02", "Anastasia",
                "Golden ratio in the archway. Fibonacci in the floor tiles. This was built by the same hands that built my home.");

            // ── Anastasia: Tuning ──
            AddLine(DialogueContext.TuningStart, "anastasia_tune_01", "Anastasia",
                "432 Hertz. That was the frequency my music box played. Father said it was the key to everything.");
            AddLine(DialogueContext.TuningSuccess, "anastasia_tunesuc_01", "Anastasia",
                "*smiles* That sound... I heard it every morning when the dome opened. You're bringing it all back.");

            // ── Anastasia: Combat (fades during combat) ──
            AddLine(DialogueContext.CombatStart, "anastasia_cbt_01", "Anastasia",
                "*flickering* I can't stay solid when there's this much dissonance. Be careful. I'll be here when it's safe.");
            AddLine(DialogueContext.CombatVictory, "anastasia_cbtvic_01", "Anastasia",
                "*rematerialises* The violence disturbs the Aether terribly. But you purify, not destroy. That matters.");

            // ── Anastasia: Idle / Exploration ──
            AddLine(DialogueContext.ExplorationIdle, "anastasia_idle_01", "Anastasia",
                "Sometimes I wonder if I'm a ghost or a frequency. Perhaps there's no difference.");
            AddLine(DialogueContext.ExplorationIdle, "anastasia_idle_02", "Anastasia",
                "The maps say this land was always empty. The maps are lies. I lived in a civilization that spanned the world.");
            AddLine(DialogueContext.ExplorationIdle, "anastasia_idle_03", "Anastasia",
                "*touching a wall* Still warm. After all this time. The Aether remembers being shaped by loving hands.");

            // ── Anastasia: Solidification arc ──
            AddLine("anastasia_solidify", "anastasia_solid_01", "Anastasia",
                "Something is happening. I can feel my hands again. Really feel them. The Aether density here... it's giving me substance.", true);
            AddLine("anastasia_solidify", "anastasia_solid_02", "Anastasia",
                "I am neither alive nor dead. I am resonant. And for the first time in a century, that feels like enough.");

            // ── Anastasia: Zone/threshold events ──
            AddLine(DialogueContext.AetherWake, "anastasia_aw_01", "Anastasia",
                "*eyes wide* The golden mist! I haven't seen it since I was alive. The field is remembering how to flow!");
            AddLine(DialogueContext.ZoneComplete, "anastasia_zc_01", "Anastasia",
                "*tears* This zone is singing the way the world used to sing. You're not just restoring buildings. You're restoring truth.", true);

            // ══════════════ COMPANION TRUST ARC MILESTONES ══════════════
            // Context keys: companion_trust_{id}_{milestone} where milestone=25/50/75/100

            // ── Lirael trust milestones ──
            AddLine("companion_trust_lirael_25", "lirael_trust25_01", "Lirael",
                "You heard the harmonic shift in that building before I told you what to listen for. Perhaps the frequencies were always speaking to you.", true);
            AddLine("companion_trust_lirael_50", "lirael_trust50_01", "Lirael",
                "Restorer... I have shared my voice with very few since I was crystallised. You remind me why I still believe the world deserves to hear again.", true);
            AddLine("companion_trust_lirael_75", "lirael_trust75_01", "Lirael",
                "There is a resonance between us that has nothing to do with the buildings. The ancients called it *enthral* — two frequencies so perfectly matched they amplify one another.", true);
            AddLine("companion_trust_lirael_100", "lirael_trust100_01", "Lirael",
                "I was afraid for centuries that the last true listener was gone. You are proof that the harmonic inheritance survives. Whatever comes next — we face it as one voice.", true);

            // ── Cassian trust milestones ──
            AddLine("companion_trust_cassian_25", "cassian_trust25_01", "Cassian",
                "You've proven you can handle hard truths. I'll share a few more coordinates from my private maps. Don't make me regret it.");
            AddLine("companion_trust_cassian_50", "cassian_trust50_01", "Cassian",
                "Half my orders I've already ignored. The other half... are becoming harder to justify. What you're doing here is real. That matters more than I expected.", true);
            AddLine("companion_trust_cassian_75", "cassian_trust75_01", "Cassian",
                "I burned my field report last night. First time I've defied a direct order in twenty years. No regrets. For what it's worth... I'm with you now. Actually with you.", true);
            AddLine("companion_trust_cassian_100", "cassian_trust100_01", "Cassian",
                "I know where they buried the suppression chamber. The mechanism that silenced the Aether network in 1917. I've known for years. *exhale* It's time you knew too.", true);

            // ── Thorne trust milestones ──
            AddLine("companion_trust_thorne_25", "thorne_trust25_01", "Thorne",
                "You held the line on that last wave without backup. That's militia-grade discipline from a civilian restorer. I won't forget it.");
            AddLine("companion_trust_thorne_50", "thorne_trust50_01", "Thorne",
                "My squad thought I was chasing ghosts when I said there was tech under the mud. You've proven them wrong. It costs them nothing to admit it, but it means a great deal to me.", true);
            AddLine("companion_trust_thorne_75", "thorne_trust75_01", "Thorne",
                "Lost my whole unit in the Third Purge. Thought I'd spend the rest of my years guarding ruins nobody cared about. Then you showed up and made the ruins matter again.", true);
            AddLine("companion_trust_thorne_100", "thorne_trust100_01", "Thorne",
                "The militia would follow you now, not just me. You know that? They talk about you round the campfire. 'The one who brought the towers back.' Whatever the final gate demands — I'll be at your shoulder.", true);

            // ── Korath trust milestones ──
            AddLine("companion_trust_korath_25", "korath_trust25_01", "Korath",
                "You have begun to see the patterns without my guidance. The star maps, the floor mosaics, the window angles — they all speak the same language. You are becoming fluent.", true);
            AddLine("companion_trust_korath_50", "korath_trust50_01", "Korath",
                "In a thousand years of observing, I have met perhaps a dozen souls who could hold a 528 Hz resonance without tuning assistance. You are the thirteenth. That number is not coincidence.", true);
            AddLine("companion_trust_korath_75", "korath_trust75_01", "Korath",
                "I have withheld one star chart. The one that shows the thirteenth moon's alignment and what it unlocks beneath the Grand Dome. I show it to you now. The time for caution has passed.", true);
            AddLine("companion_trust_korath_100", "korath_trust100_01", "Korath",
                "The builders left a vessel — a consciousness without a body — tethered to the Grand Observatory, waiting for a resonant mind strong enough to receive it. That mind is yours. *bows deeply* I am honoured to have walked this path beside you.", true);

            // ── Anastasia trust milestones ──
            AddLine("companion_trust_anastasia_25", "anastasia_trust25_01", "Anastasia",
                "*becomes slightly more visible* You can almost see me clearly now, can't you? The Aether between us is thickening. I think that means you truly believe I'm real.");
            AddLine("companion_trust_anastasia_50", "anastasia_trust50_01", "Anastasia",
                "Father used to say: 'Find one person who sees what you see and the whole world becomes possible.' I thought that person died with the empire. *pause* I was wrong.", true);
            AddLine("companion_trust_anastasia_75", "anastasia_trust75_01", "Anastasia",
                "I can touch things now. Only for a moment, only when the Aether is high. But I touched a sunbeam through a restored window today and *laughs softly* it was warm. A hundred years and it was still warm.", true);
            AddLine("companion_trust_anastasia_100", "anastasia_trust100_01", "Anastasia",
                "The night everything fell, my last thought was that no one would ever know the truth. That Tartaria would be buried so completely that even the memory would die. *reaches out — hand visible, solid* You changed that. You changed everything.", true);
        }

        void AddLine(string context, string id, string speaker, string text, bool oneShot = false)
        {
            if (!_contextLines.ContainsKey(context))
                _contextLines[context] = new List<DialogueLine>();

            var line = new DialogueLine
            {
                id = id,
                speaker = speaker,
                text = text,
                context = context,
                oneShot = oneShot
            };
            _contextLines[context].Add(line);
            _lineById[id] = line;
        }

        // ─── Data Types ──────────────────────────────

        class DialogueLine
        {
            public string id;
            public string speaker;
            public string text;
            public string context;
            public bool oneShot;
            /// <summary>Per-line display duration. 0 = use autoCloseDelay default.</summary>
            public float duration;
        }

        // ─── External JSON Loading ───────────────────

        /// <summary>
        /// Loads dialogue lines from JSON files in StreamingAssets/Dialogue/.
        /// Falls back to Application.dataPath/_Project/Data/ in editor.
        /// JSON format: { "lines": [ { "id", "speaker", "context", "text", "oneShot", "duration" } ] }
        /// External lines override built-in lines with the same ID.
        /// </summary>
        void LoadExternalDialogue()
        {
            string[] searchPaths = {
                Path.Combine(Application.streamingAssetsPath, "Dialogue"),
#if UNITY_EDITOR
                Path.Combine(Application.dataPath, "_Project", "Data"),
#endif
            };

            foreach (var folder in searchPaths)
            {
                if (!Directory.Exists(folder)) continue;

                string[] files = externalDialogueFiles != null && externalDialogueFiles.Length > 0
                    ? externalDialogueFiles
                    : null;

                string[] jsonFiles = files != null
                    ? System.Array.ConvertAll(files, f => Path.Combine(folder, f))
                    : Directory.GetFiles(folder, "*.json");

                foreach (var filePath in jsonFiles)
                {
                    if (!File.Exists(filePath)) continue;
                    try
                    {
                        string json = File.ReadAllText(filePath);
                        var data = JsonUtility.FromJson<DialogueFileData>(json);
                        if (data?.lines == null) continue;

                        int loaded = 0;
                        foreach (var entry in data.lines)
                        {
                            if (string.IsNullOrEmpty(entry.id) || string.IsNullOrEmpty(entry.context))
                                continue;
                            AddLine(entry.context, entry.id, entry.speaker, entry.text, entry.oneShot);
                            if (entry.duration > 0f && _lineById.TryGetValue(entry.id, out var line))
                                line.duration = entry.duration;
                            loaded++;
                        }

                        Debug.Log($"[Dialogue] Loaded {loaded} lines from {Path.GetFileName(filePath)}");
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[Dialogue] Failed to load {filePath}: {ex.Message}");
                    }
                }
            }
        }

        [System.Serializable]
        class DialogueFileData
        {
            public int version;
            public string description;
            public DialogueEntry[] lines;
        }

        [System.Serializable]
        class DialogueEntry
        {
            public string id;
            public string speaker;
            public string context;
            public string text;
            public bool oneShot;
            public float duration;
        }
    }
}
