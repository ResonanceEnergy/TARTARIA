using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Audio;
using Tartaria.Input;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 3 Orphan Train Puzzle — 13 rail segment activations to complete the Continental Rail route.
    /// Player must restore each segment via cymatic tuning + protect from wrong-note dissonance spawns.
    /// 
    /// Per GDD §03 Moon 3 "Restoration" beat (Days 6-12):
    /// - 13 dormant rail segments need activation
    /// - Each segment: tune resonance frequency → rail lights golden
    /// - Wrong notes spawn dissonance enemies (Mud Golems)
    /// - All 13 complete → Orphan Train solidifies, lullaby climax unlocked
    /// 
    /// Wires to RailEscortController + Moon3RailAudioManager + VFXController.
    /// </summary>
    [DisallowMultipleComponent]
    public class Moon3OrphanTrainPuzzle : MonoBehaviour
    {
        public static Moon3OrphanTrainPuzzle Instance { get; private set; }

        [Header("Puzzle Configuration")]
        [SerializeField] int totalSegments = 13;
        [SerializeField] Vector3[] segmentPositions;  // Set via editor or procedural
        [SerializeField] GameObject mudGolemPrefab;   // Enemy prefab for wrong-note spawns

        [Header("State")]
        int _segmentsActivated;
        readonly HashSet<string> _activatedSegmentIds = new HashSet<string>();
        readonly List<GameObject> _activeSegments = new List<GameObject>();

        public int SegmentsRemaining => totalSegments - _segmentsActivated;
        public float PuzzleProgress => _segmentsActivated / (float)totalSegments;
        public bool IsComplete => _segmentsActivated >= totalSegments;

        public event System.Action<int> OnSegmentActivated;  // passes remaining count
        public event System.Action OnPuzzleComplete;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>
        /// Initializes the puzzle, spawning all 13 rail segments along the Continental Rail route.
        /// Called by Moon3ContentSpawner when Moon 3 unlocks.
        /// </summary>
        public void InitializePuzzle(Vector3 railStart)
        {
            if (_activeSegments.Count > 0) return; // Already initialized

            // Generate segment positions if not set (linear rail path)
            if (segmentPositions == null || segmentPositions.Length == 0)
            {
                segmentPositions = GenerateSegmentPositions(railStart);
            }

            // Spawn all 13 segments
            for (int i = 0; i < totalSegments; i++)
            {
                var segment = CreateRailSegment(segmentPositions[i], i);
                _activeSegments.Add(segment);
            }

            Debug.Log($"[Moon3 Train Puzzle] Initialized: {totalSegments} rail segments along Continental Rail route");

            // Audio: dormant rail hum
            AudioManager.Instance?.PlayLoopingSFX("Moon3_RailDormantHum", railStart, 0.3f);
        }

        Vector3[] GenerateSegmentPositions(Vector3 start)
        {
            var positions = new Vector3[totalSegments];

            // Linear rail path with gentle curves
            for (int i = 0; i < totalSegments; i++)
            {
                float distance = i * 20f;  // 20m spacing
                float curve = Mathf.Sin(i * 0.4f) * 5f;  // Gentle S-curve

                positions[i] = start + new Vector3(
                    curve,
                    0f,
                    distance
                );
            }

            return positions;
        }

        GameObject CreateRailSegment(Vector3 position, int index)
        {
            var segment = new GameObject($"RailSegment_{index:D2}");
            segment.transform.position = position;
            segment.transform.rotation = Quaternion.Euler(0, 0, 0);

            // Visual: dormant rail ties (gray metallic)
            var filter = segment.AddComponent<MeshFilter>();
            filter.mesh = CreateRailMesh();

            var renderer = segment.AddComponent<MeshRenderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            renderer.material.color = new Color(0.3f, 0.3f, 0.35f);  // Dormant gray
            renderer.material.SetFloat("_Metallic", 0.7f);

            // Collider
            var collider = segment.AddComponent<BoxCollider>();
            collider.size = new Vector3(3f, 0.5f, 4f);

            // Component for activation
            var segmentComp = segment.AddComponent<OrphanTrainSegment>();
            segmentComp.segmentId = $"segment_{index:D2}";
            segmentComp.segmentIndex = index;
            segmentComp.OnActivated += HandleSegmentActivated;
            segmentComp.OnWrongNote += HandleWrongNote;

            // Dim light (unactivated)
            var light = segment.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.5f, 0.5f, 0.6f);  // Dim blue-white
            light.range = 3f;
            light.intensity = 0.3f;

            return segment;
        }

        Mesh CreateRailMesh()
        {
            // Procedural rail ties (simple beveled rectangle)
            var mesh = new Mesh();
            var vertices = new Vector3[]
            {
                // Top face (ties)
                new(-1.5f, 0.25f, -2f), new(1.5f, 0.25f, -2f), new(1.5f, 0.25f, 2f), new(-1.5f, 0.25f, 2f),
                // Bottom face
                new(-1.5f, 0f, -2f), new(1.5f, 0f, -2f), new(1.5f, 0f, 2f), new(-1.5f, 0f, 2f),
                // Rails (two parallel strips)
                new(-1.2f, 0.3f, -2f), new(-0.8f, 0.3f, -2f), new(-0.8f, 0.3f, 2f), new(-1.2f, 0.3f, 2f),
                new(0.8f, 0.3f, -2f), new(1.2f, 0.3f, -2f), new(1.2f, 0.3f, 2f), new(0.8f, 0.3f, 2f)
            };

            var triangles = new int[]
            {
                // Top
                0, 1, 2,  0, 2, 3,
                // Bottom
                4, 6, 5,  4, 7, 6,
                // Rails
                8, 9, 10,  8, 10, 11,
                12, 13, 14,  12, 14, 15
            };

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        void HandleSegmentActivated(OrphanTrainSegment segment)
        {
            if (_activatedSegmentIds.Contains(segment.segmentId)) return;  // Already activated

            _activatedSegmentIds.Add(segment.segmentId);
            _segmentsActivated++;

            Debug.Log($"[Moon3 Train Puzzle] Segment activated: {segment.segmentId} ({SegmentsRemaining} remaining)");

            // VFX: rail turns golden, electric sparks
            if (VFXController.Instance != null)
            {
                VFXController.Instance.PlayAetherPulse(segment.transform.position, 1.2f, Color.blue);
            }

            // Audio: rail activation chime (432 Hz harmonic)
            AudioManager.Instance?.PlaySFX2D("Moon3_RailActivate");
            HapticFeedbackManager.Instance?.PlayDiscovery();

            // Progress feedback
            OnSegmentActivated?.Invoke(SegmentsRemaining);

            // Quest progress
            QuestManager.Instance?.ProgressByType(QuestObjectiveType.TalkToNPC /*was ActivateRail*/, segment.segmentId);

            // Notify rail escort controller
            if (RailEscortController.Instance != null)
            {
                RailEscortController.Instance.OnRailSegmentReactivated(_segmentsReactivated);
            }

            // Check completion
            if (IsComplete)
            {
                CompletePuzzle();
            }
        }

        void HandleWrongNote(OrphanTrainSegment segment)
        {
            Debug.Log($"[Moon3 Train Puzzle] Wrong note on {segment.segmentId} — spawning dissonance enemy!");

            // Spawn Mud Golem (dissonance defender)
            SpawnDissonanceEnemy(segment.transform.position);

            // Audio: harsh dissonance screech
            AudioManager.Instance?.PlaySFX2D("Moon3_WrongNote");
            HapticFeedbackManager.Instance?.PlayCombatHit();

            // HUD warning
            if (UI.HUDController.Instance != null)
            {
                UI.HUDController.Instance.ShowBanner(
                    "Wrong Note!",
                    "Dissonance manifests — the rails reject false frequencies!",
                    2f
                );
            }
        }

        void SpawnDissonanceEnemy(Vector3 position)
        {
            if (mudGolemPrefab != null)
            {
                var golem = Instantiate(mudGolemPrefab, position + Vector3.up * 2f, Quaternion.identity);
                golem.name = "MudGolem_WrongNote";
                Debug.Log($"[Moon3 Train Puzzle] Spawned Mud Golem at {position}");
            }
            else
            {
                Debug.LogWarning("[Moon3 Train Puzzle] MudGolem prefab not assigned, skipping enemy spawn");

                // Fallback: create primitive golem
                var golem = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                golem.transform.position = position + Vector3.up * 1f;
                golem.transform.localScale = new Vector3(1.2f, 1.5f, 1.2f);
                golem.GetComponent<Renderer>().material.color = new Color(0.3f, 0.2f, 0.15f);
                golem.name = "MudGolem_Placeholder_WrongNote";
            }
        }

        void CompletePuzzle()
        {
            Debug.Log("[Moon3 Train Puzzle] ★ COMPLETE ★ All 13 rail segments activated! Orphan Train ready for lullaby climax.");

            // Audio: rail resonance swell (all segments harmonize)
            AudioManager.Instance?.StopLoopingSFX("Moon3_RailDormantHum");
            AudioManager.Instance?.PlaySFX2D("Moon3_PuzzleComplete");

            // VFX: golden wave travels entire rail length
            if (VFXController.Instance != null)
            {
                VFXController.Instance.PlayAetherPulse(transform.position, 8f, Color.green);
            }

            // Haptic celebration
            HapticFeedbackManager.Instance?.PlayDiscovery();

            // HUD banner
            if (UI.HUDController.Instance != null)
            {
                UI.HUDController.Instance.ShowBanner(
                    "Continental Rail Restored",
                    "Every segment sings. The children can finally go home. The lullaby begins.",
                    7f
                );
            }

            // Fire completion event
            OnPuzzleComplete?.Invoke();

            // Achievement
            AchievementSystem.Instance?.Unlock("moon3_rail_master");

            // Notify escort controller
            if (RailEscortController.Instance != null)
            {
                RailEscortController.Instance.OnRailSegmentReactivated();  // Final call
            }
        }

        /// <summary>
        /// Save/load support: restore activated segment IDs.
        /// </summary>
        public void LoadState(HashSet<string> activatedIds)
        {
            _activatedSegmentIds.Clear();
            foreach (var id in activatedIds)
            {
                _activatedSegmentIds.Add(id);
            }

            _segmentsActivated = _activatedSegmentIds.Count;

            // Update already-activated segments (visual state)
            foreach (var segment in _activeSegments)
            {
                if (segment != null)
                {
                    var comp = segment.GetComponent<OrphanTrainSegment>();
                    if (comp != null && _activatedSegmentIds.Contains(comp.segmentId))
                    {
                        comp.SetActivatedVisuals();
                    }
                }
            }

            Debug.Log($"[Moon3 Train Puzzle] Loaded state: {_segmentsActivated}/{totalSegments} segments activated");
        }

        public HashSet<string> SaveState()
        {
            return new HashSet<string>(_activatedSegmentIds);
        }
    }

    /// <summary>
    /// Individual orphan train rail segment component — handles cymatic tuning interaction + activation.
    /// Renamed from RailSegment to OrphanTrainSegment to avoid collision with ContinentalRailSystem.RailSegment.
    /// </summary>
    public class OrphanTrainSegment : MonoBehaviour, IInteractable
    {
        public string segmentId;
        public int segmentIndex;

        public event System.Action<OrphanTrainSegment> OnActivated;
        public event System.Action<OrphanTrainSegment> OnWrongNote;

        bool _isActivated;
        bool _isTuning;

        public string GetInteractPrompt()
        {
            if (_isActivated) return "Rail Segment Active (432 Hz)";
            return "[E] Tune Rail Resonance";
        }

        public void Interact(GameObject interactor)
        {
            if (_isActivated) return;
            if (_isTuning) return;

            _isTuning = true;
            StartCoroutine(TuningSequence());
        }

        IEnumerator TuningSequence()
        {
            Debug.Log($"[RailSegment {segmentId}] Tuning sequence started");

            // For beta: simplified 3-note sequence (full impl: cymatic minigame)
            // In production: CymaticWaterTuningMiniGame.Instance.StartMiniGame()

            // Show tuning UI prompt
            if (UI.HUDController.Instance != null)
            {
                UI.HUDController.Instance.ShowBanner(
                    "Tuning Rail Segment",
                    "Match the resonance frequency: 432 Hz...",
                    3f
                );
            }

            // Simulate tuning duration
            yield return new WaitForSeconds(2f);

            // Random success/fail for beta (80% success)
            bool success = Random.value > 0.2f;

            if (success)
            {
                // Success: activate rail segment
                _isActivated = true;
                _isTuning = false;

                SetActivatedVisuals();

                // Audio: harmonic chime
                AudioManager.Instance?.PlaySFX2D("CymaticSuccess");

                // Notify puzzle controller
                OnActivated?.Invoke(this);
            }
            else
            {
                // Fail: wrong note, spawn enemy
                _isTuning = false;

                // Audio: dissonance screech
                AudioManager.Instance?.PlaySFX2D("CymaticFail");

                // Notify puzzle controller
                OnWrongNote?.Invoke(this);
            }
        }

        public void SetActivatedVisuals()
        {
            // Change rail color to golden
            var renderer = GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(1f, 0.9f, 0.5f);  // Golden
                renderer.material.EnableKeyword("_EMISSION");
                renderer.material.SetColor("_EmissionColor", new Color(1f, 0.9f, 0.5f) * 0.5f);
            }

            // Brighten light
            var light = GetComponent<Light>();
            if (light != null)
            {
                light.color = new Color(1f, 0.9f, 0.5f);  // Golden
                light.intensity = 1.5f;
                light.range = 6f;
            }

            // Particle effects: golden sparks
            var particles = gameObject.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startColor = new Color(1f, 0.9f, 0.5f);
            main.startSize = 0.2f;
            main.startLifetime = 1f;
            main.maxParticles = 20;
            main.loop = true;
        }
    }
}
