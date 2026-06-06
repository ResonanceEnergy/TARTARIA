using UnityEngine;
using UnityEngine.AI;
using Tartaria.Core;
using Tartaria.Audio;
using System.Collections;

namespace Tartaria.Integration
{
    /// <summary>
    /// Lirael - Orphan Train Ghost Companion (100% Implementation)
    /// Spectral child Echo with lullaby/music powers, innocence, and haunting wisdom.
    /// From 01_LORE_BIBLE.md + 03A_MAIN_STORYLINE_REWRITE.md.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Animator))]
    public class LiraelControllerComplete : MonoBehaviour
    {
        public static LiraelControllerComplete Instance { get; private set; }

        [Header("Character Stats")]
        [SerializeField] private string characterName = "Lirael";
        [SerializeField] private int trustLevel = 0;
        [SerializeField] private int manifestationLevel = 0; // 0-100 (fully spectral → fully solid)
        
        [Header("Music Powers")]
        [SerializeField] private float lullabyHealRadius = 10f;
        [SerializeField] private float lullabyHealAmount = 5f; // per second
        [SerializeField] private float lullabyCooldown = 60f;
        [SerializeField] private bool isPlayingLullaby = false;

        [Header("AI Behavior")]
        [SerializeField] private float followDistance = 4f;
        [SerializeField] private bool isFollowingPlayer = true;
        [SerializeField] private float floatHeight = 0.5f; // Hovers above ground

        [Header("References")]
        [SerializeField] private Transform player;
        [SerializeField] private NavMeshAgent navAgent;
        [SerializeField] private Animator animator;
        [SerializeField] private Material spectralMaterial;

        private float _lastLullabyTime = -9999f;
        private Coroutine _lullabyCoroutine;

        // Dialogue (from 01_LORE_BIBLE.md)
        private readonly string[] INTRO_LINES = new[]
        {
            "They told us the mud was a blanket. It was a grave.",
            "I remember... singing on the train. All of us children.",
            "The world was warm once. Full of light and sound.",
            "Are you here to wake the world up too?"
        };

        private readonly string[] DISCOVERY_REACTIONS = new[]
        {
            "This place... I remember it! Before the mud!",
            "It sang once. Can you hear the echo?",
            "The children used to play here...",
            "Make it sing again. Please."
        };

        private readonly string[] LULLABY_LINES = new[]
        {
            "*begins humming an ancient lullaby*",
            "This is the song the stone-cutters sang...",
            "Listen... the earth remembers...",
            "432 Hertz. The healing frequency."
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

            // Subscribe to events
            GameEvents.OnBuildingRestored += OnBuildingRestored;
            GameEvents.OnBuildingDiscovered += OnBuildingDiscovered;

            // Set spectral appearance
            UpdateSpectralAppearance();

            Debug.Log($"[LiraelController] ✅ {characterName} initialized (Manifestation: {manifestationLevel}%)");

            StartCoroutine(PlayIntroDialogue());
        }

        void OnDestroy()
        {
            GameEvents.OnBuildingRestored -= OnBuildingRestored;
            GameEvents.OnBuildingDiscovered -= OnBuildingDiscovered;
        }

        void Update()
        {
            UpdateAI();
            UpdateFloating();
            UpdateLullabyHealing();
        }

        void UpdateAI()
        {
            if (player == null || !isFollowingPlayer) return;

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer > followDistance)
            {
                navAgent.SetDestination(player.position);
                navAgent.isStopped = false;
            }
            else
            {
                navAgent.isStopped = true;
            }
        }

        void UpdateFloating()
        {
            // Spectral floating effect (sine wave bob)
            float bob = Mathf.Sin(Time.time * 2f) * 0.1f;
            transform.position = new Vector3(transform.position.x, floatHeight + bob, transform.position.z);
        }

        void UpdateSpectralAppearance()
        {
            // Fade in as manifestation increases
            if (spectralMaterial != null)
            {
                float alpha = Mathf.Lerp(0.3f, 1f, manifestationLevel / 100f);
                Color color = spectralMaterial.color;
                color.a = alpha;
                spectralMaterial.color = color;
            }
        }

        void UpdateLullabyHealing()
        {
            if (!isPlayingLullaby) return;

            // Heal all nearby allies (including player)
            Collider[] nearby = Physics.OverlapSphere(transform.position, lullabyHealRadius);
            foreach (var col in nearby)
            {
                var health = col.GetComponent<PlayerHealthController>();
                if (health != null && health.CurrentHealth < health.MaxHealth)
                {
                    health.Heal(lullabyHealAmount * Time.deltaTime);
                }
            }
        }

        // ════════════════════════════════════════════════════════════
        // EVENT HANDLERS
        // ════════════════════════════════════════════════════════════

        void OnBuildingRestored(string buildingId)
        {
            // Increase manifestation when buildings are restored
            IncreaseManifestation(5);
            SayRandomLine(DISCOVERY_REACTIONS);
        }

        void OnBuildingDiscovered(string buildingName, Vector3 position)
        {
            SayRandomLine(DISCOVERY_REACTIONS);
        }

        // ════════════════════════════════════════════════════════════
        // MUSIC POWERS
        // ════════════════════════════════════════════════════════════

        public void PlayLullaby()
        {
            if (Time.time - _lastLullabyTime < lullabyCooldown)
            {
                Say("I need more time to gather the melody...");
                return;
            }

            if (_lullabyCoroutine != null)
                StopCoroutine(_lullabyCoroutine);

            _lullabyCoroutine = StartCoroutine(LullabySequence());
        }

        IEnumerator LullabySequence()
        {
            SayRandomLine(LULLABY_LINES);
            isPlayingLullaby = true;
            _lastLullabyTime = Time.time;

            // Play audio
            AudioFeedbackController.Instance?.PlayMusic("Lullaby432Hz", loop: true);

            // VFX: Healing aura
            VFXWiringController.Instance?.SpawnVFX("LullabyAura", transform.position);

            // Heal for 10 seconds
            float duration = 10f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            isPlayingLullaby = false;
            AudioFeedbackController.Instance?.StopMusic("Lullaby432Hz");

            Say("The world listened... for a moment.");
        }

        // ════════════════════════════════════════════════════════════
        // MANIFESTATION SYSTEM
        // ════════════════════════════════════════════════════════════

        public void IncreaseManifestation(int amount)
        {
            int oldLevel = manifestationLevel;
            manifestationLevel = Mathf.Clamp(manifestationLevel + amount, 0, 100);

            if (manifestationLevel != oldLevel)
            {
                Debug.Log($"[LiraelController] Manifestation: {oldLevel}% → {manifestationLevel}%");
                UpdateSpectralAppearance();
            }

            // Milestones
            if (manifestationLevel >= 50 && oldLevel < 50)
                Say("I can feel... the warmth. I''m becoming real again.");
            if (manifestationLevel >= 100 && oldLevel < 100)
                Say("I''m... solid? I can touch things! Thank you!");
        }

        // ════════════════════════════════════════════════════════════
        // DIALOGUE
        // ════════════════════════════════════════════════════════════

        IEnumerator PlayIntroDialogue()
        {
            yield return new WaitForSeconds(5f);
            Say(INTRO_LINES[0]); // "They told us the mud was a blanket. It was a grave."
            yield return new WaitForSeconds(6f);
            Say("My name is Lirael. I was on the train... the orphan train.");
            yield return new WaitForSeconds(6f);
            Say("Will you help me wake the others?");
        }

        void Say(string line)
        {
            Debug.Log($"[Lirael]: {line}");
            DialogueManager.Instance?.ShowDialogue(characterName, line, 5f);
            AudioFeedbackController.Instance?.PlayDialogue(characterName, line);
        }

        void SayRandomLine(string[] lines)
        {
            if (lines.Length > 0)
                Say(lines[Random.Range(0, lines.Length)]);
        }

        // ════════════════════════════════════════════════════════════
        // PUBLIC API
        // ════════════════════════════════════════════════════════════

        public int GetManifestationLevel() => manifestationLevel;
        public bool IsFullyManifested() => manifestationLevel >= 100;
        public void ForceDialogue(string line) => Say(line);
    }
}
