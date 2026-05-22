using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Gameplay;
using Tartaria.Input;
using Tartaria.Save;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 2 (Crystalline Caverns) content spawner — dissonance crystal purging + Cassian intro.
    ///
    /// Design per GDD §03:
    ///   - Dissonance crystals appear inside restored buildings (black, angular, wrong)
    ///   - Micro-giant mode (shrink) to explore fractal architecture + destroy crystals
    ///   - Cassian companion introduction (helpful but suspicious)
    ///   - Mud Golems spawn as dissonance defenders
    ///   - Bell tower repair → scalar wave pulse
    ///   - Ionized fountain purge (climax event)
    ///
    /// Activates when player completes Moon 1 (Echohaven restoration).
    /// </summary>
    public class Moon2ContentSpawner : MonoBehaviour
    {
        public static Moon2ContentSpawner Instance { get; private set; }

        [Header("Content State")]
        [SerializeField] bool moon2Unlocked = false;
        [SerializeField] bool cassianIntroduced = false;
        [SerializeField] bool bellTowerRestored = false;
        [SerializeField] bool fountainPurgeComplete = false;

        [Header("Dissonance Crystals")]
        [SerializeField] int totalCrystals = 12;  // GDD: ~12 crystals scattered in fractal corridors
        int _crystalsDestroyed;

        [Header("Spawning")]
        [SerializeField] Vector3 cassianSpawnPoint = new(15f, 1.5f, 20f);  // Near cathedral entrance
        [SerializeField] Vector3[] crystalSpawnPoints;  // Set via editor or runtime
        [SerializeField] Vector3 bellTowerCenter = new(10f, 12f, 15f);

        List<GameObject> _activeCrystals = new();
        List<DissonanceCrystal> _dissonanceCrystals = new();
        GameObject _cassianNPC;
        bool _contentSpawned;

        public bool IsMoon2Active => moon2Unlocked && !fountainPurgeComplete;
        public int CrystalsRemaining => totalCrystals - _crystalsDestroyed;
        public float PurgeProgress => _crystalsDestroyed / (float)totalCrystals;

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
            // Check if Moon 2 should auto-unlock (Moon 1 complete)
            if (SaveManager.Instance != null)
            {
                var moonProgress = SaveManager.Instance.GetMoonProgress(1);  // Moon 1
                if (moonProgress >= 100f && !moon2Unlocked)
                {
                    UnlockMoon2();
                }
            }
        }

        // ─── Public API ───────────────────────────────

        /// <summary>
        /// Unlocks Moon 2 content (called when Moon 1 complete).
        /// </summary>
        public void UnlockMoon2()
        {
            if (moon2Unlocked) return;

            moon2Unlocked = true;
            Debug.Log("[Moon 2] Lunar Moon unlocked — Challenge of Shadows begins");

            SpawnMoon2Content();

            // Tutorial hint
            // TODO: TutorialSystem not implemented
            // TODO: HUDController not implemented
        }

        /// <summary>
        /// Spawns all Moon 2 content: Cassian, dissonance crystals, Mud Golems.
        /// </summary>
        void SpawnMoon2Content()
        {
            if (_contentSpawned) return;
            _contentSpawned = true;

            // Spawn Cassian NPC
            if (!cassianIntroduced)
            {
                SpawnCassian();
            }

            // Spawn dissonance crystals
            SpawnDissonanceCrystals();

            // Activate dissonance ambient audio
            // TODO: AudioManager.PlayLoopingSFX not implemented;

            Debug.Log($"[Moon 2] Spawned {totalCrystals} dissonance crystals + Cassian NPC");
        }

        void SpawnCassian()
        {
            // Create Cassian NPC
            _cassianNPC = new GameObject("Cassian_NPC");
            _cassianNPC.transform.position = cassianSpawnPoint;

            // Add MeshRenderer (placeholder capsule for beta)
            var filter = _cassianNPC.AddComponent<MeshFilter>();
            filter.mesh = Resources.GetBuiltinResource<Mesh>("Capsule.fbx");
            var renderer = _cassianNPC.AddComponent<MeshRenderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            renderer.material.color = new Color(0.3f, 0.5f, 0.8f);  // Blue-tinted

            // Add collider + interaction
            var collider = _cassianNPC.AddComponent<CapsuleCollider>();
            collider.height = 2f;
            collider.radius = 0.4f;

            // Cassian dialogue trigger
            // TODO: var dialogue = _cassianNPC.AddComponent<DialogueTrigger>();
            // dialogue.dialogueId = "cassian_intro";  // Wired if DialogueTrigger exists

            cassianIntroduced = true;
            Debug.Log("[Moon 2] Cassian spawned at cathedral entrance");

            // Cassian intro dialogue
            DialogueManager.Instance?.PlayContextDialogue("cassian_intro");
        }

        void SpawnDissonanceCrystals()
        {
            // Generate crystal positions if not set manually
            if (crystalSpawnPoints == null || crystalSpawnPoints.Length == 0)
            {
                crystalSpawnPoints = GenerateCrystalPositions(totalCrystals);
            }

            for (int i = 0; i < Mathf.Min(totalCrystals, crystalSpawnPoints.Length); i++)
            {
                var crystal = CreateDissonanceCrystal(crystalSpawnPoints[i], i);
                _activeCrystals.Add(crystal);
            }
        }

        GameObject CreateDissonanceCrystal(Vector3 position, int index)
        {
            var crystal = new GameObject($"DissonanceCrystal_{index:D2}");
            crystal.transform.position = position;
            crystal.transform.rotation = Random.rotation;

            // Visual: black spiky crystal (procedural for beta)
            var filter = crystal.AddComponent<MeshFilter>();
            filter.mesh = CreateCrystalMesh();

            var renderer = crystal.AddComponent<MeshRenderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            renderer.material.color = new Color(0.05f, 0.05f, 0.05f);  // Almost black
            renderer.material.SetFloat("_Metallic", 0.8f);

            // Collider + interaction
            var collider = crystal.AddComponent<BoxCollider>();
            collider.size = Vector3.one * 1.2f;

            // Add DissonanceCrystal component (handles purge interaction)
            var crystalComp = crystal.AddComponent<DissonanceCrystal>();
            crystalComp.OnDestroyed += OnCrystalDestroyed;

            // Pulsing dissonance VFX
            var light = crystal.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.5f, 0.1f, 0.1f);  // Dark red
            light.range = 3f;
            light.intensity = 0.5f;

            return crystal;
        }

        Mesh CreateCrystalMesh()
        {
            // Procedural spiky crystal (simple octahedron for beta)
            var mesh = new Mesh();
            var vertices = new Vector3[]
            {
                new(0, 1.5f, 0),    // top
                new(0.7f, 0, 0),    // front right
                new(0, 0, 0.7f),    // front left
                new(-0.7f, 0, 0),   // back left
                new(0, 0, -0.7f),   // back right
                new(0, -0.5f, 0)    // bottom
            };

            var triangles = new int[]
            {
                // Top faces
                0, 1, 2,
                0, 2, 3,
                0, 3, 4,
                0, 4, 1,
                // Bottom faces
                5, 2, 1,
                5, 3, 2,
                5, 4, 3,
                5, 1, 4
            };

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        Vector3[] GenerateCrystalPositions(int count)
        {
            var positions = new Vector3[count];
            var basePos = cassianSpawnPoint + Vector3.up * 2f;  // Above cathedral entrance

            for (int i = 0; i < count; i++)
            {
                // Scatter in fractal pattern (rough circle for beta)
                float angle = (i / (float)count) * Mathf.PI * 2f;
                float radius = 5f + Random.Range(0f, 8f);
                float height = Random.Range(1f, 10f);

                positions[i] = basePos + new Vector3(
                    Mathf.Cos(angle) * radius,
                    height,
                    Mathf.Sin(angle) * radius
                );
            }

            return positions;
        }

        void OnCrystalDestroyed(DissonanceCrystal crystal)
        {
            _crystalsDestroyed++;

            Debug.Log($"[Moon 2] Dissonance crystal destroyed ({_crystalsDestroyed}/{totalCrystals})");

            // VFX + audio feedback
            Audio.AudioManager.Instance?.PlaySFX2D("Moon2_PurgeCrackle");
            Input.HapticFeedbackManager.Instance?.PlayDiscovery();

            // Progress tracking
            QuestManager.Instance?.ProgressByType(QuestObjectiveType.TalkToNPC /*was ClearDissonance*/, crystal.gameObject.name);

            // Check if all crystals destroyed
            if (_crystalsDestroyed >= totalCrystals && !fountainPurgeComplete)
            {
                TriggerFountainPurge();
            }
        }

        /// <summary>
        /// Triggers climax event: ionized fountain purge (Days 19-24).
        /// </summary>
        void TriggerFountainPurge()
        {
            fountainPurgeComplete = true;

            Debug.Log("[Moon 2] All dissonance crystals destroyed — triggering Fountain Purge climax!");

            // Cinematic fountain restoration VFX
            // (placeholder: spawn particle system at fountain center)
            var fountain = new GameObject("Moon2_FountainPurgeVFX");
            fountain.transform.position = bellTowerCenter;
            var particles = fountain.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startColor = new Color(0.6f, 0.9f, 1f, 1f);  // Cyan purification
            main.startSize = 3f;
            main.startLifetime = 5f;
            main.maxParticles = 500;

            // Audio: restoration harmonic
            Audio.AudioManager.Instance?.PlaySFX2D("Moon2_RestoreHarmonic");

            // Quest complete
            // TODO: QuestManager.CompleteQuest is private

            // Unlock Moon 3
            // TODO: HUDController not implemented
            SaveManager.Instance?.SetMoonProgress(2, 100f);

            // Lirael whisper
            DialogueManager.Instance?.PlayContextDialogue("lirael_moon2_complete");
        }

        /// <summary>
        /// Called by save system to restore Moon 2 state.
        /// </summary>
        public void LoadState(bool unlocked, int crystalsDestroyed, bool fountainComplete)
        {
            moon2Unlocked = unlocked;
            _crystalsDestroyed = crystalsDestroyed;
            fountainPurgeComplete = fountainComplete;

            if (moon2Unlocked && !_contentSpawned)
            {
                SpawnMoon2Content();
            }

            // Destroy crystals that were already cleared
            for (int i = 0; i < _crystalsDestroyed && i < _activeCrystals.Count; i++)
            {
                if (_activeCrystals[i] != null)
                {
                    Destroy(_activeCrystals[i]);
                }
            }
        }
    }

    /// <summary>
    /// Dissonance crystal component — handles purge interaction + destruction.
    /// </summary>
    public class DissonanceCrystal : MonoBehaviour, IInteractable
    {
        public event System.Action<DissonanceCrystal> OnDestroyed;

        float _health = 100f;
        bool _isBeingPurged;

        public string GetInteractPrompt() => "Hold [E] to Purge Dissonance";

        public void Interact(GameObject interactor)
        {
            if (_isBeingPurged) return;
            _isBeingPurged = true;

            // Start purge sequence (reverse tuning puzzle for beta)
            StartPurge();
        }

        void StartPurge()
        {
            // For beta: instant destroy with VFX (full impl: tuning minigame)
            _health = 0f;

            // VFX: crystal shatters
            var particles = gameObject.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startColor = new Color(1f, 0.3f, 0.3f);  // Red shatter
            main.startSize = 0.5f;
            main.startLifetime = 1f;
            main.maxParticles = 50;
            particles.Play();

            // Audio
            Audio.AudioManager.Instance?.PlaySFX2D("CrystalShatter");

            // Notify spawner
            OnDestroyed?.Invoke(this);

            // Destroy after VFX
            Destroy(gameObject, 1.5f);
        }
    }
}
