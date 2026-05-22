using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Gameplay;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 4 (Deep Forge) content spawner — star fort construction + moat puzzles + guardian golem.
    ///
    /// Design per GDD §03:
    ///   - Massive buried star fort (geometric bastions, dry moats)
    ///   - Fort resists tuning — dissonant energy inside
    ///   - Echo NPCs: confused garrison fragments ("The commander... something happened...")
    ///   - Fill moats with conductive pure-water (pipe puzzle)
    ///   - Precision rock cut bastion blocks (giant mode)
    ///   - Align six-pointed geometry (12 perfect alignment points)
    ///   - Hidden inscription: "For my brother, the Builder. Hold the line. — Z." (Zereth)
    ///   - Corrupted guardian golem: 30-foot humanoid, living mud + shattered stone
    ///   - Once Tartarian guardian giant Maelix, corrupted by centuries of dissonance
    ///   - Giant-mode wrestling match while defending bastions
    ///   - Moats flood → fort connects to grid → bell tower scalar waves → golem cleansed
    ///   - Giant's final memory crystal: Maelix was Korath's brother, "Z" = Zereth (3rd brother, Dissonant One)
    ///   - 17-Hour Clock Fragment recovered from fort core
    ///
    /// Activates when player completes Moon 3 (Electric Moon orphan train).
    /// </summary>
    public class Moon4ContentSpawner : MonoBehaviour
    {
        public static Moon4ContentSpawner Instance { get; private set; }

        [Header("Content State")]
        [SerializeField] bool moon4Unlocked = false;
        [SerializeField] bool moatsFlooded = false;
        [SerializeField] bool golemDefeated = false;
        [SerializeField] bool clockFragmentRecovered = false;

        [Header("Star Fort")]
        [SerializeField] int totalBastions = 12;  // Per GDD: 12 perfect alignment points
        int _bastionsAligned;

        [Header("Moat Puzzles")]
        [SerializeField] int totalMoatSegments = 6;  // 6 moat segments to flood
        int _moatsFlooded;

        [Header("Spawning")]
        [SerializeField] Vector3 fortCenter = new(100f, 5f, 80f);  // Deep Forge star fort
        [SerializeField] Vector3 golemSpawnPoint;  // Set via editor or calculated
        [SerializeField] Vector3[] bastionPoints;  // 12 geometric positions
        [SerializeField] Vector3[] moatSegmentStarts;  // 6 moat pipe puzzle locations

        GameObject _guardianGolem;
        readonly List<GameObject> _echoNPCs = new();
        readonly List<GameObject> _bastionMarkers = new();
        bool _contentSpawned;
        bool _golemEncounterTriggered;

        public bool IsMoon4Active => moon4Unlocked && !clockFragmentRecovered;
        public int BastionsRemaining => totalBastions - _bastionsAligned;
        public float BastionProgress => _bastionsAligned / (float)totalBastions;
        public float MoatProgress => _moatsFlooded / (float)totalMoatSegments;

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
            // Check if Moon 4 should auto-unlock (Moon 3 complete)
            if (SaveManager.Instance != null)
            {
                var moonProgress = SaveManager.Instance.GetMoonProgress(3);  // Moon 3
                if (moonProgress >= 100f && !moon4Unlocked)
                {
                    UnlockMoon4();
                }
            }
        }

        // ─── Public API ───────────────────────────────

        /// <summary>
        /// Unlocks Moon 4 content (called when Moon 3 complete).
        /// </summary>
        public void UnlockMoon4()
        {
            if (moon4Unlocked) return;

            moon4Unlocked = true;
            Debug.Log("[Moon 4] Self-Existing Moon unlocked — The Form of Foundations begins");

            SpawnMoon4Content();

            // Tutorial hint
            TutorialSystem.Instance?.Show(TutorialStep.Moon4Intro);
            HUDController.Instance?.ShowInteractionPrompt("Moon 4: The Form of Foundations — A star fort calls...");
        }

        /// <summary>
        /// Spawns all Moon 4 content: star fort, Echo NPCs, moat puzzles, guardian golem.
        /// </summary>
        void SpawnMoon4Content()
        {
            if (_contentSpawned) return;
            _contentSpawned = true;

            // Spawn Echo NPCs (confused garrison fragments)
            SpawnEchoGarrison();

            // Spawn bastion alignment markers (12 geometric points)
            SpawnBastionMarkers();

            // Spawn moat pipe puzzle segments (6 locations)
            SpawnMoatPuzzles();

            // Activate dissonant ambient audio
            Audio.AudioManager.Instance?.PlayLoopingSFX("Moon4_DissonantPulse", fortCenter);

            Debug.Log($"[Moon 4] Spawned star fort ({totalBastions} bastions, {totalMoatSegments} moat segments)");
        }

        void SpawnEchoGarrison()
        {
            // Create 3 confused Echo NPCs around fort perimeter
            for (int i = 0; i < 3; i++)
            {
                float angle = (i / 3f) * Mathf.PI * 2f;
                var spawnPos = fortCenter + new Vector3(
                    Mathf.Cos(angle) * 20f,
                    0f,
                    Mathf.Sin(angle) * 20f
                );

                var echo = new GameObject($"EchoNPC_Garrison_{i:D2}");
                echo.transform.position = spawnPos;

                // Visual: translucent humanoid capsule (Echo spirit)
                var filter = echo.AddComponent<MeshFilter>();
                filter.mesh = Resources.GetBuiltinResource<Mesh>("Capsule.fbx");
                var renderer = echo.AddComponent<MeshRenderer>();
                renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                renderer.material.color = new Color(0.7f, 0.7f, 0.9f, 0.3f);  // Ghostly blue-white
                renderer.material.SetFloat("_Surface", 1);  // Transparent mode
                renderer.material.SetFloat("_Blend", 0);

                echo.transform.localScale = new Vector3(0.8f, 2f, 0.8f);  // Human proportions

                // Add dialogue trigger
                var dialogue = echo.AddComponent<EchoGarrisonDialogue>();

                _echoNPCs.Add(echo);
            }

            Debug.Log("[Moon 4] Spawned 3 Echo garrison NPCs");
        }

        void SpawnBastionMarkers()
        {
            // Generate bastion positions if not set manually (star fort 12-point geometry)
            if (bastionPoints == null || bastionPoints.Length == 0)
            {
                bastionPoints = GenerateBastionPositions(totalBastions);
            }

            for (int i = 0; i < Mathf.Min(totalBastions, bastionPoints.Length); i++)
            {
                var bastion = CreateBastionMarker(bastionPoints[i], i);
                _bastionMarkers.Add(bastion);
            }
        }

        GameObject CreateBastionMarker(Vector3 position, int index)
        {
            var bastion = new GameObject($"BastionMarker_{index:D2}");
            bastion.transform.position = position;

            // Visual: stone cube (placeholder for bastion alignment point)
            var filter = bastion.AddComponent<MeshFilter>();
            filter.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");

            var renderer = bastion.AddComponent<MeshRenderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            renderer.material.color = new Color(0.4f, 0.35f, 0.3f);  // Dark stone

            bastion.transform.localScale = new Vector3(2f, 3f, 2f);  // Bastion block

            // Collider + interaction
            var collider = bastion.AddComponent<BoxCollider>();
            collider.size = Vector3.one;

            // Add BastionAlignment component (handles alignment interaction)
            var alignment = bastion.AddComponent<BastionAlignment>();
            alignment.OnAligned += OnBastionAligned;

            // Check for inscription on bastion 0 (Zereth's message)
            if (index == 0)
            {
                var inscription = bastion.AddComponent<InscriptionTrigger>();
                inscription.inscriptionText = "For my brother, the Builder. Hold the line. — Z.";
            }

            return bastion;
        }

        Vector3[] GenerateBastionPositions(int count)
        {
            var positions = new Vector3[count];
            float radius = 30f;  // Star fort outer ring

            for (int i = 0; i < count; i++)
            {
                // 12-point star geometry (alternating inner/outer points)
                float angle = (i / (float)count) * Mathf.PI * 2f;
                float r = (i % 2 == 0) ? radius : radius * 0.7f;  // Alternating radius for star shape

                positions[i] = fortCenter + new Vector3(
                    Mathf.Cos(angle) * r,
                    Random.Range(-0.5f, 0.5f),  // Slight height variance (buried)
                    Mathf.Sin(angle) * r
                );
            }

            return positions;
        }

        void SpawnMoatPuzzles()
        {
            // Generate moat positions if not set manually
            if (moatSegmentStarts == null || moatSegmentStarts.Length == 0)
            {
                moatSegmentStarts = GenerateMoatPositions(totalMoatSegments);
            }

            for (int i = 0; i < Mathf.Min(totalMoatSegments, moatSegmentStarts.Length); i++)
            {
                var moatPuzzle = CreateMoatPuzzle(moatSegmentStarts[i], i);
                // Moat puzzles don't need tracking list (fire events directly)
            }
        }

        GameObject CreateMoatPuzzle(Vector3 position, int index)
        {
            var puzzle = new GameObject($"MoatPuzzle_{index:D2}");
            puzzle.transform.position = position;

            // Visual: pipe segment (cylinder placeholder)
            var filter = puzzle.AddComponent<MeshFilter>();
            filter.mesh = Resources.GetBuiltinResource<Mesh>("Cylinder.fbx");

            var renderer = puzzle.AddComponent<MeshRenderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            renderer.material.color = new Color(0.3f, 0.3f, 0.3f);  // Dark metal pipe

            puzzle.transform.localScale = new Vector3(0.5f, 3f, 0.5f);  // Pipe segment

            // Add MoatPipeInteraction component
            var interaction = puzzle.AddComponent<MoatPipeInteraction>();
            interaction.OnFlooded += OnMoatSegmentFlooded;

            return puzzle;
        }

        Vector3[] GenerateMoatPositions(int count)
        {
            var positions = new Vector3[count];
            float radius = 25f;  // Moat ring (between inner court + outer bastions)

            for (int i = 0; i < count; i++)
            {
                float angle = (i / (float)count) * Mathf.PI * 2f;
                positions[i] = fortCenter + new Vector3(
                    Mathf.Cos(angle) * radius,
                    -1f,  // Below ground (moat trench)
                    Mathf.Sin(angle) * radius
                );
            }

            return positions;
        }

        void OnBastionAligned(BastionAlignment bastion)
        {
            _bastionsAligned++;

            Debug.Log($"[Moon 4] Bastion aligned ({_bastionsAligned}/{totalBastions})");

            // VFX + audio feedback
            Audio.AudioManager.Instance?.PlaySFX2D("Moon4_BastionSnap");
            Input.HapticFeedbackManager.Instance?.PlayDiscovery();

            // Progress tracking
            QuestManager.Instance?.ProgressByType(QuestObjectiveType.AlignBastions, bastion.gameObject.name);

            // Check for moats flooded + all bastions aligned → trigger golem encounter
            if (_bastionsAligned >= totalBastions && _moatsFlooded >= totalMoatSegments && !_golemEncounterTriggered)
            {
                TriggerGolemEncounter();
            }
        }

        void OnMoatSegmentFlooded(MoatPipeInteraction moat)
        {
            _moatsFlooded++;

            Debug.Log($"[Moon 4] Moat segment flooded ({_moatsFlooded}/{totalMoatSegments})");

            // VFX: water flow particles
            Audio.AudioManager.Instance?.PlaySFX2D("Moon4_WaterFlow");

            // Quest progress
            QuestManager.Instance?.ProgressByType(QuestObjectiveType.FloodMoats, moat.gameObject.name);

            // Check for trigger conditions
            if (_bastionsAligned >= totalBastions && _moatsFlooded >= totalMoatSegments && !_golemEncounterTriggered)
            {
                TriggerGolemEncounter();
            }
        }

        /// <summary>
        /// Triggers guardian golem encounter (Days 13-18 conflict).
        /// </summary>
        void TriggerGolemEncounter()
        {
            _golemEncounterTriggered = true;

            Debug.Log("[Moon 4] Golem encounter triggered — corrupted guardian emerges!");

            // Spawn guardian golem
            golemSpawnPoint = fortCenter + Vector3.forward * 5f;
            _guardianGolem = new GameObject("GuardianGolem_Maelix");
            _guardianGolem.transform.position = golemSpawnPoint;
            _guardianGolem.transform.localScale = Vector3.one * 3f;  // 30-foot scale (10m)

            // Visual: large capsule (placeholder for 30-foot mud golem)
            var filter = _guardianGolem.AddComponent<MeshFilter>();
            filter.mesh = Resources.GetBuiltinResource<Mesh>("Capsule.fbx");
            var renderer = _guardianGolem.AddComponent<MeshRenderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            renderer.material.color = new Color(0.25f, 0.2f, 0.15f);  // Living mud + shattered stone

            // Add health + combat
            var health = _guardianGolem.AddComponent<MudGolemHealth>();
            health.maxHealth = 500f;  // Boss-tier health
            health.OnDeath += OnGolemDefeated;

            // Collider
            var collider = _guardianGolem.AddComponent<CapsuleCollider>();
            collider.height = 5f;
            collider.radius = 1.5f;

            // Distorted voice SFX
            Audio.AudioManager.Instance?.PlaySFX2D("Moon4_GolemRoar");

            // Dialogue: "The song... the song was... WRONG..."
            DialogueManager.Instance?.PlayContextDialogue("moon4_golem_distorted");

            // Quest objective: defeat corrupted guardian
            QuestManager.Instance?.ActivateQuest("moon4_guardian_battle");

            HUDController.Instance?.ShowInteractionPrompt("Corrupted Guardian emerged — Giant Mode required!");
        }

        void OnGolemDefeated()
        {
            golemDefeated = true;

            Debug.Log("[Moon 4] Guardian golem defeated — Maelix's memory crystal recovered");

            // Climax: moats flood, fort connects to grid, bell tower activates
            TriggerFortActivation();
        }

        /// <summary>
        /// Triggers climax event: fort connects to grid + bell tower scalar waves (Days 19-24).
        /// </summary>
        void TriggerFortActivation()
        {
            Debug.Log("[Moon 4] Fort activation climax — scalar waves lighting up distant zones!");

            // Moats glow with conductive water
            var moatVFX = new GameObject("Moon4_MoatGlowVFX");
            moatVFX.transform.position = fortCenter;
            var particles = moatVFX.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startColor = new Color(0.5f, 0.7f, 1f, 1f);  // Blue conductive water
            main.startSize = 4f;
            main.startLifetime = 10f;
            main.maxParticles = 2000;

            // Bell tower scalar waves
            Audio.AudioManager.Instance?.PlaySFX2D("Moon4_BellTowerWaves");

            // Golem crumbles peacefully, reveals crystal
            if (_guardianGolem != null)
            {
                // Golden light from golem chest
                var light = _guardianGolem.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(1f, 0.9f, 0.5f);
                light.range = 15f;
                light.intensity = 3f;

                // Drop memory crystal
                SpawnMemoryCrystal(golemSpawnPoint + Vector3.up * 2f);

                Destroy(_guardianGolem, 3f);  // Crumble after 3s
            }

            // Fort connects to global grid
            Debug.Log("[Moon 4] Star fort routing powers trains from Moon 3");

            // Quest complete
            QuestManager.Instance?.CompleteQuest("moon4_star_fort_restoration");

            // Trigger revelation sequence
            Invoke(nameof(TriggerRevelation), 5f);  // After VFX settles
        }

        void TriggerRevelation()
        {
            clockFragmentRecovered = true;

            Debug.Log("[Moon 4] Revelation — Maelix was Korath's brother, Z = Zereth!");

            // Dialogue: Korath's echo reveals Maelix + Zereth connection
            DialogueManager.Instance?.PlayContextDialogue("moon4_korath_brother_revelation");

            // 17-Hour Clock Fragment acquired
            GrantClockFragment();

            // Unlock Moon 5
            HUDController.Instance?.ShowInteractionPrompt("Moon 4 Complete — Self-Existing Moon connected!");
            SaveManager.Instance?.SetMoonProgress(4, 100f);
        }

        void SpawnMemoryCrystal(Vector3 position)
        {
            var crystal = new GameObject("MaelixMemoryCrystal");
            crystal.transform.position = position;

            // Visual: glowing golden crystal
            var filter = crystal.AddComponent<MeshFilter>();
            filter.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            var renderer = crystal.AddComponent<MeshRenderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            renderer.material.color = new Color(1f, 0.9f, 0.5f, 0.8f);  // Golden translucent
            renderer.material.SetFloat("_Surface", 1);
            renderer.material.SetFloat("_Blend", 0);

            crystal.transform.localScale = Vector3.one * 0.5f;

            // Interaction: play Maelix's final memory
            var interact = crystal.AddComponent<MemoryCrystalInteract>();

            // Pulsing light
            var light = crystal.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.9f, 0.5f);
            light.range = 8f;
            light.intensity = 2f;
        }

        void GrantClockFragment()
        {
            Debug.Log("[Moon 4] 17-Hour Clock Fragment acquired — first hint of Tartarian time system");

            // Add to inventory
            InventorySystem.Instance?.Add("clock_fragment_17hour", 1);

            // Achievement unlock
            AchievementSystem.Instance?.UnlockAchievement("H07");  // Hidden: "Discover Zereth's trigger room" (clock fragment)

            // Lore entry
            // TODO: Unlock codex entry about 17-hour Tartarian time system
        }

        /// <summary>
        /// Called by save system to restore Moon 4 state.
        /// </summary>
        public void LoadState(bool unlocked, int bastionsAligned, int moatsFlooded, bool golemDead, bool fragmentRecovered)
        {
            moon4Unlocked = unlocked;
            _bastionsAligned = bastionsAligned;
            _moatsFlooded = moatsFlooded;
            golemDefeated = golemDead;
            clockFragmentRecovered = fragmentRecovered;

            if (moon4Unlocked && !_contentSpawned)
            {
                SpawnMoon4Content();
            }

            // Destroy bastions/moats that were already completed
            for (int i = 0; i < _bastionsAligned && i < _bastionMarkers.Count; i++)
            {
                if (_bastionMarkers[i] != null)
                {
                    _bastionMarkers[i].GetComponent<BastionAlignment>()?.MarkAligned();
                }
            }

            // Check if golem encounter should trigger
            if (_bastionsAligned >= totalBastions && _moatsFlooded >= totalMoatSegments && !_golemEncounterTriggered && !golemDefeated)
            {
                TriggerGolemEncounter();
            }
        }
    }

    /// <summary>
    /// Bastion alignment component — handles geometric snap interaction.
    /// </summary>
    public class BastionAlignment : MonoBehaviour, IInteractable
    {
        public event System.Action<BastionAlignment> OnAligned;

        bool _isAligned;

        public string GetPrompt() => _isAligned ? "Bastion Aligned" : "Hold [E] to Align Bastion";

        public void Interact(GameObject interactor)
        {
            if (_isAligned) return;

            // For beta: instant align with VFX (full impl: geometric puzzle)
            _isAligned = true;

            // VFX: golden snap particles
            var particles = gameObject.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startColor = new Color(1f, 0.9f, 0.5f);
            main.startSize = 1f;
            main.startLifetime = 2f;
            main.maxParticles = 100;
            particles.Play();

            // Audio
            Audio.AudioManager.Instance?.PlaySFX2D("BastionAlign");

            // Change color to golden (aligned)
            var renderer = GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(1f, 0.9f, 0.5f);
            }

            // Notify spawner
            OnAligned?.Invoke(this);
        }

        public void MarkAligned()
        {
            _isAligned = true;
            var renderer = GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(1f, 0.9f, 0.5f);
            }
        }
    }

    /// <summary>
    /// Moat pipe interaction — handles conductive water flooding puzzle.
    /// </summary>
    public class MoatPipeInteraction : MonoBehaviour, IInteractable
    {
        public event System.Action<MoatPipeInteraction> OnFlooded;

        bool _isFlooded;

        public string GetPrompt() => _isFlooded ? "Moat Flooded" : "Hold [E] to Connect Pipe";

        public void Interact(GameObject interactor)
        {
            if (_isFlooded) return;

            // For beta: instant flood (full impl: pipe puzzle mini-game)
            _isFlooded = true;

            // VFX: water flow particles
            var particles = gameObject.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startColor = new Color(0.5f, 0.7f, 1f);  // Blue water
            main.startSize = 0.6f;
            main.startLifetime = 3f;
            main.maxParticles = 200;
            particles.Play();

            // Audio
            Audio.AudioManager.Instance?.PlaySFX2D("WaterFlow");

            // Change color to blue (flooded)
            var renderer = GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(0.4f, 0.6f, 0.9f);
            }

            // Notify spawner
            OnFlooded?.Invoke(this);
        }
    }

    /// <summary>
    /// Echo garrison NPC dialogue — confused fort defenders.
    /// </summary>
    public class EchoGarrisonDialogue : MonoBehaviour, IInteractable
    {
        bool _firstInteraction = true;

        public string GetPrompt() => "Talk to Echo Garrison";

        public void Interact(GameObject interactor)
        {
            if (_firstInteraction)
            {
                _firstInteraction = false;
                DialogueManager.Instance?.PlayContextDialogue("moon4_echo_garrison_confused");
                Audio.AudioManager.Instance?.PlaySFX2D("Moon4_EchoWhisper");
            }
        }
    }

    /// <summary>
    /// Inscription trigger — displays Zereth's message on bastion.
    /// </summary>
    public class InscriptionTrigger : MonoBehaviour, IInteractable
    {
        public string inscriptionText;
        bool _read;

        public string GetPrompt() => _read ? "Inscription Read" : "Read Inscription";

        public void Interact(GameObject interactor)
        {
            if (_read) return;
            _read = true;

            Debug.Log($"[Moon 4 Inscription] {inscriptionText}");
            HUDController.Instance?.ShowInteractionPrompt($"Inscription: {inscriptionText}");

            // Achievement: discovered Zereth connection
            AchievementSystem.Instance?.UnlockAchievement("L04");  // "Zereth's Truth" partial progress
        }
    }

    /// <summary>
    /// Memory crystal interaction — plays Maelix's final memory cinematic.
    /// </summary>
    public class MemoryCrystalInteract : MonoBehaviour, IInteractable
    {
        bool _viewed;

        public string GetPrompt() => _viewed ? "Memory Viewed" : "View Memory Crystal";

        public void Interact(GameObject interactor)
        {
            if (_viewed) return;
            _viewed = true;

            Debug.Log("[Moon 4] Viewing Maelix's final memory...");

            // Play memory cinematic (for beta: dialogue + audio)
            DialogueManager.Instance?.PlayContextDialogue("moon4_maelix_memory");
            Audio.AudioManager.Instance?.PlaySFX2D("Moon4_MemoryCrystal");

            // Destroy crystal after view
            Destroy(gameObject, 5f);
        }
    }
}
