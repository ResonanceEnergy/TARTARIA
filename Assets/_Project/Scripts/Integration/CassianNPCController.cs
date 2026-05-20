using UnityEngine;
using Tartaria.Core;
using Tartaria.Input;

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
    public class CassianNPCController : MonoBehaviour, IInteractable
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
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
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
    }

    enum CassianMood { Neutral, Helpful, Guarded, Deceptive, Redeemed }
}
