using UnityEngine;
using UnityEngine.AI;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// CassianController - Mentor companion (100% Implementation)
    /// Combat trainer, tactical advisor, mysterious past.
    /// From 01_LORE_BIBLE.md + 05_CHARACTERS_DIALOGUE.md.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Animator))]
    public class CassianController : MonoBehaviour
    {
        public static CassianController Instance { get; private set; }

        [Header("Character Stats")]
        [SerializeField] private string characterName = "Cassian";
        [SerializeField] private int trustLevel = 50; // Starts higher (experienced ally)
        [SerializeField] private bool hasDarkSecret = true; // Plot twist: Reset agent

        [Header("Combat Training")]
        [SerializeField] private bool hasTaughtBasicCombat = false;
        [SerializeField] private bool hasTaughtAdvancedCombat = false;
        [SerializeField] private int combatTipsGiven = 0;

        [Header("AI Behavior")]
        [SerializeField] private float followDistance = 5f;
        [SerializeField] private bool isFollowingPlayer = false; // Only follows during specific quests

        [Header("References")]
        [SerializeField] private Transform player;
        [SerializeField] private NavMeshAgent navAgent;
        [SerializeField] private Animator animator;

        private readonly string[] INTRO_LINES = new[]
        {
            "You''re persistent. I''ll give you that.",
            "These ruins... I know them better than I should.",
            "If you want to survive out here, you''ll need to learn to fight.",
            "Trust me. I''ve been doing this longer than you can imagine."
        };

        private readonly string[] COMBAT_TIPS = new[]
        {
            "Don''t just swing blindly. Match their frequency.",
            "Dodge first, strike second. Always.",
            "The Mud Golems are slow but relentless. Use that.",
            "Harmonic damage is key. Learn the frequencies.",
            "Watch their movements. They telegraph everything."
        };

        private readonly string[] DARK_HINTS = new[]
        {
            "I''ve seen the Reset from... another perspective.",
            "Some secrets are buried for a reason.",
            "You remind me of someone I used to know. Before.",
            "Not all of us chose our sides freely."
        };

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            navAgent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
        }

        void Start()
        {
            if (player == null)
            {
                var spawner = PlayerSpawner.Instance;
                if (spawner != null && spawner.IsPlayerSpawned())
                    player = spawner.GetPlayer().transform;
            }

            // Subscribe to events
            GameEvents.OnEnemyKilled += OnEnemyKilled;
            GameEvents.OnPlayerDamaged += OnPlayerDamaged;

            Debug.Log($"[CassianController] ✅ {characterName} initialized (Trust: {trustLevel})");
        }

        void OnDestroy()
        {
            GameEvents.OnEnemyKilled -= OnEnemyKilled;
            GameEvents.OnPlayerDamaged -= OnPlayerDamaged;
        }

        void Update()
        {
            if (isFollowingPlayer && player != null)
            {
                float distance = Vector3.Distance(transform.position, player.position);
                if (distance > followDistance)
                {
                    navAgent.SetDestination(player.position);
                }
                else
                {
                    navAgent.isStopped = true;
                }
            }
        }

        void OnEnemyKilled(EnemyKilledEventArgs args)
        {
            // Occasionally give combat tips
            if (Random.value < 0.3f && combatTipsGiven < 5)
            {
                SayRandomLine(COMBAT_TIPS);
                combatTipsGiven++;
            }
        }

        void OnPlayerDamaged(PlayerDamagedEventArgs args)
        {
            if (!hasTaughtBasicCombat)
            {
                Say("Dodge more! You''re taking too many hits!");
                hasTaughtBasicCombat = true;
            }
        }

        public void TeachCombatTraining()
        {
            Say("Alright. Let me show you how to fight properly.");
            Say("First rule: match your frequency to theirs. Resonance is your weapon.");
            hasTaughtBasicCombat = true;

            // Grant player combat abilities
            PlayerAbilities.Instance?.UnlockAbility("HarmonicStrike");
        }

        public void TeachAdvancedCombat()
        {
            if (!hasTaughtBasicCombat)
            {
                Say("Master the basics first.");
                return;
            }

            Say("You''re ready for advanced techniques. Listen carefully...");
            Say("Harmonic combos multiply damage. Chain your strikes.");
            hasTaughtAdvancedCombat = true;

            PlayerAbilities.Instance?.UnlockAbility("HarmonicCombo");
        }

        public void RevealDarkSecret()
        {
            if (!hasDarkSecret) return;

            Say("There''s something you need to know about me...");
            Say("I... I was part of the Reset. I worked for them.");
            Say("But seeing what they did... what WE did... I couldn''t stay.");
            Say("I''ve been trying to undo it ever since. Even if it kills me.");

            hasDarkSecret = false; // Revealed
            trustLevel = 100; // Full trust after honesty

            Debug.Log("[CassianController] Dark secret revealed!");
        }

        void Say(string line)
        {
            Debug.Log($"[Cassian]: {line}");
            DialogueManager.Instance?.ShowDialogue(characterName, line, 5f);
            AudioFeedbackController.Instance?.PlayDialogue(characterName, line);
        }

        void SayRandomLine(string[] lines)
        {
            if (lines.Length > 0)
                Say(lines[Random.Range(0, lines.Length)]);
        }

        public void SetFollowing(bool follow) => isFollowingPlayer = follow;
        public int GetTrustLevel() => trustLevel;
        public bool HasDarkSecret() => hasDarkSecret;
    }
}
