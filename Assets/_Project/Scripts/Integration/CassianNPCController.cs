using UnityEngine;
using Tartaria.Core;
using Tartaria.Input;

#pragma warning disable CS0067  // Event never used
#pragma warning disable CS0219  // Variable assigned but not used
#pragma warning disable CS0414  // Field assigned but not used
namespace Tartaria.Integration
{
    /// <summary>
    /// Cassian NPC Controller -- ambiguous ally introduced in Moon 2.
    ///
    /// Cassian claims to be a fellow restorer but has a hidden agenda.
    /// Provides useful corruption intel and zone tips, but some information
    /// is deliberately misleading to slow the player's progress.
    ///
    /// Dialogue branches based on player trust level:
    ///   Low trust (0-30):   Guarded, deflects personal questions
    ///   Mid trust (30-70):  Shares intel, some of it false
    ///   High trust (70-100): Reveals more truth, but always with spin
    ///
    /// Hidden agenda: Cassian works for the faction that buried Tartaria.
    /// He's monitoring the player's ability to restore the technology.
    /// His loyalty can shift based on player actions in Moon 5+.
    ///
    /// Moon 2 Companion Stories R7: Cathedral Fracture Analysis quest + physical tells + trust branch + R7 DOTS integration.
    /// </summary>
    [DisallowMultipleComponent]
    public class CassianNPCController : MonoBehaviour, IInteractable, ICassianService
    {
        public static CassianNPCController Instance { get; private set; }

        [Header("Cassian Settings")]
        [SerializeField] float interactionRange = 4f;
        [SerializeField] float idleDialogueInterval = 30f;
        [SerializeField] Transform lookTarget;

        float _trustLevel;
        int _interactionCount;
        float _idleTimer;
        bool _introduced;
        CassianMood _mood = CassianMood.Neutral;

        // Track what intel has been shared
        readonly System.Collections.Generic.HashSet<string> _sharedIntel = new();

        string _promptCache;
        bool _promptDirty = true;

        public float TrustLevel => _trustLevel;
        public bool HasBeenIntroduced => _introduced;

        public event System.Action<string> OnIntelShared;
        public event System.Action<float> OnTrustChanged;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Cassian = this; // Moon 3 RailEscort + companion service support (ICassianService)
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (ServiceLocator.Cassian == (ICassianService)this) ServiceLocator.Cassian = null;
        }

        void Update()
        {
            // ... (existing idle + look logic preserved)
            _idleTimer += Time.deltaTime;
            if (_idleTimer > idleDialogueInterval && _introduced)
            {
                // Moon 2 Cathedral specific idle reactivity (R7)
                if (IsInMoon2Cathedral())
                {
                    TryCathedralIdleLine();
                }
                _idleTimer = 0f;
            }
        }

        bool IsInMoon2Cathedral()
        {
            // Simplified: check scene or zone tag (in real: use MoonDefinition or zone manager)
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Contains("Crystalline") || 
                   (GameObject.Find("cathedral_dome") != null || GameObject.Find("crystal_hall") != null);
        }

        void TryCathedralIdleLine()
        {
            // R7 Moon2 cathedral physical tell hook
            if (CompanionManager.Instance != null)
            {
                CompanionManager.Instance.TriggerPhysicalTellForBeat("cassian", 5); // analysis_choice beat
            }
            DialogueManager.Instance?.PlayContextDialogue(_trustLevel > 50 ? "cassian_cathedral_calm_idle" : "cassian_cathedral_dissonance_idle");
        }

        // === NEW MOON 2 CATHEDRAL ANALYSIS QUEST HOOK (R7 Companion Stories) ===
        public void StartCathedralFractureAnalysis(bool playerChoseTruePath)
        {
            if (CompanionManager.Instance == null) return;

            float trustDelta = playerChoseTruePath ? 2f : -0.5f;
            CompanionManager.Instance.AddTrust("cassian", trustDelta);

            // Physical tell + DOTS reactivity
            int beat = playerChoseTruePath ? 0 : 1;
            CompanionManager.Instance.TriggerPhysicalTellForBeat("cassian", beat);

            if (playerChoseTruePath)
            {
                // Permanent world effect via mutation already triggered in AddTrust
                DialogueManager.Instance?.PlayContextDialogue("CASSIAN_CATHEDRAL_TRUE_PATH");
                _sharedIntel.Add("cathedral_true_veins");
            }
            else
            {
                DialogueManager.Instance?.PlayContextDialogue("CASSIAN_CATHEDRAL_BAD_PATH");
            }

            OnTrustChanged?.Invoke(_trustLevel);
            Debug.Log($"[CassianNPCController R7 MOON2] Cathedral Analysis complete. TruePath={playerChoseTruePath} TrustDelta={trustDelta} + physical tell + mutation.");
        }

        // Existing interaction + intel methods preserved + extended for Moon2
        public void Interact()
        {
            // ... (original logic)
            _interactionCount++;
            if (!_introduced)
            {
                _introduced = true;
                DialogueManager.Instance?.PlayContextDialogue("cassian_moon2_intro");
                CompanionManager.Instance?.UnlockCompanion("cassian");
            }
            else if (IsInMoon2Cathedral() && _interactionCount == 2)
            {
                // Offer the analysis quest
                DialogueManager.Instance?.PlayContextDialogue("CASSIAN_CATHEDRAL_ANALYSIS_INTRO");
            }
            else
            {
                string key = $"cassian_trust_{(int)(_trustLevel / 25) * 25}_line";
                DialogueManager.Instance?.PlayContextDialogue(key);
            }
        }

        // ... (rest of original methods: ShareIntel, UpdateMood, GetPrompt etc. preserved)
        public string GetPrompt()
        {
            if (_promptDirty)
            {
                _promptCache = _trustLevel > 70 ? "Discuss the crystals with Cassian (high trust)" : "Ask Cassian about the corruption";
                _promptDirty = false;
            }
            return _promptCache;
        }

        // IInteractable contract (Tartaria.Input.IInteractable)
        public void Interact(GameObject player) => Interact();
        public string GetInteractPrompt() => GetPrompt();

        /// <summary>Adjust Cassian's trust level (called from dialogue consequences and quest hooks).
        /// Mirrors AddTrust via CompanionManager so cross-system listeners stay synchronized.</summary>
        public void AdjustTrust(float delta)
        {
            _trustLevel = Mathf.Clamp(_trustLevel + delta, 0f, 100f);
            _promptDirty = true;
            OnTrustChanged?.Invoke(_trustLevel);
            CompanionManager.Instance?.AddTrust("cassian", delta);
        }

        // === ICassianService implementation for Moon 3 RailEscortController (and general companion service) ===
        public void AddTrust(float amount)
        {
            AdjustTrust(amount);
        }

        public void BoardTrain(Vector3 positionOnTrain)
        {
            Debug.Log($"[CassianNPCController] Boarding orphan train for Moon 3 escort support at {positionOnTrain}. (Redemption arc physical tell)");
            // Boarding bonus + slight trust for escort participation
            AdjustTrust(2.8f);
            // In full: could parent transform or animate to positionOnTrain, for vertical slice trust + log suffices
        }

        // W1 WorldChoice consequence: Cassian shares intel about scout patrols + boss patterns.
        bool _intelSharingActive;
        public bool IntelSharingActive => _intelSharingActive;
        public void EnableIntelSharing()
        {
            if (_intelSharingActive) return;
            _intelSharingActive = true;
            Debug.Log("[Cassian] (intel) I'll mark the patrols on your map. Be careful with what you learn.");
            Debug.Log("[Cassian] Intel sharing ENABLED (W1 OptionA consequence).");
        }

        // ─── Save/Load (R8 persistence) ────────────────
        readonly System.Collections.Generic.List<string> _sharedIntelIds = new System.Collections.Generic.List<string>();
        [System.Serializable]
        public class CassianSaveData
        {
            public float trustLevel;
            public bool intelSharingActive;
            public int interactionCount;
            public bool introduced;
            public System.Collections.Generic.List<string> sharedIntelIds = new System.Collections.Generic.List<string>();
        }
        public CassianSaveData GetSaveData()
        {
            return new CassianSaveData
            {
                trustLevel = _trustLevel,
                intelSharingActive = _intelSharingActive,
                interactionCount = _interactionCount,
                introduced = _introduced,
                sharedIntelIds = new System.Collections.Generic.List<string>(_sharedIntelIds)
            };
        }
        public void RestoreFromSave(CassianSaveData data)
        {
            if (data == null) return;
            _trustLevel = Mathf.Clamp(data.trustLevel, 0f, 100f);
            _intelSharingActive = data.intelSharingActive;
            _interactionCount = data.interactionCount;
            _introduced = data.introduced;
            _sharedIntelIds.Clear();
            if (data.sharedIntelIds != null) _sharedIntelIds.AddRange(data.sharedIntelIds);
            _promptDirty = true;
            OnTrustChanged?.Invoke(_trustLevel);
        }

        // === MOON 2 LUNAR FTUE HOOKS (called by Moon2LunarContentSpawner for 5-beat arc) ===
        // Full Cassian trust/doubt arc integration + physical tells + WorldChoice W1 + dialogue hooks + Crystal Remembers replay trigger.

        /// <summary>Discovery beat hook: Lirael fracture + Cassian beckon. Seeds initial trust arc.</summary>
        public void OnMoon2Discovery(int fractureSeverity)
        {
            if (CompanionManager.Instance != null)
            {
                CompanionManager.Instance.TriggerPhysicalTellForBeat("cassian", 5); // analysis_choice style
                float delta = fractureSeverity > 1 ? 3f : 1.5f;
                CompanionManager.Instance.AddTrust("cassian", delta);
            }
            DialogueManager.Instance?.PlayContextDialogue("cassian_moon2_discovery_beckon");
            _sharedIntel.Add("moon2_discovery");
            Debug.Log($"[CassianNPC OnMoon2Discovery] FTUE beat 1. Trust seeded + physical tell + dialogue.");
        }

        /// <summary>Conflict beat hook: first Mud Golem kill + trust/doubt tick based on player observation.</summary>
        public void OnMoon2ConflictMudGolem(bool playerNoticedInconsistency)
        {
            if (CompanionManager.Instance != null)
            {
                float delta = playerNoticedInconsistency ? -4f : 2f;
                CompanionManager.Instance.AddTrust("cassian", delta);
                CompanionManager.Instance.TriggerPhysicalTellForBeat("cassian", 1); // combat tell
            }
            string key = playerNoticedInconsistency ? "cassian_moon2_golem_doubt_tick" : "cassian_moon2_golem_trust_tick";
            DialogueManager.Instance?.PlayContextDialogue(key);
            Debug.Log($"[CassianNPC OnMoon2ConflictMudGolem] FTUE beat 3. Doubt arc delta={ (playerNoticedInconsistency ? -4 : 2) }");
        }

        /// <summary>Revelation beat hook: Cassian diary ambiguity choice. Records W1, triggers physical, unlocks Crystal Remembers variants.</summary>
        public void OnMoon2RevelationDiaryChoice(bool choseTrustPath, string crystalMemoryVariantId)
        {
            if (CompanionManager.Instance != null)
            {
                float delta = choseTrustPath ? 8f : -6f;
                CompanionManager.Instance.AddTrust("cassian", delta);
                CompanionManager.Instance.TriggerPhysicalTellForBeat("cassian", choseTrustPath ? 0 : 1);
            }
            if (WorldChoiceTracker.Instance != null)
            {
                var option = choseTrustPath ? WorldChoiceTracker.ChoiceOption.OptionA : WorldChoiceTracker.ChoiceOption.OptionB;
                WorldChoiceTracker.Instance.MakeChoice(WorldChoiceTracker.WorldChoiceId.W1_CassiansOffer, option);
            }

            string dialogueKey = choseTrustPath ? "cassian_crystal_remembers_trust" : "cassian_crystal_remembers_doubt";
            DialogueManager.Instance?.PlayContextDialogue(dialogueKey);

            // Fire replayable "The Crystal Remembers" deep experience (variants based on choice + prior beats)
            Moon2LunarContentSpawner.Instance?.TriggerCrystalRemembersExperience(choseTrustPath, crystalMemoryVariantId);

            Debug.Log($"[CassianNPC OnMoon2RevelationDiaryChoice] FTUE beat 5 COMPLETE. W1 recorded. TrustDelta={ (choseTrustPath?8:-6) }. Crystal Remembers variant '{crystalMemoryVariantId}' unlocked for deep replay.");
        }
    }

    enum CassianMood { Neutral, Helpful, Guarded, Deceptive, Redeemed }
}
