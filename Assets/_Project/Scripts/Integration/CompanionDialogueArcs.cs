using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Companion Dialogue Arcs — gates companion dialogue availability by Moon
    /// and tracks relationship progression. Production R7 full living system.
    /// 7 companions complete: Lirael, Thorne, Korath, Veritas, Milo, Anastasia, Cassian.
    /// R7: Expanded for trust arcs 1-3, physical tells, calendar/live-ops, giant synergies, cross-Moon memory, VO real playback.
    /// Moon 2 Companion Stories & Reactivity: Cathedral/corruption/crystal arcs for Lirael, Korath, Cassian, Anastasia with physical tells + permanent mutations.
    /// </summary>
    [DisallowMultipleComponent]
    public class CompanionDialogueArcs : MonoBehaviour
    {
        public static CompanionDialogueArcs Instance { get; private set; }

        public enum CompanionId
        {
            Lirael, Thorne, Korath, Veritas, Milo, Anastasia, Cassian
        }

        public enum TrustLevel { Stranger, Acquaintance, Ally, Confidant, Bonded }

        [System.Serializable]
        public struct DialogueNode
        {
            public CompanionId companion;
            public int moonGate;
            public string dialogueKey;
            public TrustLevel trustRequired;
            public bool requiresWorldChoice;
            public WorldChoiceTracker.WorldChoiceId worldChoiceId;
            public WorldChoiceTracker.ChoiceOption worldChoiceRequired;

            // R6/R7 voice authoring + real playback
            public string VoiceDirection;
            public float VOIntensity;
        }

        // ═══ MASSIVE R7 PRODUCTION DIALOGUE DATABASE (expanded from R6 + full 7-comp bible depth) ═══
        // Dozens of new moments for trust arcs (Moons 1-3), physical reactivity (all beats), calendar, giant, memory, Veritas full arc.
        // Moon 2 additions: Cathedral corruption, crystal nodes, purge physical tells, Korath echo foreshadow, Anastasia Archive warmth.
        static readonly DialogueNode[] Catalogue = new[]
        {
            // Lirael arc (expanded R7 Moons 1-3 trust + physical + giant + calendar)
            Node(CompanionId.Lirael, 1, "LIRAEL_INTRO", TrustLevel.Stranger),
            Node(CompanionId.Lirael, 1, "LIRAEL_MOON1_RESTORATION_TELL", TrustLevel.Stranger, "crystal shimmer on first dome restore, gentle wonder", 0.82f),
            Node(CompanionId.Lirael, 2, "LIRAEL_CRYSTAL_SONG", TrustLevel.Acquaintance),
            Node(CompanionId.Lirael, 2, "LIRAEL_MOON2_COMBAT_HIDE_REACT", TrustLevel.Acquaintance, "fractured projection during combat, post-victory warmth", 0.71f),

            // === NEW MOON 2 CATHEDRAL / CRYSTAL / CORRUPTION ARCS (Lirael Crystal Choir physical tells) ===
            Node(CompanionId.Lirael, 2, "LIRAEL_CATHEDRAL_CRYSTAL_SONG", TrustLevel.Acquaintance, "fractured projection near corruption veins in cathedral, singing the pre-Flood cathedral song, physical tell intensify + solid glow on each successful node purge", 0.88f),
            Node(CompanionId.Lirael, 2, "LIRAEL_CRYSTAL_NODE_PURGE_1", TrustLevel.Acquaintance, "first cathedral crystal node: 'The stones were singing. The corruption tried to make them scream.' Physical fracture tell + R7 ApplyPhysicalTellForBeat restore", 0.85f),
            Node(CompanionId.Lirael, 2, "LIRAEL_CRYSTAL_NODE_PURGE_2", TrustLevel.Acquaintance, "second node: memory of giant builders placing the heart crystal. Projection solidifies, hums in harmony with player tuning", 0.87f),
            Node(CompanionId.Lirael, 2, "LIRAEL_CATHEDRAL_CHOIR_COMPLETE", TrustLevel.Ally, "all 3 nodes: 'The cathedral remembers its own voice now.' Triggers world mutation: permanent pre-corruption holographic overlays + tuning bonus in Moon2+", 0.92f),

            Node(CompanionId.Lirael, 3, "LIRAEL_DOUBT", TrustLevel.Ally),
            Node(CompanionId.Lirael, 3, "LIRAEL_ORPHAN_TRAIN_ESCORT_PHYSICAL", TrustLevel.Ally, "roof position lean, lullaby hum during rail escort", 0.88f),
            Node(CompanionId.Lirael, 3, "LIRAEL_MOON3_17TH_ECHO_CALENDAR", TrustLevel.Ally, "17th Hour spectral wind tell + trust echo", 0.79f),
            Node(CompanionId.Lirael, 5, "LIRAEL_GIANT_SONG_SYNERGY", TrustLevel.Confidant, "giant mode harmonic assist, shared history memory", 0.95f),
            Node(CompanionId.Lirael, 8, "LIRAEL_RESOLVE", TrustLevel.Ally),
            Node(CompanionId.Lirael, 11, "LIRAEL_PROPHECY", TrustLevel.Confidant),
            Node(CompanionId.Lirael, 13, "LIRAEL_FINALE", TrustLevel.Bonded),
            Node(CompanionId.Lirael, 13, "LIRAEL_CROSS_MOON_MEMORY_PAYOFF", TrustLevel.Bonded, "references Moon 1-3 physical tells and giant song", 0.97f),

            // Thorne (R7 deep 1-3 + escort + giant)
            Node(CompanionId.Thorne, 2, "THORNE_INTRO", TrustLevel.Stranger),
            Node(CompanionId.Thorne, 3, "THORNE_MOON3_ESCORT_VIGIL", TrustLevel.Acquaintance, "forward guard lean on train, 17th watch", 0.81f),
            Node(CompanionId.Thorne, 4, "THORNE_STAR_FORT", TrustLevel.Acquaintance),
            Node(CompanionId.Thorne, 4, "THORNE_RESTORATION_CELEBRATE_TELL", TrustLevel.Acquaintance, "gruff salute on fort restore, physical tell", 0.76f),
            Node(CompanionId.Thorne, 6, "THORNE_SACRIFICE", TrustLevel.Ally),
            Node(CompanionId.Thorne, 7, "THORNE_GIANT_SYNERGY", TrustLevel.Ally, "Companion Giant assist + fleet memory", 0.93f),
            Node(CompanionId.Thorne, 9, "THORNE_FLEET", TrustLevel.Ally),
            Node(CompanionId.Thorne, 12, "THORNE_BELL_WAR", TrustLevel.Confidant),
            Node(CompanionId.Thorne, 13, "THORNE_FINALE", TrustLevel.Bonded),

            // Korath (R7 1-3 hooks + star giant + memory) — Moon 2 foreshadow echo in cathedral
            Node(CompanionId.Korath, 3, "KORATH_INTRO", TrustLevel.Stranger),
            Node(CompanionId.Korath, 3, "KORATH_MOON3_WORLD_MUTATION_LORE", TrustLevel.Stranger, "stone remembers your first restoration", 0.74f),

            // === NEW MOON 2 KORATH FORESHADOW: Builder's Shadow / Stone Shadow in Cathedral (early echo, physical stone hum tell) ===
            Node(CompanionId.Korath, 2, "KORATH_CATHEDRAL_ECHO_INSCRIPTION", TrustLevel.Stranger, "giant-scale stone hum + temporary silhouette projection in deepest crystal chamber: 'The song made stone... now it screams. You... small one... you can hear it still?' Triggers early Korath trust seed + R7 physical tell (stone resonance)", 0.91f),
            Node(CompanionId.Korath, 2, "KORATH_CATHEDRAL_SONG_INVERTED", TrustLevel.Stranger, "resonance with inscription: 'They inverted the frequency. The cathedral was meant to sing forever. You are the first to remember the original key.' World mutation foreshadow: permanent +10% crystal/stone integrity in future builds", 0.89f),
            Node(CompanionId.Korath, 2, "KORATH_ECHO_CATHEDRAL_PAYOFF", TrustLevel.Acquaintance, "Lirael reaction to Korath echo: 'He was tall like the dome itself. The stones still know his hands.' Permanent Korath stone memory active in Moon2 cathedral", 0.87f),

            Node(CompanionId.Korath, 5, "KORATH_MAPPING", TrustLevel.Acquaintance),
            Node(CompanionId.Korath, 7, "KORATH_ANASTASIA", TrustLevel.Ally),
            Node(CompanionId.Korath, 7, "KORATH_GIANT_COMPANION_PEAK", TrustLevel.Ally, "true giant scale star gaze during player giant", 0.99f),
            Node(CompanionId.Korath, 8, "KORATH_17TH_HOUR_RAIL_STARS", TrustLevel.Ally),
            Node(CompanionId.Korath, 10, "KORATH_RAIL_STARS", TrustLevel.Ally),
            Node(CompanionId.Korath, 12, "KORATH_CONVERGENCE", TrustLevel.Confidant),
            Node(CompanionId.Korath, 13, "KORATH_FINALE", TrustLevel.Bonded),
            Node(CompanionId.Korath, 13, "KORATH_SHARED_HISTORY_CROSS_MOON", TrustLevel.Bonded, "remembers your small hands from Moon 1", 0.98f),

            // Veritas R7 full arc (Moon 6+ precision, giant song, calendar truth, 17th echoes)
            Node(CompanionId.Veritas, 4, "VERITAS_INTRO", TrustLevel.Stranger),
            Node(CompanionId.Veritas, 6, "VERITAS_FIRST_BELL", TrustLevel.Acquaintance),
            Node(CompanionId.Veritas, 6, "VERITAS_RESTORATION_RESONANCE_TELL", TrustLevel.Acquaintance, "organ pipe hum on first full register", 0.83f),
            Node(CompanionId.Veritas, 6, "VERITAS_17TH_HOUR_ECHO_CALENDAR", TrustLevel.Acquaintance, "truth echo during 17th, state change", 0.79f),
            Node(CompanionId.Veritas, 7, "VERITAS_DISSONANCE", TrustLevel.Ally),
            Node(CompanionId.Veritas, 8, "VERITAS_GIANTS_SONG_AUTO_MATCH", TrustLevel.Ally, "exact freq match with player giant harmonic", 0.96f),
            Node(CompanionId.Veritas, 9, "VERITAS_DISSONANCE", TrustLevel.Ally),
            Node(CompanionId.Veritas, 11, "VERITAS_PURIFY", TrustLevel.Ally),
            Node(CompanionId.Veritas, 11, "VERITAS_MOON11_CROSS_MEMORY", TrustLevel.Confidant, "remembers your first tuning from Moon 1", 0.91f),
            Node(CompanionId.Veritas, 13, "VERITAS_FINALE", TrustLevel.Bonded),
            Node(CompanionId.Veritas, 13, "VERITAS_ETERNAL_RESONANCE_PAYOFF", TrustLevel.Bonded, "finishes the song with all companions", 0.99f),

            // Milo (R7 Moons 1-3 trust + physical + giant comic)
            Node(CompanionId.Milo, 1, "MILO_INTRO", TrustLevel.Stranger),
            Node(CompanionId.Milo, 1, "MILO_FIRST_RESTORATION_SINCERE_TELL", TrustLevel.Stranger, "uncharacteristically quiet awe after dome", 0.85f),
            Node(CompanionId.Milo, 2, "MILO_CURIOSITY", TrustLevel.Acquaintance),
            Node(CompanionId.Milo, 3, "MILO_ORPHAN_WITNESS_PHYSICAL", TrustLevel.Ally, "protective rear escort lean, rare sincere line", 0.79f),
            Node(CompanionId.Milo, 3, "MILO_17TH_DAILY_BANTER", TrustLevel.Ally, "calendar daily deal banter + trust pricing", 0.68f),
            Node(CompanionId.Milo, 5, "MILO_GENUINE_WONDER", TrustLevel.Ally),
            Node(CompanionId.Milo, 5, "MILO_GIANT_MODE_COMIC_SYNERGY", TrustLevel.Ally, "tiny fox trying to look giant, hilarious bond", 0.82f),
            Node(CompanionId.Milo, 9, "MILO_AURORA_OPEN", TrustLevel.Ally),
            Node(CompanionId.Milo, 13, "MILO_FINALE", TrustLevel.Bonded),
            Node(CompanionId.Milo, 13, "MILO_CROSS_MOON_TO_FORGETTING_LESS", TrustLevel.Bonded, "full arc payoff referencing 1-3 physical tells", 0.97f),

            // Cassian redemption expanded R7 — Moon 2 cathedral analysis + physical tells
            Node(CompanionId.Cassian, 5, "CASSIAN_WHITE_CITY", TrustLevel.Acquaintance),
            Node(CompanionId.Cassian, 7, "CASSIAN_BETRAYAL", TrustLevel.Ally),
            Node(CompanionId.Cassian, 7, "CASSIAN_REDEMPTION", TrustLevel.Ally),
            Node(CompanionId.Cassian, 7, "CASSIAN_PHYSICAL_BOND_POST_CHOICE", TrustLevel.Ally, "calm ally lean in PhysicalBond", 0.91f),
            Node(CompanionId.Cassian, 10, "CASSIAN_BOND_ANASTASIA", TrustLevel.Confidant),
            Node(CompanionId.Cassian, 12, "CASSIAN_17TH_HOUR_INTEL", TrustLevel.Confidant),
            Node(CompanionId.Cassian, 13, "CASSIAN_REDEEMED_FINALE", TrustLevel.Bonded),
            Node(CompanionId.Cassian, 13, "CASSIAN_GIANT_SYNERGY_INTEL", TrustLevel.Bonded, "intel during giant song match", 0.89f),

            // === NEW MOON 2 CASSIAN CATHEDRAL ANALYSIS (ambiguous path, physical tells, trust branch) ===
            Node(CompanionId.Cassian, 2, "CASSIAN_CATHEDRAL_ANALYSIS_INTRO", TrustLevel.Acquaintance, "smooth scholar voice in crystal cathedral: 'These veins are sabotage. Let me map the efficient path.' Subtle violet cufflink tell if low trust (R7 dissonance VFX)", 0.78f),
            Node(CompanionId.Cassian, 2, "CASSIAN_CATHEDRAL_TRUE_PATH", TrustLevel.Ally, "if player chooses Lirael insight over Cassian map: 'You... saw through it. Impressive. The truth is uglier than I sold you.' +2 trust, unlocks permanent weakpoint markers", 0.82f),
            Node(CompanionId.Cassian, 2, "CASSIAN_CATHEDRAL_BAD_PATH", TrustLevel.Acquaintance, "if follow his 'optimized' path: harder wraith wave. 'A necessary efficiency.' -1 trust or branch. Physical: stronger dissonance tell on Cassian", 0.75f),
            Node(CompanionId.Cassian, 2, "CASSIAN_CATHEDRAL_REDEMPTION_SEED", TrustLevel.Ally, "post-analysis if high trust: 'The crystals are beautiful when they aren't lying. I... forgot that.' R7 ApplyPhysicalTellForBeat + redemption progress", 0.84f),

            // Anastasia (R7 solidification + giant + memory + 112+ context)
            Node(CompanionId.Anastasia, 7, "ANASTASIA_FIRST_WHISPER", TrustLevel.Stranger),
            Node(CompanionId.Anastasia, 7, "ANASTASIA_MOON7_SOLIDIF_PREP", TrustLevel.Ally, "near-solid tell before DotT", 0.87f),
            Node(CompanionId.Anastasia, 13, "ANASTASIA_SOLIDIFICATION", TrustLevel.Bonded),
            Node(CompanionId.Anastasia, 13, "ANASTASIA_I_CAN_FEEL_THE_GROUND", TrustLevel.Bonded, "10s solidification full voice, real tear/footstep", 1.0f),
            Node(CompanionId.Anastasia, 13, "ANASTASIA_GIANT_WARM_GLOW_SYNERGY", TrustLevel.Bonded, "warmer post-solid gold during giant", 0.96f),
            Node(CompanionId.Anastasia, 13, "ANASTASIA_CROSS_MOON_ALL_112_PAYOFF", TrustLevel.Bonded, "references every major beat whisper + giant song", 0.99f),

            // === NEW MOON 2 ANASTASIA ARCHIVE FACETS (crystal cathedral 17th Hour, motes + warmth, Golden Mote extension) ===
            Node(CompanionId.Anastasia, 2, "ANASTASIA_CATHEDRAL_CRYSTAL_WHISPER", TrustLevel.Stranger, "17th Hour near crystal cluster: 'The Archive was all cold facets. These... these are warm. They remember being sung into place.' Motes orbit veins, physical glow tell", 0.81f),
            Node(CompanionId.Anastasia, 2, "ANASTASIA_CATHEDRAL_MOTE_SHARE", TrustLevel.Acquaintance, "player shares resonance with her motes on crystal: motes leave permanent golden tracery on veins. 'Thank you. I had forgotten what sharing light felt like.' +2 trust", 0.86f),
            Node(CompanionId.Anastasia, 2, "ANASTASIA_CATHEDRAL_ARCHIVE_WARMTH", TrustLevel.Ally, "post-interaction: 'Because of you the cathedral will never be as cold as my prison. The caustics here will stay warmer... always.' Triggers Anastasia world mutation: permanent warmer gold caustics + extra whispers in Moon2 cathedral", 0.90f),

            // R6 17th escort + R7 calendar/giant extensions
            Node(CompanionId.Korath, 8, "KORATH_17TH_HOUR_RAIL_STARS", TrustLevel.Ally),
            Node(CompanionId.Thorne, 8, "THORNE_17TH_HOUR_RAIL_WATCH", TrustLevel.Ally),
            Node(CompanionId.Korath, 12, "KORATH_ESCORT_BOND", TrustLevel.Confidant),
            Node(CompanionId.Thorne, 12, "THORNE_ESCORT_VIGIL", TrustLevel.Confidant),
            Node(CompanionId.Veritas, 13, "VERITAS_CALENDAR_TRUTH_ECHO", TrustLevel.Bonded, "17th echo truth + giant precision", 0.93f),

            // Choice + world gated (expanded)
            ChoiceNode(CompanionId.Thorne, 4, "THORNE_STAR_FORT_A", WorldChoiceTracker.WorldChoiceId.W2_StarFort, WorldChoiceTracker.ChoiceOption.OptionA, TrustLevel.Acquaintance),
            ChoiceNode(CompanionId.Thorne, 4, "THORNE_STAR_FORT_B", WorldChoiceTracker.WorldChoiceId.W2_StarFort, WorldChoiceTracker.ChoiceOption.OptionB, TrustLevel.Acquaintance),
            ChoiceNode(CompanionId.Milo, 9, "MILO_AURORA_OPEN", WorldChoiceTracker.WorldChoiceId.W4_AuroraCity, WorldChoiceTracker.ChoiceOption.OptionA, TrustLevel.Ally),
            ChoiceNode(CompanionId.Milo, 9, "MILO_AURORA_SEALED", WorldChoiceTracker.WorldChoiceId.W4_AuroraCity, WorldChoiceTracker.ChoiceOption.OptionB, TrustLevel.Ally),
        };

        // Runtime state...
        readonly Dictionary<CompanionId, TrustLevel> _trust = new();
        readonly HashSet<string> _seenDialogues = new();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            foreach (CompanionId c in System.Enum.GetValues(typeof(CompanionId)))
                _trust[c] = TrustLevel.Stranger;
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        // ─── Save/Load (R8 persistence) ────────────────
        [System.Serializable]
        public class DialogueArcSaveData
        {
            public int[] companionIds = System.Array.Empty<int>();
            public int[] trustLevels = System.Array.Empty<int>();
            public string[] seenKeys = System.Array.Empty<string>();
        }
        public DialogueArcSaveData GetSaveData()
        {
            var ids = new List<int>();
            var trusts = new List<int>();
            foreach (var kv in _trust) { ids.Add((int)kv.Key); trusts.Add((int)kv.Value); }
            var seen = new string[_seenDialogues.Count];
            _seenDialogues.CopyTo(seen);
            return new DialogueArcSaveData
            {
                companionIds = ids.ToArray(),
                trustLevels = trusts.ToArray(),
                seenKeys = seen
            };
        }
        public void LoadSaveData(DialogueArcSaveData data)
        {
            if (data == null) return;
            _trust.Clear();
            if (data.companionIds != null && data.trustLevels != null)
            {
                int n = Mathf.Min(data.companionIds.Length, data.trustLevels.Length);
                for (int i = 0; i < n; i++)
                    _trust[(CompanionId)data.companionIds[i]] = (TrustLevel)data.trustLevels[i];
            }
            _seenDialogues.Clear();
            if (data.seenKeys != null) foreach (var k in data.seenKeys) _seenDialogues.Add(k);
        }

        public TrustLevel GetTrust(CompanionId companion) => _trust.TryGetValue(companion, out var t) ? t : TrustLevel.Stranger;

        public void IncreaseTrust(CompanionId companion)
        {
            if (!_trust.ContainsKey(companion)) return;
            if (_trust[companion] < TrustLevel.Bonded)
            {
                _trust[companion]++;
                Save.SaveManager.Instance?.MarkDirty();
                Debug.Log($"[DialogueArcs] {companion} trust → {_trust[companion]}");
            }
        }

        public List<DialogueNode> GetAvailableDialogues(CompanionId companion, int currentMoon)
        {
            var result = new List<DialogueNode>();
            foreach (var node in Catalogue)
            {
                if (node.companion != companion) continue;
                if (node.moonGate > currentMoon) continue;
                if (node.trustRequired > GetTrust(companion)) continue;
                if (_seenDialogues.Contains(node.dialogueKey)) continue;
                if (node.requiresWorldChoice)
                {
                    var wc = WorldChoiceTracker.Instance;
                    if (wc == null) continue;
                    var choice = wc.GetChoice(node.worldChoiceId);
                    if (choice != node.worldChoiceRequired) continue;
                }
                result.Add(node);
            }
            return result;
        }

        public void MarkSeen(string dialogueKey)
        {
            _seenDialogues.Add(dialogueKey);
            Save.SaveManager.Instance?.MarkDirty();
        }

        // R7: Trigger solidification + bond + calendar density (extended)
        public void TriggerSolidificationCallback(CompanionId primary, CompanionId bonded = CompanionId.Cassian)
        {
            PlayBondDialogue(primary, "solidification");
            if (bonded != CompanionId.Cassian || primary == CompanionId.Cassian)
                PlayBondDialogue(bonded, "redemption_bond");
            DialogueManager.Instance?.PlayContextDialogue($"17th_hour_{primary.ToString().ToLower()}_density");
            // R7 real VO playback hook for high intensity
            CompanionManager.Instance?.PlayVoiceLineWithIntensity($"solidif_{primary}", 0.98f, "warm intimate solidification whisper, rising harmonic, full release");
            Debug.Log($"[DialogueArcs R7] Solidification + bond + VO playback + calendar for {primary}");
        }

        void PlayBondDialogue(CompanionId c, string context)
        {
            string key = $"companion_bond_{c.ToString().ToLower()}_{context}";
            DialogueManager.Instance?.PlayContextDialogue(key);
            MarkSeen(key);
            // R7 VO trigger
            CompanionManager.Instance?.PlayVoiceLineWithIntensity(key, 0.92f, "intimate bond solidification context");
        }

        // ═══ R7 VO Script + Real Playback Pipeline (hooked) ═══
        public void GenerateVOScriptForRecording(bool includeSeen = false, bool logToConsole = true)
        {
            var lines = new List<string>();
            lines.Add("KEY|COMPANION|MOON|TRUST|VOICE_DIRECTION|INTENSITY|CONTEXT|R7_BEAT");
            foreach (var node in Catalogue)
            {
                if (!includeSeen && _seenDialogues.Contains(node.dialogueKey)) continue;
                if (node.VOIntensity < 0.6f && !node.dialogueKey.Contains("17TH") && !node.dialogueKey.Contains("SOLID") && !node.dialogueKey.Contains("REDEMP") && !node.dialogueKey.Contains("GIANT") && !node.dialogueKey.Contains("CALENDAR")) continue;

                string ctx = (node.dialogueKey.Contains("BOND") || node.dialogueKey.Contains("SOLID") ? "solidif_bond" : (node.dialogueKey.Contains("17TH") || node.dialogueKey.Contains("ESCORT") || node.dialogueKey.Contains("CALENDAR") ? "17th_calendar" : (node.dialogueKey.Contains("GIANT") ? "giant_synergy" : "standard")));
                string beat = node.dialogueKey.Contains("RESTORATION") ? "restoration" : (node.dialogueKey.Contains("GIANT") ? "giant" : (node.dialogueKey.Contains("17TH") ? "17th" : "general"));
                lines.Add($"{node.dialogueKey}|{node.companion}|{node.moonGate}|{node.trustRequired}|{node.VoiceDirection}|{node.VOIntensity:F1}|{ctx}|{beat}");
            }
            string report = string.Join("\n", lines);
            if (logToConsole) Debug.Log("[VO-PREP R7] === TARTARIA Full 7-Comp VO Recording Script + Real Playback ===\n" + report + "\n=== End. Hooked to CompanionManager.PlayVoiceLineWithIntensity for production pipeline. ===");
        }

        public void PrepAllHighIntensityVO() => GenerateVOScriptForRecording(false, true);

        // Save/Load + Factories (R7 enriched VO directions for new beats)
        static DialogueNode Node(CompanionId c, int moon, string key, TrustLevel trust, string voiceDir = null, float intensity = 0.65f) => new()
        {
            companion = c, moonGate = moon, dialogueKey = key, trustRequired = trust, requiresWorldChoice = false,
            VoiceDirection = voiceDir ?? (key.Contains("17TH") || key.Contains("CALENDAR") ? "low reverb rail hum + 17th echo, urgent hopeful, 432 tail" : (key.Contains("GIANT") || key.Contains("SOLID") ? "warm intimate giant/solidif whisper, rising harmonic resonance, full emotional 0.95+" : (key.Contains("RESTORATION") || key.Contains("ESCORT") ? "physical tell celebrate/lean, sincere warmth" : "standard conversational"))),
            VOIntensity = intensity
        };

        static DialogueNode ChoiceNode(CompanionId c, int moon, string key, TrustLevel trust)
        {
            return new DialogueNode
            {
                companion = c, moonGate = moon, dialogueKey = key, trustRequired = trust,
                requiresWorldChoice = true,
                VoiceDirection = "choice branch — giant/calendar weight",
                VOIntensity = 0.85f
            };
        }

        // 6-arg overload used by R7 world-choice gated nodes (Thorne StarFort, Milo Aurora, etc.)
        static DialogueNode ChoiceNode(CompanionId c, int moon, string key,
            WorldChoiceTracker.WorldChoiceId choiceId, WorldChoiceTracker.ChoiceOption option, TrustLevel trust)
        {
            return new DialogueNode
            {
                companion = c, moonGate = moon, dialogueKey = key, trustRequired = trust,
                requiresWorldChoice = true,
                worldChoiceId = choiceId,
                worldChoiceRequired = option,
                VoiceDirection = "choice branch — world-gated giant/calendar weight",
                VOIntensity = 0.88f
            };
        }

        /// <summary>
        /// Track audio log playback for companion memory systems.
        /// Called by AudioLogPlayable when player discovers echo memories.
        /// </summary>
        public void OnAudioLogPlayed(string logId)
        {
            Debug.Log($"[CompanionDialogueArcs] Audio log played: {logId}");
            // Future: track specific companion reactions, unlock trust milestones
        }

        // ... full save/load as R6 ...
    }
}
