using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Gameplay;
using Tartaria.AI;
using Tartaria.Input;
using Tartaria.UI;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 7 Korath companion controller.
    /// 25-foot ancient giant, teaches 9-band energy and harmonic rock cutting.
    /// Multi-session thawing from Aether ice → awakening → teaching → sacrifice.
    /// </summary>
    public class KorathCompanionController : MonoBehaviour
    {
        public static KorathCompanionController Instance { get; private set; }

        public enum KorathStage
        {
            Frozen,        // In Aether ice
            Awakening,     // Mid-thaw
            Awakened,      // Fully conscious, teaching phase
            Sacrificed     // Post-climax, echo-only
        }

        [Header("State")]
        [SerializeField] KorathStage currentStage = KorathStage.Frozen;

        [Header("Spawn")]
        [SerializeField] Vector3 stasisVaultCenter = new Vector3(400f, -30f, 500f);

        GameObject _korathGiant;
        Material _korathMaterial;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SpawnAwakened()
        {
            if (_korathGiant != null) return;

            _korathGiant = new GameObject("Korath_Giant");
            _korathGiant.transform.position = stasisVaultCenter;
            _korathGiant.transform.localScale = Vector3.one * 2.5f; // 25-foot scale (~7.5m)

            // Visual: massive humanoid capsule
            var filter = _korathGiant.AddComponent<MeshFilter>();
            filter.mesh = Resources.GetBuiltinResource<Mesh>("Capsule.fbx");
            
            var renderer = _korathGiant.AddComponent<MeshRenderer>();
            _korathMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            _korathMaterial.color = new Color(0.3f, 0.25f, 0.2f); // Ancient stone-flesh
            renderer.material = _korathMaterial;

            // Add collider
            var collider = _korathGiant.AddComponent<CapsuleCollider>();
            collider.height = 5f;
            collider.radius = 1.5f;

            // Add dialogue
            var dialogue = _korathGiant.AddComponent<KorathDialogue>();

            // Violet-aurora light (9-band energy)
            Light korathLight = _korathGiant.AddComponent<Light>();
            korathLight.type = LightType.Point;
            korathLight.color = new Color(0.7f, 0.5f, 1f); // Violet-aurora
            korathLight.range = 20f;
            korathLight.intensity = 3f;

            currentStage = KorathStage.Awakened;

            Debug.Log("[Korath] Giant awakened! 25-foot ancient teacher stands.");
            HUDController.Instance?.ShowObjective("⚡ KORATH AWAKENS ⚡");
        }

        public void TeachNineBandEnergy()
        {
            if (currentStage != KorathStage.Awakened) return;

            Debug.Log("[Korath] Teaching 9-band energy: anti-gravity, consciousness amplification.");
            
            // Grant player 9-band abilities
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var abilities = player.GetComponent<PlayerAbilities>();
                if (abilities != null)
                {
                    abilities.Unlock9BandEnergy();
                }
            }

            HUDController.Instance?.ShowObjective("9-Band Energy Unlocked! Anti-gravity + consciousness buffs available.");
            
            DialogueManager.Instance?.PlayContextDialogue("korath_teach_9band");
            Audio.AudioManager.Instance?.PlaySFX2D("NineBandUnlock");

            // Quest progress
            QuestManager.Instance?.CompleteQuest("moon7_9band_unlock");
        }

        public void TeachHarmonicRockCutting()
        {
            if (currentStage != KorathStage.Awakened) return;

            Debug.Log("[Korath] Teaching harmonic rock cutting: precision stone shaping via resonance.");
            
            // Grant player rock cutting ability
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var abilities = player.GetComponent<PlayerAbilities>();
                if (abilities != null)
                {
                    abilities.UnlockHarmonicRockCutting();
                }
            }

            HUDController.Instance?.ShowObjective("Harmonic Rock Cutting Unlocked! Shape stone with resonance.");
            
            DialogueManager.Instance?.PlayContextDialogue("korath_teach_rockcutting");

            // Quest progress
            QuestManager.Instance?.CompleteQuest("moon7_rockcutting_unlock");
        }

        public void TriggerSacrifice()
        {
            if (currentStage != KorathStage.Awakened) return;

            currentStage = KorathStage.Sacrificed;

            Debug.Log("[Korath] SACRIFICE: Korath pours resonance into bell tower → lights half planetary grid!");
            StartCoroutine(SacrificeSequence());
        }

        IEnumerator SacrificeSequence()
        {
            HUDController.Instance?.ShowObjective("⚡ KORATH'S SACRIFICE ⚡");
            yield return new WaitForSeconds(2f);

            // Dialogue
            HUDController.Instance?.ShowDialogue("Korath", "The grid must wake. Take my resonance, old friends...");
            yield return new WaitForSeconds(3f);

            // VFX: Golden light pours from Korath into sky
            GameObject vfx = new GameObject("KorathSacrifice_VFX");
            vfx.transform.position = _korathGiant.transform.position + Vector3.up * 5f;

            ParticleSystem ps = vfx.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 5f;
            main.startSpeed = 10f;
            main.startSize = 1f;
            main.startColor = new Color(1f, 0.9f, 0.5f);
            main.loop = false;
            main.maxParticles = 1000;

            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 1000) });

            Destroy(vfx, 6f);

            yield return new WaitForSeconds(4f);

            // Korath's body fades to golden light
            if (_korathMaterial != null)
            {
                _korathMaterial.color = new Color(1f, 0.9f, 0.5f, 0.5f);
            }

            yield return new WaitForSeconds(2f);

            // Body disappears, voice remains
            if (_korathGiant != null)
            {
                Destroy(_korathGiant.GetComponent<MeshRenderer>());
            }

            HUDController.Instance?.ShowDialogue("Korath (echo)", "I remain. Voice without form. The old way.");
            yield return new WaitForSeconds(2f);

            Debug.Log("[Korath] Sacrifice complete. Half the planetary grid lights up.");

            // Achievement
            AchievementSystem.Instance?.Unlock("korath_sacrifice");
        }

        public KorathStage CurrentStage => currentStage;
    }

    /// <summary>
    /// Korath dialogue controller.
    /// Dialogue varies by stage.
    /// </summary>
    public class KorathDialogue : MonoBehaviour, IInteractable
    {
        public event System.Action OnTeachingComplete;

        int _dialogueIndex = 0;

        readonly string[][] _dialogueByStage = {
            // Frozen (ice block, muffled voice)
            new[] {
                "You... came. A small spark... carrying the old fire.",
                "Trapped... how long? The ice... holds memory.",
                "Free me... then we teach."
            },
            // Awakening
            new[] {
                "The warmth... I remember warmth.",
                "Maelix... my brother... where is Maelix?",
                "The grid... it still sleeps."
            },
            // Awakened
            new[] {
                "You wake the grid. Good. We were the guardians once.",
                "Nine bands. You've touched seven. Let me show you the ninth.",
                "Maelix fell to dissonance. Zereth... betrayed us. I remain.",
                "The rock cutting— it's not force. It's ASKING. The stone remembers its form.",
                "When the time comes... I will give what remains. The grid needs a pulse."
            },
            // Sacrificed (echo-only)
            new[] {
                "I am here. Voice without flesh. The old way.",
                "Half the grid wakes. The other half... needs Zereth's key.",
                "You carry our hope now, little spark."
            }
        };

        public string GetInteractPrompt() => "[E] Talk to Korath";

        public void Interact(GameObject player)
        {
            var controller = KorathCompanionController.Instance;
            if (controller == null) return;

            int stageIndex = (int)controller.CurrentStage;
            string[] lines = _dialogueByStage[stageIndex];
            string line = lines[_dialogueIndex % lines.Length];

            Debug.Log($"[Korath] {line}");
            HUDController.Instance?.ShowDialogue("Korath", line);

            DialogueManager.Instance?.PlayContextDialogue($"korath_dialogue_{stageIndex}_{_dialogueIndex}");
            Audio.AudioManager.Instance?.PlaySFX2D("Korath_Voice");

            _dialogueIndex++;

            // Trigger teaching complete after the ninth-band teaching line (stage 2, line 1)
            if (stageIndex == 2 && _dialogueIndex == 2)
            {
                OnTeachingComplete?.Invoke();
            }
        }
    }

    /// <summary>
    /// Moon 7 golem siege boss encounter.
    /// Waves of 30-foot mud golems attack star fort perimeter.
    /// Korath fights beside player in climax.
    /// </summary>
    public class Moon7GolemSiegeBoss : MonoBehaviour
    {
        public static Moon7GolemSiegeBoss Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] int totalGolemWaves = 3;
        [SerializeField] int golemsPerWave = 4;
        [SerializeField] Vector3 starFortCenter = new Vector3(450f, 0f, 550f);
        [SerializeField] float spawnRadius = 40f;

        int _currentWave = 0;
        int _golemsDefeated = 0;
        List<GameObject> _activeGolems = new List<GameObject>();

        public event System.Action OnSiegeComplete;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void StartSiege()
        {
            Debug.Log("[GolemSiege] CLIMAX: Massive golem siege begins! Korath fights beside player!");
            
            HUDController.Instance?.ShowObjective("⚡ GOLEM SIEGE! Defend the star fort! ⚡");
            
            Audio.AudioManager.Instance?.PlaySFX2D("GolemSiegeStart");

            StartCoroutine(SiegeSequence());
        }

        IEnumerator SiegeSequence()
        {
            for (_currentWave = 0; _currentWave < totalGolemWaves; _currentWave++)
            {
                Debug.Log($"[GolemSiege] Wave {_currentWave + 1}/{totalGolemWaves}!");
                
                HUDController.Instance?.ShowObjective($"Wave {_currentWave + 1}/{totalGolemWaves}");

                SpawnWave();

                // Wait for wave clear
                while (_activeGolems.Count > 0)
                {
                    _activeGolems.RemoveAll(g => g == null);
                    yield return new WaitForSeconds(1f);
                }

                yield return new WaitForSeconds(3f); // Breathing room between waves
            }

            OnSiegeComplete?.Invoke();
            
            Debug.Log("[GolemSiege] SIEGE COMPLETE! All waves defeated!");
            HUDController.Instance?.ShowObjective("⚡ SIEGE VICTORIOUS! ⚡");

            QuestManager.Instance?.CompleteQuest("moon7_golem_siege");
        }

        void SpawnWave()
        {
            for (int i = 0; i < golemsPerWave; i++)
            {
                float angle = (i / (float)golemsPerWave) * Mathf.PI * 2f;
                Vector3 spawnPos = starFortCenter + new Vector3(
                    Mathf.Cos(angle) * spawnRadius,
                    0f,
                    Mathf.Sin(angle) * spawnRadius
                );

                GameObject golem = SpawnSiegeGolem(spawnPos, _currentWave * golemsPerWave + i);
                _activeGolems.Add(golem);
            }
        }

        GameObject SpawnSiegeGolem(Vector3 position, int index)
        {
            GameObject golem = new GameObject($"SiegeGolem_{index}");
            golem.transform.position = position;
            golem.transform.localScale = Vector3.one * 3f; // 30-foot scale

            // Visual: large mud golem capsule
            var filter = golem.AddComponent<MeshFilter>();
            filter.mesh = Resources.GetBuiltinResource<Mesh>("Capsule.fbx");
            
            var renderer = golem.AddComponent<MeshRenderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            renderer.material.color = new Color(0.25f, 0.2f, 0.15f); // Living mud

            // Add health
            var health = golem.AddComponent<MudGolemHealth>();
            // TODO: Assembly dependency - enable after Tartaria.AI recompiles
            // health.SetMaxHealth(300f);
            // health.OnDeath += () => OnGolemDefeated(golem);

            // Add collider
            var collider = golem.AddComponent<CapsuleCollider>();
            collider.height = 5f;
            collider.radius = 1.5f;

            // Simple AI: move toward fort center
            var ai = golem.AddComponent<SimpleGolemAI>();
            ai.targetPosition = starFortCenter;

            Debug.Log($"[GolemSiege] Siege golem {index} spawned at {position}");

            return golem;
        }

        void OnGolemDefeated(GameObject golem)
        {
            _golemsDefeated++;
            
            Debug.Log($"[GolemSiege] Golem defeated! ({_golemsDefeated} total)");
            
            // VFX: mud dissolves
            Audio.AudioManager.Instance?.PlaySFX2D("GolemDefeat");
            
            _activeGolems.Remove(golem);
        }
    }

    /// <summary>
    /// Simple AI for siege golems: move toward target, attack in range.
    /// </summary>
    public class SimpleGolemAI : MonoBehaviour
    {
        public Vector3 targetPosition;
        [SerializeField] float moveSpeed = 2f;
        [SerializeField] float attackRange = 5f;
        [SerializeField] float attackCooldown = 2f;

        float _lastAttackTime = 0f;

        void Update()
        {
            if (targetPosition == Vector3.zero) return;

            // Move toward target
            Vector3 direction = (targetPosition - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, targetPosition);

            if (distance > attackRange)
            {
                transform.position += direction * moveSpeed * Time.deltaTime;
                transform.forward = direction;
            }
            else
            {
                // In attack range
                if (Time.time - _lastAttackTime >= attackCooldown)
                {
                    Attack();
                    _lastAttackTime = Time.time;
                }
            }
        }

        void Attack()
        {
            Debug.Log($"[SimpleGolemAI] {gameObject.name} attacks!");
            
            // Simple attack: find player in range
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && Vector3.Distance(transform.position, player.transform.position) <= attackRange)
            {
                var health = player.GetComponent<Tartaria.Gameplay.PlayerHealth>();
                health?.TakeDamage(20); // 20 damage per hit
            }
        }
    }

    /// <summary>
    /// Korath ice thaw interaction (multi-session).
    /// </summary>
    public class KorathIceInteract : MonoBehaviour, IInteractable
    {
        public event System.Action OnThawSession;

        int _sessionsCompleted = 0;
        const int SESSIONS_REQUIRED = 3;

        public string GetInteractPrompt() => $"[Hold E] Thaw Korath ({_sessionsCompleted}/{SESSIONS_REQUIRED})";

        public void Interact(GameObject player)
        {
            if (_sessionsCompleted >= SESSIONS_REQUIRED) return;

            StartCoroutine(ThawSession());
        }

        IEnumerator ThawSession()
        {
            Debug.Log($"[KorathIce] Thaw session {_sessionsCompleted + 1}/{SESSIONS_REQUIRED} starting...");
            
            HUDController.Instance?.ShowObjective("Channeling resonance into Aether ice...");

            yield return new WaitForSeconds(3f);

            _sessionsCompleted++;

            Debug.Log($"[KorathIce] Thaw session complete! ({_sessionsCompleted}/{SESSIONS_REQUIRED})");
            
            OnThawSession?.Invoke();

            if (_sessionsCompleted >= SESSIONS_REQUIRED)
            {
                Debug.Log("[KorathIce] FINAL THAW! Korath awakens!");
                KorathCompanionController.Instance?.SpawnAwakened();
                Destroy(gameObject, 2f);
            }
        }
    }
}
