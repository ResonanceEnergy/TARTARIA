using UnityEngine;
using System.Collections;
using Tartaria.Core;
using Tartaria.Gameplay;
using Tartaria.Input;
using Tartaria.Audio;

#pragma warning disable CS0067  // Event never used
#pragma warning disable CS0219  // Variable assigned but not used
#pragma warning disable CS0414  // Field assigned but not used
namespace Tartaria.Integration
{
    /// <summary>
    /// 6-Band Healing System controller for Moon 5.
    /// Manages healing auras, ceremonies, and 6-band resonance mechanics.
    /// </summary>
    public class SixBandHealingController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] float healingRadius = 50f;
        [SerializeField] int baseHealingRate = 5; // HP per second
        [SerializeField] float ceremonyDuration = 10f;

        Vector3 _centerPoint;
        bool _ceremonyActive = false;
        GameObject _ceremonyVFX;

        public void Initialize(Vector3 center, float radius)
        {
            _centerPoint = center;
            healingRadius = radius;

            Debug.Log("[SixBandHealing] 6-band healing system initialized.");
        }

        void Update()
        {
            // Passive healing for player in White City area
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                float dist = Vector3.Distance(player.transform.position, _centerPoint);
                if (dist <= healingRadius)
                {
                    var playerHealth = player.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.Heal((int)(baseHealingRate * Time.deltaTime));
                    }
                }
            }
        }

        public void StartHealingCeremony()
        {
            if (_ceremonyActive) return;

            StartCoroutine(HealingCeremonySequence());
        }

        IEnumerator HealingCeremonySequence()
        {
            _ceremonyActive = true;

            Debug.Log("[SixBandHealing] Healing ceremony begins!");
            GameEvents.RaiseHUDShowObjective("⚡ 6-BAND HEALING CEREMONY ⚡");

            // Ceremony VFX: large golden sphere
            _ceremonyVFX = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _ceremonyVFX.transform.position = _centerPoint + Vector3.up * 3f;
            _ceremonyVFX.transform.localScale = Vector3.one * 0.5f;

            Renderer ceremonyRend = _ceremonyVFX.GetComponent<Renderer>();
            ceremonyRend.material.color = new Color(1f, 0.9f, 0.5f, 0.6f);

            // Expand sphere over time
            float elapsed = 0f;
            while (elapsed < ceremonyDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / ceremonyDuration;

                _ceremonyVFX.transform.localScale = Vector3.one * Mathf.Lerp(0.5f, 15f, progress);
                ceremonyRend.material.color = new Color(1f, 0.9f, 0.5f, Mathf.Lerp(0.6f, 0.1f, progress));

                yield return null;
            }

            // Ceremony complete
            Destroy(_ceremonyVFX);
            _ceremonyActive = false;

            Debug.Log("[SixBandHealing] Healing ceremony complete!");
            GameEvents.RaiseHUDShowObjective("⚡ Healing ceremony complete! All NPCs restored ⚡");

            // Audio
            AudioManager.Instance?.PlaySFX2D("Moon5_HealingCeremonyComplete");

            // Quest completion
            QuestManager.Instance?.CompleteQuest("moon5_17_healing_ceremony");

            // Dialogue
            DialogueManager.Instance?.PlayContextDialogue("moon5_healing_ceremony_complete");
        }
    }

    /// <summary>
    /// White City Scholar NPC — provides lore and dialogue about pavilions.
    /// </summary>
    public class WhiteCityScholarNPC : MonoBehaviour, IInteractable
    {
        public int scholarIndex;
        public event System.Action OnDialogueComplete;

        readonly string[] _dialogueLines = {
            "Welcome to the White City. These pavilions once housed the world's greatest architects.",
            "The 1893 World's Fair was no temporary exhibit. It was a living city, frozen in time.",
            "Golden-ratio precision in every column. The Tartarians never measured — they *listened*.",
            "Captain Thorne's been circling for two centuries. Stubborn pilot, that one.",
            "The 6-band healing auras... they're returning. I can feel it in my bones."
        };

        bool _hasSpoken = false;

        public string GetInteractPrompt() => "[E] Talk to Scholar";

        public void Interact(GameObject player)
        {
            if (!_hasSpoken)
            {
                _hasSpoken = true;
                OnDialogueComplete?.Invoke();
            }

            string line = _dialogueLines[scholarIndex % _dialogueLines.Length];

            Debug.Log($"[Scholar {scholarIndex}] {line}");
            GameEvents.RaiseHUDShowDialogue($"Scholar {scholarIndex + 1}", line);

            DialogueManager.Instance?.PlayContextDialogue($"moon5_scholar_{scholarIndex}");
            AudioManager.Instance?.PlaySFX2D("NPC_Scholar_Voice");
        }
    }

    /// <summary>
    /// White City Pilgrim NPC — seekers of healing.
    /// </summary>
    public class WhiteCityPilgrimNPC : MonoBehaviour, IInteractable
    {
        public int pilgrimIndex;

        readonly string[] _dialogueLines = {
            "I traveled three hundred miles to find this place. The healing aura... it's real.",
            "My daughter was sick for years. One hour in the pavilion, and she's laughing again."
        };

        public string GetInteractPrompt() => "[E] Talk to Pilgrim";

        public void Interact(GameObject player)
        {
            string line = _dialogueLines[pilgrimIndex % _dialogueLines.Length];

            Debug.Log($"[Pilgrim {pilgrimIndex}] {line}");
            GameEvents.RaiseHUDShowDialogue($"Pilgrim", line);

            DialogueManager.Instance?.PlayContextDialogue($"moon5_pilgrim_{pilgrimIndex}");
            AudioManager.Instance?.PlaySFX2D("NPC_Pilgrim_Voice");
        }
    }

    /// <summary>
    /// Airship dock construction interaction.
    /// </summary>
    public class AirshipDockInteract : MonoBehaviour, IInteractable
    {
        public event System.Action OnDockComplete;

        [SerializeField] float constructionProgress = 0f;
        const float CONSTRUCTION_DURATION = 6f;

        bool _isComplete = false;
        bool _isConstructing = false;

        public string GetInteractPrompt()
        {
            if (_isComplete) return "Airship Dock Complete ✓";
            if (_isConstructing) return $"Constructing... {constructionProgress / CONSTRUCTION_DURATION:P0}";
            return "[E] Construct Airship Dock Foundation";
        }

        public void Interact(GameObject player)
        {
            if (_isComplete || _isConstructing) return;

            StartCoroutine(ConstructDock());
        }

        IEnumerator ConstructDock()
        {
            _isConstructing = true;

            Debug.Log("[AirshipDock] Constructing dock foundation...");
            GameEvents.RaiseHUDShowObjective("Constructing airship dock...");

            // Construction progress
            while (constructionProgress < CONSTRUCTION_DURATION)
            {
                constructionProgress += Time.deltaTime;
                yield return null;
            }

            _isComplete = true;
            _isConstructing = false;

            Debug.Log("[AirshipDock] Dock foundation complete!");

            // Audio
            AudioManager.Instance?.PlaySFX2D("Moon5_DockComplete");

            // Notify spawner
            OnDockComplete?.Invoke();
        }
    }

    /// <summary>
    /// Dissonance Healer boss fight controller.
    /// Phase 1: Corrupted healing waves (purple damage zones)
    /// Phase 2: Summons corrupted healers (mini-bosses)
    /// </summary>
    public class DissonanceHealerBoss : MonoBehaviour
    {
        public event System.Action OnPhase1Complete;
        public event System.Action OnPhase2Complete;
        public event System.Action OnBossDefeated;

        [Header("Boss Stats")]
        [SerializeField] int maxHealth = 1000;
        int _currentHealth;
        int _currentPhase = 1;

        [Header("Phase Thresholds")]
        const int PHASE_2_THRESHOLD = 500; // 50% HP

        bool _phase1Complete = false;
        bool _phase2Complete = false;
        bool _isDefeated = false;

        void Start()
        {
            _currentHealth = maxHealth;

            // Boss health bar (TODO: integrate with UI system)
            Debug.Log($"[DissonanceHealer] Boss spawned! HP: {_currentHealth}/{maxHealth}");

            StartCoroutine(BossBehavior());
        }

        IEnumerator BossBehavior()
        {
            while (!_isDefeated)
            {
                if (_currentPhase == 1)
                {
                    // Phase 1: Corrupted healing waves
                    yield return StartCoroutine(Phase1Attack());
                }
                else if (_currentPhase == 2)
                {
                    // Phase 2: Summon corrupted healers
                    yield return StartCoroutine(Phase2Attack());
                }

                yield return new WaitForSeconds(3f);
            }
        }

        IEnumerator Phase1Attack()
        {
            // Spawn purple damage wave
            Debug.Log("[DissonanceHealer] Phase 1 attack: Corrupted healing wave!");

            // TODO: Spawn damage zone VFX
            GameObject damageZone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            damageZone.transform.position = transform.position;
            damageZone.transform.localScale = new Vector3(10f, 0.1f, 10f);

            Renderer zoneRend = damageZone.GetComponent<Renderer>();
            zoneRend.material.color = new Color(0.8f, 0.2f, 1f, 0.5f);

            yield return new WaitForSeconds(2f);

            Destroy(damageZone);
        }

        IEnumerator Phase2Attack()
        {
            // Summon mini-boss
            Debug.Log("[DissonanceHealer] Phase 2 attack: Summon corrupted healer!");

            // TODO: Spawn mini-boss enemy

            yield return new WaitForSeconds(2f);
        }

        public void TakeDamage(int damage)
        {
            if (_isDefeated) return;

            _currentHealth -= damage;
            Debug.Log($"[DissonanceHealer] Took {damage} damage! HP: {_currentHealth}/{maxHealth}");

            // Check phase transitions
            if (!_phase1Complete && _currentHealth <= PHASE_2_THRESHOLD)
            {
                _phase1Complete = true;
                _currentPhase = 2;
                OnPhase1Complete?.Invoke();
                Debug.Log("[DissonanceHealer] PHASE 2 BEGINS!");
                GameEvents.RaiseHUDShowObjective("⚠ BOSS PHASE 2 ⚠");
            }

            // Check defeat
            if (_currentHealth <= 0)
            {
                _isDefeated = true;
                _phase2Complete = true;
                OnPhase2Complete?.Invoke();
                OnBossDefeated?.Invoke();

                // Death VFX
                StartCoroutine(DeathSequence());
            }
        }

        IEnumerator DeathSequence()
        {
            Debug.Log("[DissonanceHealer] Boss defeated! Purifying...");

            // Fade out
            Renderer bossRend = GetComponentInChildren<Renderer>();
            if (bossRend != null)
            {
                float elapsed = 0f;
                float duration = 2f;
                Color startColor = bossRend.material.color;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                    bossRend.material.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                    yield return null;
                }
            }

            // Destroy boss
            Destroy(gameObject);
        }

        void OnCollisionEnter(Collision collision)
        {
            // Player collision damage
            if (collision.gameObject.CompareTag("Player"))
            {
                var playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(20);
                }
            }

            // Player attack damage
            if (collision.gameObject.CompareTag("PlayerAttack"))
            {
                TakeDamage(50);
            }
        }
    }
}
