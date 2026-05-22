using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Gameplay;
using Tartaria.Input;
using Tartaria.Save;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 3 (Windswept Highlands) content spawner — resonance trains + orphan adoption.
    ///
    /// Design per GDD §03:
    ///   - Spectral Orphan Train materializes on dormant rails (translucent, humming sadly)
    ///   - Aboard: ghost children in Victorian clothing — Dissonant Orphan Train
    ///   - Lirael backstory reveal: "I remember this train. I was on it. We sang to keep the mud away… but the song broke."
    ///   - Reactivate first resonance rail segment (giant mode rock cutting, precision rail ties)
    ///   - Tune cymatic gardens to free spectral children → junior architects (auto-build during offline)
    ///   - Train derailment ambush → protect children while repairing tracks
    ///   - Children sing 432 Hz lullaby → reactivates entire rail, train solidifies golden
    ///   - Orphan Train Lullaby Crystal drops → passive 432 Hz healing zone
    ///
    /// Activates when player completes Moon 2 (Lunar Moon dissonance purge).
    /// </summary>
    public class Moon3ContentSpawner : MonoBehaviour
    {
        public static Moon3ContentSpawner Instance { get; private set; }

        [Header("Content State")]
        [SerializeField] bool moon3Unlocked = false;
        [SerializeField] bool trainMaterialized = false;
        [SerializeField] bool railSegmentReactivated = false;
        [SerializeField] bool lullabyClimaxComplete = false;

        [Header("Orphan Train")]
        [SerializeField] int totalOrphans = 8;  // Per GDD: ~8 spectral children to adopt
        int _orphansFreed;

        [Header("Rail Segments")]
        [SerializeField] int totalRailSegments = 5;  // 5 segments to reactivate
        int _segmentsReactivated;

        [Header("Spawning")]
        [SerializeField] Vector3 trainSpawnPoint = new(50f, 2f, 30f);  // Windswept Highlands rail junction
        [SerializeField] Vector3[] cymaticGardenPoints;  // Set via editor or runtime
        [SerializeField] Vector3[] railSegmentStarts;  // 5 rail tie positions
        [SerializeField] GameObject mudGolemPrefab;  // Mud Golem enemy prefab (assign in editor)

        [Header("Puzzle Systems")]
        Gameplay.Moon3OrphanTrainPuzzle _trainPuzzle;

        GameObject _spectralTrain;
        readonly List<GameObject> _cymaticGardens = new();
        readonly List<GameObject> _adoptedOrphans = new();
        readonly List<GameObject> _spawnedGolems = new();
        bool _contentSpawned;
        bool _derailmentTriggered;

        public bool IsMoon3Active => moon3Unlocked && !lullabyClimaxComplete;
        public int OrphansRemaining => totalOrphans - _orphansFreed;
        public float OrphanProgress => _orphansFreed / (float)totalOrphans;
        public float RailProgress => _segmentsReactivated / (float)totalRailSegments;

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
            // Check if Moon 3 should auto-unlock (Moon 2 complete)
            if (SaveManager.Instance != null)
            {
                var moonProgress = SaveManager.Instance.GetMoonProgress(2);  // Moon 2
                if (moonProgress >= 100f && !moon3Unlocked)
                {
                    UnlockMoon3();
                }
            }
        }

        // ─── Public API ───────────────────────────────

        /// <summary>
        /// Unlocks Moon 3 content (called when Moon 2 complete).
        /// </summary>
        public void UnlockMoon3()
        {
            if (moon3Unlocked) return;

            moon3Unlocked = true;
            Debug.Log("[Moon 3] Electric Moon unlocked — The Spark of Service begins");

            SpawnMoon3Content();

            // Tutorial hint
                    }

        /// <summary>
        /// Spawns all Moon 3 content: spectral train, cymatic gardens, rail segments.
        /// </summary>
        void SpawnMoon3Content()
        {
            if (_contentSpawned) return;
            _contentSpawned = true;

            // Spawn spectral Orphan Train
            if (!trainMaterialized)
            {
                SpawnSpectralTrain();
            }

            // Spawn cymatic gardens (8 garden orbs to tune)
            SpawnCymaticGardens();

            // Initialize orphan train puzzle (13 rail segments)
            InitializeTrainPuzzle();

            // Activate sad train ambient audio (spectral train whistle + distant crying)
            var trainAmbience = Audio.AudioManager.Instance?.PlayLoopingSFX("SpectralTrainWhistle", trainSpawnPoint, 0.35f);
            if (trainAmbience != null)
            {
                Debug.Log("[Moon 3] Spectral train ambience active"); + 13 rail segments");
        }

        void InitializeTrainPuzzle()
        {
            // Create train puzzle system
            var puzzleGO = new GameObject("Moon3_TrainPuzzle");
            puzzleGO.transform.position = trainSpawnPoint;
            _trainPuzzle = puzzleGO.AddComponent<Gameplay.Moon3OrphanTrainPuzzle>();
            _trainPuzzle.InitializePuzzle(trainSpawnPoint);

            // Subscribe to completion
            _trainPuzzle.OnPuzzleComplete += () =>
            {
                Debug.Log("[Moon 3] Train puzzle complete → lullaby climax ready");
                // May trigger lullaby climax if all orphans also freed
                if (_orphansFreed >= totalOrphans && _segmentsReactivated >= totalRailSegments && !lullabyClimaxComplete)
                {
                    TriggerLullabyClimax();
                }
            }
            }

            Debug.Log($"[Moon 3] Spawned spectral Orphan Train + {totalOrphans} cymatic gardens");
        }

        void SpawnSpectralTrain()
        {
            // Create spectral Orphan Train
            _spectralTrain = new GameObject("SpectralOrphanTrain");
            _spectralTrain.transform.position = trainSpawnPoint;

            // Visual: translucent train (placeholder cube for beta)
            var filter = _spectralTrain.AddComponent<MeshFilter>();
            filter.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            var renderer = _spectralTrain.AddComponent<MeshRenderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            renderer.material.color = new Color(0.6f, 0.7f, 0.9f, 0.4f);  // Translucent blue-white
            renderer.material.SetFloat("_Surface", 1);  // Transparent mode
            renderer.material.SetFloat("_Blend", 0);  // Alpha blend

            // Scale to train size (placeholder)
            _spectralTrain.transform.localScale = new Vector3(3f, 2f, 12f);  // Train car dimensions

            // Collider + interaction
            var collider = _spectralTrain.AddComponent<BoxCollider>();
            collider.size = Vector3.one;

            // Train interaction trigger
            var trainInteract = _spectralTrain.AddComponent<OrphanTrainInteract>();

            trainMaterialized = true;
            Debug.Log("[Moon 3] Spectral Orphan Train materialized on dormant rails");

            // Lirael dialogue: "I remember this train..."
            DialogueManager.Instance?.PlayContextDialogue("lirael_moon3_train_memory");

            // Ghostly children crying SFX
            Audio.AudioManager.Instance?.PlaySFX2D("Moon3_ChildrenCrying");
        }

        void SpawnCymaticGardens()
        {
            // Generate garden positions if not set manually
            if (cymaticGardenPoints == null || cymaticGardenPoints.Length == 0)
            {
                cymaticGardenPoints = GenerateGardenPositions(totalOrphans);
            }

            for (int i = 0; i < Mathf.Min(totalOrphans, cymaticGardenPoints.Length); i++)
            {
                var garden = CreateCymaticGarden(cymaticGardenPoints[i], i);
                _cymaticGardens.Add(garden);
            }
        }

        GameObject CreateCymaticGarden(Vector3 position, int index)
        {
            var garden = new GameObject($"CymaticGarden_{index:D2}");
            garden.transform.position = position;

            // Visual: glowing orb (procedural sphere for beta)
            var filter = garden.AddComponent<MeshFilter>();
            filter.mesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");

            var renderer = garden.AddComponent<MeshRenderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            renderer.material.color = new Color(0.8f, 0.9f, 0.5f, 0.7f);  // Soft yellow-green
            renderer.material.SetFloat("_Surface", 1);  // Transparent
            renderer.material.SetFloat("_Blend", 0);

            // Scale
            garden.transform.localScale = Vector3.one * 1.5f;

            // Collider + interaction
            var collider = garden.AddComponent<SphereCollider>();
            collider.radius = 0.75f;

            // Add CymaticGarden component (handles tuning interaction)
            var gardenComp = garden.AddComponent<CymaticGarden>();
            gardenComp.OnOrphanFreed += OnOrphanFreed;

            // Pulsing light VFX
            var light = garden.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.8f, 0.9f, 0.5f);
            light.range = 5f;
            light.intensity = 1.2f;

            return garden;
        }

        Vector3[] GenerateGardenPositions(int count)
        {
            var positions = new Vector3[count];
            var basePos = trainSpawnPoint + Vector3.forward * 20f;  // Ahead of train

            for (int i = 0; i < count; i++)
            {
                // Scatter along rail path (rough line for beta)
                float offset = i * 8f;
                positions[i] = basePos + new Vector3(
                    Random.Range(-3f, 3f),  // Slight lateral variance
                    0.5f,
                    offset
                );
            }

            return positions;
        }

        void OnOrphanFreed(CymaticGarden garden)
        {
            _orphansFreed++;

            Debug.Log($"[Moon 3] Orphan freed via cymatic tuning ({_orphansFreed}/{totalOrphans})");

            // VFX + audio feedback
            Audio.AudioManager.Instance?.PlaySFX2D("Moon3_OrphanFreed");
            Input.HapticFeedbackManager.Instance?.PlayDiscovery();

            // Spawn adopted orphan NPC (junior architect)
            SpawnAdoptedOrphan(garden.transform.position);

            // Progress tracking
            QuestManager.Instance?.ProgressByType(QuestObjectiveType.TalkToNPC /*was FreeOrphans*/, garden.gameObject.name);

            // Check if all orphans freed + rail segments reactivated
            if (_orphansFreed >= totalOrphans && _segmentsReactivated >= totalRailSegments && !lullabyClimaxComplete)
            {
                TriggerLullabyClimax();
            }
            else if (_orphansFreed >= totalOrphans / 2 && !_derailmentTriggered)
            {
                // Mid-escort derailment (Days 13-18 conflict)
                TriggerDerailmentAmbush();
            }
        }

        void SpawnAdoptedOrphan(Vector3 position)
        {
            var orphan = new GameObject($"AdoptedOrphan_{_orphansFreed:D2}");
            orphan.transform.position = position + Vector3.up * 0.5f;

            // Visual: small humanoid capsule (child-sized)
            var filter = orphan.AddComponent<MeshFilter>();
            filter.mesh = Resources.GetBuiltinResource<Mesh>("Capsule.fbx");
            var renderer = orphan.AddComponent<MeshRenderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            renderer.material.color = new Color(0.9f, 0.85f, 0.7f);  // Warm skin tone

            orphan.transform.localScale = new Vector3(0.4f, 0.7f, 0.4f);  // Child proportions

            // Follow player (simple "junior architect" behavior)
            var follow = orphan.AddComponent<FollowPlayer>();

            _adoptedOrphans.Add(orphan);

            // Orphan child dialogue
            DialogueManager.Instance?.PlayContextDialogue("orphan_child_help");
        }

        public void OnRailSegmentReactivated()
        {
            _segmentsReactivated++;
            Debug.Log($"[Moon 3] Rail segment reactivated ({_segmentsReactivated}/{totalRailSegments})");

            // VFX: rail sparks
            Audio.AudioManager.Instance?.PlaySFX2D("Moon3_RailHum");

            // Check for lullaby climax trigger
            if (_orphansFreed >= totalOrphans && _segmentsReactivated >= totalRailSegments && !lullabyClimaxComplete)
            {
                TriggerLullabyClimax();
            }
        }

        void TriggerDerailmentAmbush()
        {
            _derailmentTriggered = true;

            Debug.Log("[Moon 3] Derailment ambush triggered — Reset agents planted dissonance!");

            // Spawn Mud Golems along tracks (3 enemies)
            if (mudGolemPrefab != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    var spawnPos = trainSpawnPoint + Vector3.forward * (i * 10f);
                    var golem = Instantiate(mudGolemPrefab, spawnPos, Quaternion.identity);
                    golem.name = $"MudGolem_Derailment_{i}";
                    _spawnedGolems.Add(golem);
                    Debug.Log($"[Moon 3] Spawned Mud Golem at {spawnPos}");
                }
            }
            else
            {
                Debug.LogWarning("[Moon 3] MudGolem prefab not assigned, skipping enemy spawn");
            }

            // Children scream in spectral echoes
            Audio.AudioManager.Instance?.PlaySFX2D("Moon3_ChildrenScream");

            // Quest objective: protect children + repair track
            QuestManager.Instance?.ActivateQuest("moon3_derailment_defense");

                    }

        /// <summary>
        /// Triggers climax event: children sing 432 Hz lullaby → train solidifies (Days 19-24).
        /// </summary>
        void TriggerLullabyClimax()
        {
            lullabyClimaxComplete = true;

            Debug.Log("[Moon 3] Lullaby Climax — Children sing, train solidifies golden!");

            // Cinematic: children gather around train
            foreach (var orphan in _adoptedOrphans)
            {
                if (orphan != null)
                {
                    orphan.transform.position = trainSpawnPoint + Vector3.back * 5f;  // Behind train
                }
            }

            // Children sing lullaby (432 Hz harmonic)
            Audio.AudioManager.Instance?.PlaySFX2D("Moon3_LullabyHarmonic");

            // Train solidifies: change material from translucent → golden opaque
            if (_spectralTrain != null)
            {
                var renderer = _spectralTrain.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.material.color = new Color(1f, 0.9f, 0.5f, 1f);  // Golden solid
                    renderer.material.SetFloat("_Surface", 0);  // Opaque mode
                }
            }

            // Golden rail VFX (entire rail segment lights up)
            var railVFX = new GameObject("Moon3_GoldenRailVFX");
            railVFX.transform.position = trainSpawnPoint;
            var particles = railVFX.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startColor = new Color(1f, 0.9f, 0.5f, 1f);  // Golden
            main.startSize = 2f;
            main.startLifetime = 8f;
            main.maxParticles = 1000;

            // Orphan Train Lullaby Crystal drops (permanent buff)
            GrantLullabyBuff();

            // Quest complete
            QuestManager.Instance?.CompleteQuest("quest_complete");

            // Unlock Moon 4
                        SaveManager.Instance?.SetMoonProgress(3, 100f);

            // Lirael revelation dialogue
            DialogueManager.Instance?.PlayContextDialogue("lirael_moon3_revelation");
        }

        void GrantLullabyBuff()
        {
            // Orphan Train Lullaby Crystal: passive 432 Hz healing zone
            Debug.Log("[Moon 3] Orphan Train Lullaby Crystal granted — passive healing aura");, System.Collections.Generic.HashSet<string> trainPuzzleState = null)
        {
            moon3Unlocked = unlocked;
            _orphansFreed = orphansFreed;
            _segmentsReactivated = railSegments;
            lullabyClimaxComplete = climaxComplete;

            if (moon3Unlocked && !_contentSpawned)
            {
                SpawnMoon3Content();
            }

            // Destroy gardens that were already tuned
            for (int i = 0; i < _orphansFreed && i < _cymaticGardens.Count; i++)
            {
                if (_cymaticGardens[i] != null)
                {
                    Destroy(_cymaticGardens[i]);
                }
            }

            // Restore train puzzle state
            if (_trainPuzzle != null && trainPuzzleState != null)
            {
                _trainPuzzle.LoadState(trainPuzzleState);
            }
        }

        /// <summary>
        /// Save train puzzle state (for save system integration).
        /// </summary>
        public System.Collections.Generic.HashSet<string> GetTrainPuzzleState()
        {
            return _trainPuzzle != null ? _trainPuzzle.SaveState() : new System.Collections.Generic.HashSet<string>();
        /// <summary>
        /// Called by save system to restore Moon 3 state.
        /// </summary>
        public void LoadState(bool unlocked, int orphansFreed, int railSegments, bool climaxComplete)
        {
            moon3Unlocked = unlocked;
            _orphansFreed = orphansFreed;
            _segmentsReactivated = railSegments;
            lullabyClimaxComplete = climaxComplete;

            if (moon3Unlocked && !_contentSpawned)
            {
                SpawnMoon3Content();
            }

            // Destroy gardens that were already tuned
            for (int i = 0; i < _orphansFreed && i < _cymaticGardens.Count; i++)
            {
                if (_cymaticGardens[i] != null)
                {
                    Destroy(_cymaticGardens[i]);
                }
            }
        }
    }

    /// <summary>
    /// Cymatic garden component — handles tuning interaction to free orphans.
    /// </summary>
    public class CymaticGarden : MonoBehaviour, IInteractable
    {
        public event System.Action<CymaticGarden> OnOrphanFreed;

        bool _isBeingTuned;

        public string GetInteractPrompt() => "Hold [E] to Tune Cymatic Garden";

        public void Interact(GameObject interactor)
        {
            if (_isBeingTuned) return;
            _isBeingTuned = true;

            // Start tuning sequence (simple 3-note sequence for beta)
            StartTuning();
        }

        void StartTuning()
        {
            // For beta: instant free with VFX (full impl: tuning minigame)
            // VFX: garden bursts into golden particles
            var particles = gameObject.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startColor = new Color(1f, 0.9f, 0.5f);  // Golden
            main.startSize = 0.8f;
            main.startLifetime = 2f;
            main.maxParticles = 80;
            particles.Play();

            // Audio
            Audio.AudioManager.Instance?.PlaySFX2D("CymaticTune");

            // Notify spawner
            OnOrphanFreed?.Invoke(this);

            // Destroy after VFX
            Destroy(gameObject, 2.5f);
        }
    }

    /// <summary>
    /// Orphan Train interaction trigger — shows Lirael dialogue on first approach.
    /// </summary>
    public class OrphanTrainInteract : MonoBehaviour, IInteractable
    {
        bool _firstInteraction = true;

        public string GetInteractPrompt() => "Approach the Spectral Train";

        public void Interact(GameObject interactor)
        {
            if (_firstInteraction)
            {
                _firstInteraction = false;
                DialogueManager.Instance?.PlayContextDialogue("lirael_moon3_train_backstory");
                Audio.AudioManager.Instance?.PlaySFX2D("Moon3_TrainWhisper");
            }
        }
    }

    /// <summary>
    /// Simple follow player behavior for adopted orphans (junior architects).
    /// </summary>
    public class FollowPlayer : MonoBehaviour
    {
        float _speed = 2.5f;
        float _stopDistance = 3f;

        void Update()
        {
            var player = FindFirstObjectByType<CharacterController>();
            if (player == null) return;

            float dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist > _stopDistance)
            {
                Vector3 dir = (player.transform.position - transform.position).normalized;
                transform.position += dir * (_speed * Time.deltaTime);
                transform.LookAt(player.transform.position);
            }
        }
    }
}


