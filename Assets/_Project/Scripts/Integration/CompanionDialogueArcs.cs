using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Companion Dialogue Arcs — gates companion dialogue availability by Moon
    /// and tracks relationship progression. Each companion has a 13-moon dialogue
    /// arc with escalating trust, lore reveals, and branching based on World Choices.
    ///
    /// Cross-ref: docs/05_CHARACTERS_DIALOGUE.md, docs/22_DIALOGUE_BRANCHING.md
    /// </summary>
    [DisallowMultipleComponent]
    public class CompanionDialogueArcs : MonoBehaviour
    {
        public static CompanionDialogueArcs Instance { get; private set; }

        public enum CompanionId
        {
            Lirael,     // Crystal Singer — joins Moon 1
            Thorne,     // Militia Captain — joins Moon 2
            Korath,     // Star Cartographer — joins Moon 3
            Veritas,    // Bell Tower Keeper — joins Moon 4
            Milo,       // Merchant Prince — joins Moon 5
            Anastasia   // Princess — joins Moon 7
        }

        public enum TrustLevel { Stranger, Acquaintance, Ally, Confidant, Bonded }

        [System.Serializable]
        public struct DialogueNode
        {
            public CompanionId companion;
            public int moonGate;          // Moon number required
            public string dialogueKey;    // Localization key
            public TrustLevel trustRequired;
            public bool requiresWorldChoice;
            public WorldChoiceTracker.WorldChoiceId worldChoiceId;
            public WorldChoiceTracker.ChoiceOption worldChoiceRequired;
        }

        // ─── Static Catalogue ────────────────────────

        static readonly DialogueNode[] Catalogue = new[]
        {
            // Lirael arc: crystal harmonics, restoration faith
            Node(CompanionId.Lirael,  1,  "LIRAEL_INTRO",        TrustLevel.Stranger),
            Node(CompanionId.Lirael,  3,  "LIRAEL_CRYSTAL_SONG", TrustLevel.Acquaintance),
            Node(CompanionId.Lirael,  5,  "LIRAEL_DOUBT",        TrustLevel.Ally),
            Node(CompanionId.Lirael,  8,  "LIRAEL_RESOLVE",      TrustLevel.Ally),
            Node(CompanionId.Lirael, 11,  "LIRAEL_PROPHECY",     TrustLevel.Confidant),
            Node(CompanionId.Lirael, 13,  "LIRAEL_FINALE",       TrustLevel.Bonded),

            // Thorne arc: military honor, loyalty
            Node(CompanionId.Thorne,  2,  "THORNE_INTRO",        TrustLevel.Stranger),
            Node(CompanionId.Thorne,  4,  "THORNE_STAR_FORT",    TrustLevel.Acquaintance),
            Node(CompanionId.Thorne,  6,  "THORNE_SACRIFICE",    TrustLevel.Ally),
            Node(CompanionId.Thorne,  9,  "THORNE_FLEET",        TrustLevel.Ally),
            Node(CompanionId.Thorne, 12,  "THORNE_BELL_WAR",     TrustLevel.Confidant),
            Node(CompanionId.Thorne, 13,  "THORNE_FINALE",       TrustLevel.Bonded),

            // Korath arc: star maps, cosmic order
            Node(CompanionId.Korath,  3,  "KORATH_INTRO",        TrustLevel.Stranger),
            Node(CompanionId.Korath,  5,  "KORATH_MAPPING",      TrustLevel.Acquaintance),
            Node(CompanionId.Korath,  7,  "KORATH_ANASTASIA",    TrustLevel.Ally),
            Node(CompanionId.Korath, 10,  "KORATH_RAIL_STARS",   TrustLevel.Ally),
            Node(CompanionId.Korath, 12,  "KORATH_CONVERGENCE",  TrustLevel.Confidant),
            Node(CompanionId.Korath, 13,  "KORATH_FINALE",       TrustLevel.Bonded),

            // Veritas arc: bell tower resonance, truth-seeking
            Node(CompanionId.Veritas,  4,  "VERITAS_INTRO",       TrustLevel.Stranger),
            Node(CompanionId.Veritas,  6,  "VERITAS_FIRST_BELL",  TrustLevel.Acquaintance),
            Node(CompanionId.Veritas,  9,  "VERITAS_DISSONANCE",  TrustLevel.Ally),
            Node(CompanionId.Veritas, 11,  "VERITAS_PURIFY",      TrustLevel.Ally),
            Node(CompanionId.Veritas, 12,  "VERITAS_ORGAN_PREP",  TrustLevel.Confidant),
            Node(CompanionId.Veritas, 13,  "VERITAS_FINALE",      TrustLevel.Bonded),

            // Milo arc: commerce, pragmatism
            Node(CompanionId.Milo,  5,  "MILO_INTRO",         TrustLevel.Stranger),
            Node(CompanionId.Milo,  7,  "MILO_BARGAIN",       TrustLevel.Acquaintance),
            Node(CompanionId.Milo,  9,  "MILO_AURORA_MARKET",  TrustLevel.Ally),
            Node(CompanionId.Milo, 10,  "MILO_RAIL_TRADE",     TrustLevel.Ally),
            Node(CompanionId.Milo, 12,  "MILO_FESTIVAL_PLAN",  TrustLevel.Confidant),
            Node(CompanionId.Milo, 13,  "MILO_FINALE",         TrustLevel.Bonded),

            // Anastasia arc: princess, solidification, hope
            Node(CompanionId.Anastasia,  7,  "ANASTASIA_INTRO",       TrustLevel.Stranger),
            Node(CompanionId.Anastasia,  8,  "ANASTASIA_MEMORIES",    TrustLevel.Acquaintance),
            Node(CompanionId.Anastasia, 10,  "ANASTASIA_RAIL_JOURNEY",TrustLevel.Ally),
            Node(CompanionId.Anastasia, 11,  "ANASTASIA_AQUIFER",     TrustLevel.Ally),
            Node(CompanionId.Anastasia, 12,  "ANASTASIA_SOLID_PREP",  TrustLevel.Confidant),
            Node(CompanionId.Anastasia, 13,  "ANASTASIA_FINALE",      TrustLevel.Bonded),

            // World-choice gated dialogues
            ChoiceNode(CompanionId.Thorne, 4, "THORNE_STAR_FORT_A",
                WorldChoiceTracker.WorldChoiceId.W2_StarFort,
                WorldChoiceTracker.ChoiceOption.OptionA, TrustLevel.Acquaintance),
            ChoiceNode(CompanionId.Thorne, 4, "THORNE_STAR_FORT_B",
                WorldChoiceTracker.WorldChoiceId.W2_StarFort,
                WorldChoiceTracker.ChoiceOption.OptionB, TrustLevel.Acquaintance),
            ChoiceNode(CompanionId.Milo, 9, "MILO_AURORA_OPEN",
                WorldChoiceTracker.WorldChoiceId.W4_AuroraCity,
                WorldChoiceTracker.ChoiceOption.OptionA, TrustLevel.Ally),
            ChoiceNode(CompanionId.Milo, 9, "MILO_AURORA_SEALED",
                WorldChoiceTracker.WorldChoiceId.W4_AuroraCity,
                WorldChoiceTracker.ChoiceOption.OptionB, TrustLevel.Ally),
        };

        // ─── Runtime State ───────────────────────────

        readonly Dictionary<CompanionId, TrustLevel> _trust = new();
        readonly HashSet<string> _seenDialogues = new();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            // Initialize all companions at Stranger
            foreach (CompanionId c in System.Enum.GetValues(typeof(CompanionId)))
                _trust[c] = TrustLevel.Stranger;
        }

        // ─── Public API ──────────────────────────────

        public TrustLevel GetTrust(CompanionId companion) =>
            _trust.TryGetValue(companion, out var t) ? t : TrustLevel.Stranger;

        public void IncreaseTrust(CompanionId companion)
        {
            if (!_trust.ContainsKey(companion)) return;
            if (_trust[companion] < TrustLevel.Bonded)
            {
                _trust[companion]++;
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
            Debug.Log($"[DialogueArcs] Seen: {dialogueKey}");
        }

        public bool HasSeen(string dialogueKey) => _seenDialogues.Contains(dialogueKey);

        // ─── Save / Load ─────────────────────────────

        [System.Serializable]
        public struct DialogueArcSaveData
        {
            public int[] companionIds;
            public int[] trustLevels;
            public string[] seenKeys;
        }

        public DialogueArcSaveData GetSaveData()
        {
            var ids = new int[_trust.Count];
            var levels = new int[_trust.Count];
            int i = 0;
            foreach (var kv in _trust)
            {
                ids[i] = (int)kv.Key;
                levels[i] = (int)kv.Value;
                i++;
            }

            var seen = new string[_seenDialogues.Count];
            _seenDialogues.CopyTo(seen);

            return new DialogueArcSaveData
            {
                companionIds = ids,
                trustLevels = levels,
                seenKeys = seen
            };
        }

        public void LoadSaveData(DialogueArcSaveData data)
        {
            if (data.companionIds != null)
            {
                for (int i = 0; i < data.companionIds.Length; i++)
                    _trust[(CompanionId)data.companionIds[i]] = (TrustLevel)data.trustLevels[i];
            }

            _seenDialogues.Clear();
            if (data.seenKeys != null)
            {
                foreach (var k in data.seenKeys)
                    _seenDialogues.Add(k);
            }
        }

        // ─── Factories ──────────────────────────────

        static DialogueNode Node(CompanionId c, int moon, string key, TrustLevel trust) => new()
        {
            companion = c, moonGate = moon, dialogueKey = key,
            trustRequired = trust, requiresWorldChoice = false
        };

        static DialogueNode ChoiceNode(CompanionId c, int moon, string key,
            WorldChoiceTracker.WorldChoiceId wcId, WorldChoiceTracker.ChoiceOption wcOpt,
            TrustLevel trust) => new()
        {
            companion = c, moonGate = moon, dialogueKey = key,
            trustRequired = trust, requiresWorldChoice = true,
            worldChoiceId = wcId, worldChoiceRequired = wcOpt
        };
    }
}
