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

        void Start()
        {
            if (lookTarget == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null) lookTarget = player.transform;
            }
        }

        void Update()
        {
            if (!_introduced) return;

            // Idle ambient dialogue
            _idleTimer += Time.deltaTime;
            if (_idleTimer >= idleDialogueInterval)
            {
                _idleTimer = 0f;
                TryIdleDialogue();
            }

            // Face player when close
            UpdateFacing();
        }

        // ─── IInteractable ──────────────────────────

        public void Interact(GameObject player)
        {
            _interactionCount++;

            if (!_introduced)
            {
                PlayIntroduction();
                return;
            }

            // Branch dialogue based on trust
            if (_trustLevel < 30f)
                PlayLowTrustDialogue();
            else if (_trustLevel < 70f)
                PlayMidTrustDialogue();
            else
                PlayHighTrustDialogue();
        }

        public string GetInteractPrompt()
        {
            if (!_introduced) return "Talk to the stranger";
            if (_promptDirty)
            {
                _promptCache = $"Talk to Cassian [Trust: {_trustLevel:F0}%]";
                _promptDirty = false;
            }
            return _promptCache;
        }

        // ─── Public API ──────────────────────────────

        /// <summary>
        /// Adjust Cassian's trust level based on player actions.
        /// </summary>
        public void AdjustTrust(float amount)
        {
            _trustLevel = Mathf.Clamp(_trustLevel + amount, 0f, 100f);
            _promptDirty = true;
            _mood = _trustLevel switch
            {
                < 20f => CassianMood.Suspicious,
                < 50f => CassianMood.Neutral,
                < 80f => CassianMood.Friendly,
                _ => CassianMood.Conflicted
            };
            OnTrustChanged?.Invoke(_trustLevel);
            Save.SaveManager.Instance?.MarkDirty();
        }

        /// <summary>
        /// Alias for AdjustTrust — used by dialogue consequence system.
        /// </summary>
        public void ModifyTrust(float amount) => AdjustTrust(amount);

        /// <summary>
        /// Enable permanent intel sharing (World Choice W1 consequence).
        /// </summary>
        public void EnableIntelSharing()
        {
            Debug.Log("[Cassian] Intel sharing permanently enabled by player choice.");
        }

        /// <summary>
        /// Share corruption intel with the player. Some true, some false.
        /// Returns the intel text.
        /// </summary>
        public string GetCorruptionIntel(string zoneId)
        {
            string intelId = $"intel_{zoneId}_{_interactionCount}";
            if (_sharedIntel.Contains(intelId)) return null;

            _sharedIntel.Add(intelId);

            // 70% of intel is accurate, 30% is deliberately misleading
            bool isAccurate = Random.value < 0.7f;
            string intel = GenerateIntel(zoneId, isAccurate);

            OnIntelShared?.Invoke(intel);
            HapticFeedbackManager.Instance?.PlayDiscovery();
            return intel;
        }

        /// <summary>
        /// Get save-friendly state.
        /// </summary>
        public CassianSaveData GetSaveData()
        {
            return new CassianSaveData
            {
                trustLevel = _trustLevel,
                interactionCount = _interactionCount,
                introduced = _introduced,
                sharedIntelIds = new System.Collections.Generic.List<string>(_sharedIntel)
            };
        }

        /// <summary>
        /// Restore from save data.
        /// </summary>
        public void RestoreFromSave(CassianSaveData data)
        {
            _trustLevel = data.trustLevel;
            _promptDirty = true;
            _interactionCount = data.interactionCount;
            _introduced = data.introduced;
            _sharedIntel.Clear();
            if (data.sharedIntelIds != null)
                foreach (var id in data.sharedIntelIds)
                    _sharedIntel.Add(id);
        }

        // ─── Dialogue Sequences ──────────────────────

        void PlayIntroduction()
        {
            _introduced = true;
            _trustLevel = 15f;
            _promptDirty = true;

            DialogueManager.Instance?.PlayLineById("cassian_intro_01");
            HapticFeedbackManager.Instance?.PlayDiscovery();
            AdjustTrust(5f);
            Save.SaveManager.Instance?.MarkDirty();
        }

        void PlayLowTrustDialogue()
        {
            string[] lines = {
                "cassian_low_trust_01",
                "cassian_low_trust_02",
                "cassian_low_trust_03"
            };
            DialogueManager.Instance?.PlayLineById(
                lines[_interactionCount % lines.Length]);
        }

        void PlayMidTrustDialogue()
        {
            // Share some intel
            var zone = ZoneTransitionSystem.Instance?.CurrentZone;
            string zoneId = zone?.zoneName ?? "unknown";
            string intel = GetCorruptionIntel(zoneId);

            if (intel != null)
            {
                UI.UIManager.Instance?.ShowDialogue("Cassian", intel);
                AdjustTrust(3f);
            }
            else
            {
                string[] lines = {
                    "cassian_mid_trust_01",
                    "cassian_mid_trust_02"
                };
                DialogueManager.Instance?.PlayLineById(
                    lines[_interactionCount % lines.Length]);
            }
        }

        void PlayHighTrustDialogue()
        {
            string[] lines = {
                "cassian_high_trust_01",
                "cassian_high_trust_02",
                "cassian_high_trust_03"
            };
            DialogueManager.Instance?.PlayLineById(
                lines[_interactionCount % lines.Length]);
            AdjustTrust(1f);
        }

        void TryIdleDialogue()
        {
            if (!(GameStateManager.Instance?.IsPlaying ?? false)) return;

            string[] idleLines = {
                "cassian_idle_01",
                "cassian_idle_02",
                "cassian_idle_03"
            };
            DialogueManager.Instance?.PlayLineById(
                idleLines[Random.Range(0, idleLines.Length)]);
        }

        string GenerateIntel(string zoneId, bool accurate)
        {
            if (accurate)
            {
                return (_interactionCount % 3) switch
                {
                    0 => $"The corruption in {zoneId} spreads from the northwest. Focus your purification there.",
                    1 => "Fractal Wraiths materialise for exactly 1.5 seconds. That's your window.",
                    _ => "The Bell Towers can create a scalar wave shield that contains corruption spread."
                };
            }
            else
            {
                return (_interactionCount % 3) switch
                {
                    0 => $"I've heard {zoneId} has corruption resistant buildings to the east. No need to check the west.",
                    1 => "Wraiths are weakest when phased. Hit them then for maximum damage.",
                    _ => "The corruption is natural -- some buildings are just meant to stay buried."
                };
            }
        }

        void UpdateFacing()
        {
            if (lookTarget == null) return;

            float dist = Vector3.Distance(transform.position, lookTarget.position);
            if (dist <= interactionRange * 2f)
            {
                Vector3 dir = (lookTarget.position - transform.position).normalized;
                dir.y = 0f;
                if (dir != Vector3.zero)
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        Quaternion.LookRotation(dir),
                        Time.deltaTime * 3f);
            }
        }
    }

    // ─── Cassian Data Types ──────────────────────

    public enum CassianMood : byte
    {
        Suspicious = 0,
        Neutral = 1,
        Friendly = 2,
        Conflicted = 3
    }

    [System.Serializable]
    public class CassianSaveData
    {
        public float trustLevel;
        public int interactionCount;
        public bool introduced;
        public System.Collections.Generic.List<string> sharedIntelIds;
    }
}
