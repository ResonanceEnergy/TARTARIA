using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Audio;
using Tartaria.Input;

#pragma warning disable CS0067  // Event never used
#pragma warning disable CS0219  // Variable assigned but not used
#pragma warning disable CS0414  // Field assigned but not used
namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 2 Dissonance Vein Destruction Puzzle — 12 black corruption veins in cathedral fractal chambers.
    /// Player must destroy all 12 in micro-giant mode to unlock cathedral restoration climax.
    /// 
    /// Per GDD §03 Moon 2 "Conflict" beat (Days 13-18):
    /// - Dissonance veins appear in tight fractal corridors
    /// - Requires micro-giant shrink to access
    /// - Each vein destruction weakens corruption field
    /// - All 12 destroyed → fountain purge climax unlocked
    /// 
    /// Wires to Moon2ProgressionSystem + Moon2AtmosphereAudioManager + VFXController.
    /// </summary>
    [DisallowMultipleComponent]
    public class Moon2DissonanceVeinPuzzle : MonoBehaviour
    {
        public static Moon2DissonanceVeinPuzzle Instance { get; private set; }

        [Header("Puzzle Configuration")]
        [SerializeField] int totalVeins = 12;
        [SerializeField] Vector3[] veinPositions;  // Set via editor or procedural

        [Header("State")]
        int _veinsDestroyed;
        readonly HashSet<string> _destroyedVeinIds = new HashSet<string>();
        readonly List<GameObject> _activeVeins = new List<GameObject>();

        public int VeinsRemaining => totalVeins - _veinsDestroyed;
        public float PuzzleProgress => _veinsDestroyed / (float)totalVeins;
        public bool IsComplete => _veinsDestroyed >= totalVeins;

        public event System.Action<int> OnVeinDestroyed;  // passes remaining count
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
        /// Initializes the puzzle, spawning all 12 dissonance veins in fractal cathedral chambers.
        /// Called by Moon2ContentSpawner when Moon 2 unlocks.
        /// </summary>
        public void InitializePuzzle(Vector3 cathedralCenter)
        {
            if (_activeVeins.Count > 0) return; // Already initialized

            // Generate vein positions if not set (fractal ring pattern)
            if (veinPositions == null || veinPositions.Length == 0)
            {
                veinPositions = GenerateVeinPositions(cathedralCenter);
            }

            // Spawn all 12 veins
            for (int i = 0; i < totalVeins; i++)
            {
                var vein = CreateDissonanceVein(veinPositions[i], i);
                _activeVeins.Add(vein);
            }

            Debug.Log($"[Moon2 Vein Puzzle] Initialized: {totalVeins} dissonance veins spawned in fractal cathedral chambers");

            // Audio: corruption ambience starts
            AudioManager.Instance?.PlayLoopingSFX("Moon2_CorruptionDrone", cathedralCenter, 0.5f);
        }

        Vector3[] GenerateVeinPositions(Vector3 center)
        {
            var positions = new Vector3[totalVeins];

            // 3 rings of veins (4 per ring) at different heights in cathedral
            for (int i = 0; i < totalVeins; i++)
            {
                int ring = i / 4;  // 0, 1, 2
                int indexInRing = i % 4;

                float angle = (indexInRing / 4f) * Mathf.PI * 2f;
                float radius = 8f + ring * 4f;  // Expand outward
                float height = 3f + ring * 5f;  // Stack vertically

                positions[i] = center + new Vector3(
                    Mathf.Cos(angle) * radius,
                    height,
                    Mathf.Sin(angle) * radius
                );
            }

            return positions;
        }

        GameObject CreateDissonanceVein(Vector3 position, int index)
        {
            var vein = new GameObject($"DissonanceVein_{index:D2}");
            vein.transform.position = position;
            vein.transform.rotation = Quaternion.Euler(Random.Range(-30f, 30f), Random.Range(0f, 360f), Random.Range(-30f, 30f));

            // Visual: black crystalline vein (jagged tube)
            var filter = vein.AddComponent<MeshFilter>();
            filter.mesh = CreateVeinMesh();

            var renderer = vein.AddComponent<MeshRenderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            renderer.material.color = new Color(0.02f, 0.02f, 0.02f);  // Pure black
            renderer.material.SetFloat("_Metallic", 0.9f);
            renderer.material.SetFloat("_Smoothness", 0.3f);

            // Collider
            var collider = vein.AddComponent<CapsuleCollider>();
            collider.radius = 0.4f;
            collider.height = 2.5f;
            collider.direction = 1;  // Y-axis

            // Component for destruction
            var veinComp = vein.AddComponent<DissonanceVein>();
            veinComp.veinId = $"vein_{index:D2}";
            veinComp.OnDestroyed += HandleVeinDestroyed;

            // Pulsing red light (corruption indicator)
            var light = vein.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.8f, 0.1f, 0.1f);  // Dark red
            light.range = 4f;
            light.intensity = 0.7f;

            // Particle effects: black smoke tendrils
            var particles = vein.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startColor = new Color(0.1f, 0.05f, 0.05f, 0.6f);
            main.startSize = 0.3f;
            main.startLifetime = 2f;
            main.maxParticles = 30;

            return vein;
        }

        Mesh CreateVeinMesh()
        {
            // Procedural jagged tube (simple cylinder with noise for beta)
            var mesh = new Mesh();
            int segments = 8;
            int rings = 5;
            var vertices = new List<Vector3>();
            var triangles = new List<int>();

            for (int r = 0; r < rings; r++)
            {
                float y = r * 0.5f;
                float radius = 0.3f + Random.Range(-0.1f, 0.1f);  // Irregular

                for (int s = 0; s < segments; s++)
                {
                    float angle = (s / (float)segments) * Mathf.PI * 2f;
                    float x = Mathf.Cos(angle) * radius;
                    float z = Mathf.Sin(angle) * radius;
                    vertices.Add(new Vector3(x, y, z));
                }
            }

            // Build faces
            for (int r = 0; r < rings - 1; r++)
            {
                for (int s = 0; s < segments; s++)
                {
                    int current = r * segments + s;
                    int next = r * segments + (s + 1) % segments;
                    int below = (r + 1) * segments + s;
                    int belowNext = (r + 1) * segments + (s + 1) % segments;

                    triangles.Add(current);
                    triangles.Add(below);
                    triangles.Add(next);

                    triangles.Add(next);
                    triangles.Add(below);
                    triangles.Add(belowNext);
                }
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        void HandleVeinDestroyed(DissonanceVein vein)
        {
            if (_destroyedVeinIds.Contains(vein.veinId)) return;  // Already destroyed

            _destroyedVeinIds.Add(vein.veinId);
            _veinsDestroyed++;

            Debug.Log($"[Moon2 Vein Puzzle] Vein destroyed: {vein.veinId} ({VeinsRemaining} remaining)");

            // VFX + audio feedback
            AudioManager.Instance?.PlaySFX2D("Moon2_VeinShatter");
            HapticFeedbackManager.Instance?.PlayMediumImpact();

            // Progress feedback
            OnVeinDestroyed?.Invoke(VeinsRemaining);

            // Quest progress
            QuestManager.Instance?.ProgressByType(QuestObjectiveType.TalkToNPC /*was DestroyDissonance*/, vein.veinId);

            // Weakening corruption field VFX (purple to golden shift)
            if (VFXController.Instance != null)
            {
                VFXController.Instance.PlayAetherPulse(vein.transform.position, 0.8f, Color.cyan);
            }

            // Check completion
            if (IsComplete)
            {
                CompletePuzzle();
            }
        }

        void CompletePuzzle()
        {
            Debug.Log("[Moon2 Vein Puzzle] ★ COMPLETE ★ All 12 dissonance veins destroyed! Fountain purge climax unlocked.");

            // Notify progression system
            if (Integration.Moon2ProgressionSystem.Instance != null)
            {
                Integration.Moon2ProgressionSystem.Instance.OnFountainPurged();
            }

            // Audio: corruption drone fades out, harmonic restoration swells
            AudioManager.Instance?.StopLoopingSFX("Moon2_CorruptionDrone");
            AudioManager.Instance?.PlaySFX2D("Moon2_PuzzleComplete");

            // VFX: golden wave radiates from cathedral center
            if (VFXController.Instance != null)
            {
                VFXController.Instance.PlayAetherPulse(transform.position, 5f, Color.magenta);
            }

            // Haptic celebration
            HapticFeedbackManager.Instance?.PlayDiscovery();

            // HUD banner
            if (UI.HUDController.Instance != null)
            {
                UI.HUDController.Instance.ShowBanner(
                    "Dissonance Veins Purged",
                    "The cathedral breathes. The fractal corruption breaks. The fountain awaits.",
                    6f
                );
            }

            // Fire completion event
            OnPuzzleComplete?.Invoke();

            // Achievement
            AchievementSystem.Instance?.Unlock("moon2_vein_master");
        }

        /// <summary>
        /// Save/load support: restore destroyed vein IDs.
        /// </summary>
        public void LoadState(HashSet<string> destroyedIds)
        {
            _destroyedVeinIds.Clear();
            foreach (var id in destroyedIds)
            {
                _destroyedVeinIds.Add(id);
            }

            _veinsDestroyed = _destroyedVeinIds.Count;

            // Destroy already-cleared veins
            foreach (var vein in _activeVeins)
            {
                if (vein != null)
                {
                    var comp = vein.GetComponent<DissonanceVein>();
                    if (comp != null && _destroyedVeinIds.Contains(comp.veinId))
                    {
                        Destroy(vein);
                    }
                }
            }

            Debug.Log($"[Moon2 Vein Puzzle] Loaded state: {_veinsDestroyed}/{totalVeins} veins destroyed");
        }

        public HashSet<string> SaveState()
        {
            return new HashSet<string>(_destroyedVeinIds);
        }
    }

    /// <summary>
    /// Individual dissonance vein component — handles player interaction + destruction.
    /// </summary>
    public class DissonanceVein : MonoBehaviour, IInteractable
    {
        public string veinId;
        public event System.Action<DissonanceVein> OnDestroyed;

        float _health = 50f;
        bool _isBeingPurged;

        public string GetInteractPrompt()
        {
            // Micro-giant mode required
            if (Integration.MicroGiantController.Instance != null &&
                Integration.MicroGiantController.Instance.IsPlayerShrunkForMicroGiantMode())
            {
                return "[E] Purge Dissonance Vein";
            }
            return "Shrink to micro-giant scale to access (hold [Q])";
        }

        public void Interact(GameObject interactor)
        {
            // Must be in micro-giant mode
            if (Integration.MicroGiantController.Instance == null ||
                !Integration.MicroGiantController.Instance.IsPlayerShrunkForMicroGiantMode())
            {
                AudioManager.Instance?.PlaySFX2D("UI_Error");
                if (UI.HUDController.Instance != null)
                {
                    UI.HUDController.Instance.ShowBanner("Cannot Reach", "Vein too deep — shrink to micro-giant scale first.", 2f);
                }
                return;
            }

            if (_isBeingPurged) return;
            _isBeingPurged = true;

            StartPurge();
        }

        void StartPurge()
        {
            // For beta: instant destroy with VFX (full impl: tuning minigame)
            _health = 0f;

            // VFX: vein shatters into black shards
            var particles = gameObject.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startColor = new Color(0.2f, 0.05f, 0.05f);
            main.startSize = 0.4f;
            main.startLifetime = 1.5f;
            main.maxParticles = 60;
            particles.Play();

            // Audio
            AudioManager.Instance?.PlaySFX2D("Moon2_VeinShatter");

            // Notify puzzle controller
            OnDestroyed?.Invoke(this);

            // Destroy after VFX
            Destroy(gameObject, 2f);
        }
    }
}
