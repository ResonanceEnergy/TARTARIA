using System;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// World Choice Tracker — persists the 6 major branching decisions (W1-W6).
    /// Each World Choice is offered at a specific Moon and has lasting
    /// consequences on gameplay, dialogue, and visual appearance.
    ///
    /// W1 (Moon 2)  — Cassian's Offer: accept or refuse intel sharing
    /// W2 (Moon 4)  — Star Fort: militarize or sanctify the fortress
    /// W3 (Moon 6)  — Korath's Sacrifice: allow or prevent the ritual
    /// W4 (Moon 9)  — Aurora City: open gates or seal the city
    /// W5 (Moon 11) — Zereth's Plea: forgive or condemn
    /// W6 (Moon 13) — Final Alignment: restoration or transcendence
    ///
    /// Cross-ref: docs/22_DIALOGUE_BRANCHING.md, docs/03A_MAIN_STORYLINE_REWRITE.md
    /// </summary>
    [DisallowMultipleComponent]
    public class WorldChoiceTracker : MonoBehaviour
    {
        public static WorldChoiceTracker Instance { get; private set; }

        // ─── Choice Definitions ──────────────────────

        public enum WorldChoiceId : byte
        {
            W1_CassiansOffer = 1,
            W2_StarFort = 2,
            W3_KorathSacrifice = 3,
            W4_AuroraCity = 4,
            W5_ZerethPlea = 5,
            W6_FinalAlignment = 6
        }

        public enum ChoiceOption : byte
        {
            NotChosen = 0,
            OptionA = 1,   // generally the "trust/open/forgive" path
            OptionB = 2    // generally the "reject/seal/condemn" path
        }

        [Serializable]
        public class WorldChoiceDef
        {
            public WorldChoiceId id;
            public int moonNumber;
            public string title;
            public string optionALabel;
            public string optionBLabel;
            public string optionAConsequence;
            public string optionBConsequence;
        }

        // ─── State ───────────────────────────────────

        readonly Dictionary<WorldChoiceId, ChoiceOption> _choices = new();
        readonly List<WorldChoiceDef> _definitions = new();

        // ─── Events ─────────────────────────────────

        public event Action<WorldChoiceId, ChoiceOption> OnChoiceMade;
        public event Action<WorldChoiceId> OnChoicePresented;

        // ─── Lifecycle ───────────────────────────────

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            RegisterChoices();
        }

        void RegisterChoices()
        {
            Reg(WorldChoiceId.W1_CassiansOffer, 2, "Cassian's Offer",
                "Accept intel sharing", "Refuse — trust no one",
                "Cassian provides map reveals; RS gain +10% in explored zones",
                "Self-reliance bonus; 5% RS gain everywhere, no intel sharing");

            Reg(WorldChoiceId.W2_StarFort, 4, "The Star Fort",
                "Militarize the fortress", "Sanctify as a healing temple",
                "Thorne's militia gains +25% strength; combat-focused zone",
                "Lirael gains +25% healing; restoration-focused zone");

            Reg(WorldChoiceId.W3_KorathSacrifice, 6, "Korath's Sacrifice",
                "Allow the ritual to proceed", "Intervene to stop it",
                "Unlock hidden lore path; Korath trust +30; RS penalty -5",
                "Save Korath from harm; Korath trust +10; RS gain +5");

            Reg(WorldChoiceId.W4_AuroraCity, 9, "Aurora City Gates",
                "Open the gates to all", "Seal the city for protection",
                "Economy boost +50%; risk of corruption spreading",
                "Corruption contained; economy stays base; safe zone created");

            Reg(WorldChoiceId.W5_ZerethPlea, 11, "Zereth's Plea",
                "Forgive Zereth", "Condemn Zereth",
                "Zereth becomes ally; unlocks redemption arc; final boss weakened",
                "Zereth becomes final boss phase 2; harder ending; more RS reward");

            Reg(WorldChoiceId.W6_FinalAlignment, 13, "The Final Alignment",
                "Restoration — return the old world", "Transcendence — evolve beyond",
                "True Ending A: Tartaria restored to former glory",
                "True Ending B: Tartaria transcends into a new resonance plane");

            // Initialize all as not chosen
            foreach (var def in _definitions)
                _choices[def.id] = ChoiceOption.NotChosen;
        }

        void Reg(WorldChoiceId id, int moon, string title, string a, string b, string ca, string cb)
        {
            _definitions.Add(new WorldChoiceDef
            {
                id = id, moonNumber = moon, title = title,
                optionALabel = a, optionBLabel = b,
                optionAConsequence = ca, optionBConsequence = cb
            });
        }

        // ─── Public API ──────────────────────────────

        /// <summary>
        /// Present a choice to the player (called by CampaignFlowController at the correct Moon).
        /// </summary>
        public void PresentChoice(WorldChoiceId choiceId)
        {
            OnChoicePresented?.Invoke(choiceId);
            Debug.Log($"[WorldChoice] Presenting: {choiceId}");
        }

        /// <summary>
        /// Record the player's choice. Fires consequences.
        /// </summary>
        public void MakeChoice(WorldChoiceId choiceId, ChoiceOption option)
        {
            if (option == ChoiceOption.NotChosen) return;
            if (_choices.ContainsKey(choiceId) && _choices[choiceId] != ChoiceOption.NotChosen)
            {
                Debug.LogWarning($"[WorldChoice] {choiceId} already chosen!");
                return;
            }

            _choices[choiceId] = option;
            Debug.Log($"[WorldChoice] {choiceId} = {option}");

            ApplyConsequences(choiceId, option);
            OnChoiceMade?.Invoke(choiceId, option);
            SaveManager.Instance?.MarkDirty();
        }

        public ChoiceOption GetChoice(WorldChoiceId id)
        {
            return _choices.TryGetValue(id, out var opt) ? opt : ChoiceOption.NotChosen;
        }

        public bool IsChoiceMade(WorldChoiceId id)
        {
            return _choices.TryGetValue(id, out var opt) && opt != ChoiceOption.NotChosen;
        }

        public IReadOnlyList<WorldChoiceDef> Definitions => _definitions;

        // ─── Consequences ────────────────────────────

        void ApplyConsequences(WorldChoiceId id, ChoiceOption option)
        {
            switch (id)
            {
                case WorldChoiceId.W1_CassiansOffer:
                    if (option == ChoiceOption.OptionA)
                        CassianNPCController.Instance?.EnableIntelSharing();
                    break;

                case WorldChoiceId.W2_StarFort:
                    if (option == ChoiceOption.OptionA)
                        ThorneController.Instance?.ActivateMilitia();
                    break;

                case WorldChoiceId.W3_KorathSacrifice:
                    if (option == ChoiceOption.OptionA)
                        KorathController.Instance?.CompleteSacrificeRitual();
                    else
                        KorathController.Instance?.PreventSacrifice();
                    break;

                case WorldChoiceId.W4_AuroraCity:
                    // ConsequenceVisuals handles the visual shift
                    break;

                case WorldChoiceId.W5_ZerethPlea:
                    if (option == ChoiceOption.OptionA)
                        ZerethController.Instance?.BeginRedemptionArc();
                    break;

                case WorldChoiceId.W6_FinalAlignment:
                    // Ending path determined
                    break;
            }

            // Notify consequence visuals
            ConsequenceVisuals.Instance?.OnWorldChoiceChanged(id, option);
        }

        // ─── Save/Load ──────────────────────────────

        public WorldChoiceSaveData GetSaveData()
        {
            var keys = new int[_choices.Count];
            var vals = new int[_choices.Count];
            int i = 0;
            foreach (var kvp in _choices)
            {
                keys[i] = (int)kvp.Key;
                vals[i] = (int)kvp.Value;
                i++;
            }
            return new WorldChoiceSaveData
            {
                choiceIds = keys,
                choiceValues = vals
            };
        }

        public void LoadSaveData(WorldChoiceSaveData data)
        {
            if (data.choiceIds == null || data.choiceValues == null) return;
            int count = Mathf.Min(data.choiceIds.Length, data.choiceValues.Length);
            for (int i = 0; i < count; i++)
            {
                var id = (WorldChoiceId)data.choiceIds[i];
                var opt = (ChoiceOption)data.choiceValues[i];
                if (_choices.ContainsKey(id))
                    _choices[id] = opt;
            }
        }
    }

    [Serializable]
    public class WorldChoiceSaveData
    {
        public int[] choiceIds;
        public int[] choiceValues;
    }
}
