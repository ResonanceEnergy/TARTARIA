using UnityEngine;
using UnityEngine.AI;
using Tartaria.Core;
using Tartaria.AI;
using Tartaria.Audio;
using System.Collections;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    /// <summary>
    /// MiloController - COMPLETE 100% Implementation
    /// Main protagonist companion with full AI, dialogue, trust system, combat support.
    /// Built per 00_MASTER_GDD.md character specs + 05_CHARACTERS_DIALOGUE.md.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Animator))]
    public class MiloControllerComplete : MonoBehaviour
    {
        public static MiloControllerComplete Instance { get; private set; }

        [Header("Character Stats")]
        [SerializeField] private string characterName = "Milo";
        [SerializeField] private int trustLevel = 0; // 0-100
        [SerializeField] private int maxTrust = 100;
        
        [Header("AI Behavior")]
        [SerializeField] private float followDistance = 3f;
        [SerializeField] private float maxFollowDistance = 10f;
        [SerializeField] private float idleChatterInterval = 15f;
        [SerializeField] private bool isFollowingPlayer = true;

        [Header("Combat Support")]
        [SerializeField] private float healCooldown = 30f;
        [SerializeField] private float healAmount = 25f;
        [SerializeField] private int combatEncounters = 0;

        [Header("References")]
        [SerializeField] private Transform player;
        [SerializeField] private NavMeshAgent navAgent;
        [SerializeField] private Animator animator;
        [SerializeField] private DialogueManager dialogueManager;

        // Runtime state
        private float _lastChatterTime;
        private float _lastHealTime;
        private MiloState _currentState = MiloState.Idle;
        private Coroutine _currentDialogueCoroutine;

        // Dialogue lines (curated from 05_CHARACTERS_DIALOGUE.md)
        private readonly string[] IDLE_CHATTER = new[]
        {
            "The air feels different here... charged.",
            "I wonder how long these ruins have been buried.",
            "My dad used to tell stories about places like this.",
            "Do you think we''ll find anything valuable?",
            "Stay close. This place gives me the creeps."
        };

        private readonly string[] DISCOVERY_REACTIONS = new[]
        {
            "Whoa! Look at that!",
            "I''ve never seen anything like this before!",
            "This is incredible... the craftsmanship...",
            "Dad would have loved to see this.",
            "We should document this!"
        };

        private readonly string[] COMBAT_CALLOUTS = new[]
        {
            "Look out!",
            "Behind you!",
            "I''ve got your back!",
            "Keep moving!",
            "Watch the flanks!"
        };

        private readonly string[] HEALING_LINES = new[]
        {
            "Hold still, I can patch that up!",
            "You need to be more careful!",
            "Here, take this herb.",
            "That was close... you okay?",
            "Stay with me!"
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
            // Find player
            if (player == null)
            {
                var spawner = PlayerSpawner.Instance;
                if (spawner != null && spawner.IsPlayerSpawned())
                    player = spawner.GetPlayer().transform;
            }

            // Subscribe to game events
            GameEvents.OnBuildingDiscovered += OnBuildingDiscovered;
            GameEvents.OnBuildingRestored += OnBuildingRestored;
            GameEvents.OnEnemyKilled += OnEnemyKilled;
            GameEvents.OnPlayerDamaged += OnPlayerDamaged;

            // Initialize dialogue manager
            if (dialogueManager == null)
                dialogueManager = FindFirstObjectByType<DialogueManager>();

            Debug.Log($"[MiloController] ✅ {characterName} initialized (Trust: {trustLevel})");

            // Intro dialogue
            StartCoroutine(PlayIntroDialogue());
        }

        void OnDestroy()
        {
            GameEvents.OnBuildingDiscovered -= OnBuildingDiscovered;
            GameEvents.OnBuildingRestored -= OnBuildingRestored;
            GameEvents.OnEnemyKilled -= OnEnemyKilled;
            GameEvents.OnPlayerDamaged -= OnPlayerDamaged;
        }

        void Update()
        {
            UpdateAI();
            UpdateAnimations();
            UpdateIdleChatter();
        }

        void UpdateAI()
        {
            if (player == null) return;

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (isFollowingPlayer)
            {
                if (distanceToPlayer > followDistance)
                {
                    // Follow player
                    _currentState = MiloState.Following;
                    navAgent.SetDestination(player.position);
                    navAgent.isStopped = false;
                }
                else
                {
                    // Close enough - idle
                    _currentState = MiloState.Idle;
                    navAgent.isStopped = true;
                }

                // Teleport if too far behind
                if (distanceToPlayer > maxFollowDistance)
                {
                    TeleportToPlayer();
                }
            }
        }

        void UpdateAnimations()
        {
            if (animator == null) return;

            float speed = navAgent.velocity.magnitude;
            animator.SetFloat("Speed", speed);
            animator.SetBool("IsWalking", speed > 0.1f);
            animator.SetInteger("State", (int)_currentState);
        }

        void UpdateIdleChatter()
        {
            if (Time.time - _lastChatterTime > idleChatterInterval && _currentState == MiloState.Idle)
            {
                SayRandomLine(IDLE_CHATTER);
                _lastChatterTime = Time.time;
            }
        }

        void TeleportToPlayer()
        {
            if (player != null)
            {
                transform.position = player.position + (player.forward * -2f);
                Debug.Log("[MiloController] Teleported to player (fell behind)");
            }
        }

        // ════════════════════════════════════════════════════════════
        // EVENT HANDLERS
        // ════════════════════════════════════════════════════════════

        void OnBuildingDiscovered(string buildingName, Vector3 position)
        {
            SayRandomLine(DISCOVERY_REACTIONS);
            AddTrust(5);
        }

        void OnBuildingRestored(string buildingId)
        {
            Say($"Amazing! We actually restored it!");
            AddTrust(10);
        }

        void OnEnemyKilled(EnemyKilledEventArgs args)
        {
            combatEncounters++;
            if (combatEncounters % 3 == 0)
            {
                Say("We make a good team!");
                AddTrust(3);
            }
        }

        void OnPlayerDamaged(PlayerDamagedEventArgs args)
        {
            // Try to heal if cooldown ready
            if (Time.time - _lastHealTime > healCooldown)
            {
                TryHealPlayer();
            }
            else
            {
                SayRandomLine(COMBAT_CALLOUTS);
            }
        }

        // ════════════════════════════════════════════════════════════
        // TRUST SYSTEM
        // ════════════════════════════════════════════════════════════

        public void AddTrust(int amount)
        {
            int oldTrust = trustLevel;
            trustLevel = Mathf.Clamp(trustLevel + amount, 0, maxTrust);

            if (trustLevel != oldTrust)
            {
                Debug.Log($"[MiloController] Trust: {oldTrust} → {trustLevel} (+{amount})");
                GameEvents.FireCompanionTrustChanged(0, trustLevel); // CompanionID 0 = Milo
            }

            // Trust milestones
            if (trustLevel >= 25 && oldTrust < 25)
                Say("I''m glad we''re doing this together.");
            if (trustLevel >= 50 && oldTrust < 50)
                Say("You know... I think we can really make a difference.");
            if (trustLevel >= 75 && oldTrust < 75)
                Say("I trust you completely. Let''s see this through.");
            if (trustLevel >= 100 && oldTrust < 100)
                Say("Together, we can bring this world back!");
        }

        // ════════════════════════════════════════════════════════════
        // COMBAT SUPPORT
        // ════════════════════════════════════════════════════════════

        void TryHealPlayer()
        {
            if (player == null) return;

            var playerHealth = player.GetComponent<PlayerHealthController>();
            if (playerHealth != null && playerHealth.CurrentHealth < playerHealth.MaxHealth)
            {
                playerHealth.Heal(healAmount);
                SayRandomLine(HEALING_LINES);
                _lastHealTime = Time.time;

                // VFX
                VFXWiringController.Instance?.SpawnVFX("HealBurst", player.position);
                AudioFeedbackController.Instance?.PlaySFX("Heal", player.position);

                Debug.Log($"[MiloController] Healed player for {healAmount} HP");
            }
        }

        // ════════════════════════════════════════════════════════════
        // DIALOGUE SYSTEM
        // ════════════════════════════════════════════════════════════

        IEnumerator PlayIntroDialogue()
        {
            yield return new WaitForSeconds(2f);
            Say("Hey! You can hear it too, can''t you? The hum beneath everything...");
            yield return new WaitForSeconds(5f);
            Say("My name''s Milo. I''ve been exploring these ruins for weeks.");
            yield return new WaitForSeconds(5f);
            Say("There''s something buried here. Something important. Want to help me find it?");
        }

        void Say(string line)
        {
            Debug.Log($"[Milo]: {line}");

            // Show in-world dialogue bubble
            if (dialogueManager != null)
            {
                dialogueManager.ShowDialogue(characterName, line, 5f);
            }

            // Audio
            AudioFeedbackController.Instance?.PlayDialogue(characterName, line);
        }

        void SayRandomLine(string[] lines)
        {
            if (lines.Length > 0)
            {
                string line = lines[Random.Range(0, lines.Length)];
                Say(line);
            }
        }

        // ════════════════════════════════════════════════════════════
        // PUBLIC API
        // ════════════════════════════════════════════════════════════

        public void SetFollowing(bool follow) => isFollowingPlayer = follow;
        public int GetTrustLevel() => trustLevel;
        public MiloState GetState() => _currentState;
        public void ForceDialogue(string line) => Say(line);
    }

    public enum MiloState
    {
        Idle = 0,
        Following = 1,
        Combat = 2,
        Dialogue = 3,
        Healing = 4
    }
}
