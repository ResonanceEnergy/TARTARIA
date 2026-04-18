using UnityEngine;
using Tartaria.Core;
using Tartaria.Input;
using Tartaria.UI;

namespace Tartaria.Integration
{
    /// <summary>
    /// Zone Controller — manages zone boundaries, proximity-based discovery,
    /// HUD zone name display, and environmental atmosphere.
    ///
    /// Phase 1: Single zone (Echohaven) with discovery triggers for 3 buildings.
    /// Future: Multi-zone with streaming and ley line connections.
    /// </summary>
    [DisallowMultipleComponent]
    public class ZoneController : MonoBehaviour
    {
        public static ZoneController Instance { get; private set; }

        [Header("Zone Info")]
        [SerializeField] string zoneName = "Echohaven";
        [SerializeField] string zoneSubtitle = "The Buried Settlement";

        [Header("Discovery")]
        [SerializeField] float discoveryRadius = 15f;
        [SerializeField] float discoveryCheckInterval = 0.5f;

        [Header("Atmosphere")]
        [SerializeField] Color fogColorLow = new(0.3f, 0.25f, 0.2f);     // Muddy brown at RS 0
        [SerializeField] Color fogColorMid = new(0.5f, 0.45f, 0.3f);     // Golden hint at RS 50
        [SerializeField] Color fogColorHigh = new(0.8f, 0.75f, 0.5f);    // Golden glow at RS 100
        [SerializeField] float fogDensityStart = 0.03f;
        [SerializeField] float fogDensityEnd = 0.005f;

        [Header("Ambient Light")]
        [SerializeField] Color ambientLow = new(0.15f, 0.12f, 0.1f);     // Dim and gloomy
        [SerializeField] Color ambientHigh = new(0.6f, 0.55f, 0.4f);     // Warm golden

        InteractableBuilding[] _buildings;
        Transform _playerTransform;
        PlayerInputHandler _playerInputHandler;
        float _discoveryTimer;
        float _currentRS;
        bool _zoneNameShown;
        float _playerRetryTimer;
        bool _atmosphereDirty = true;
        static readonly Color SunColorCool = new Color(0.8f, 0.75f, 0.7f);
        static readonly Color SunColorWarm = new Color(1f, 0.92f, 0.75f);

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Start()
        {
            _sceneLoadTime = Time.time;

            // Runtime override: scene may have stale 30f from editor serialization
            if (discoveryRadius > 15f) discoveryRadius = 15f;

            // Find all buildings in zone, deduplicate by position
            var allBuildings = FindObjectsByType<InteractableBuilding>(FindObjectsSortMode.None);
            var unique = new System.Collections.Generic.List<InteractableBuilding>();
            var seen = new System.Collections.Generic.HashSet<string>();
            foreach (var b in allBuildings)
            {
                // Key by rounded position to catch duplicates at same location
                string key = $"{b.transform.position.x:F0}_{b.transform.position.z:F0}";
                if (seen.Add(key))
                    unique.Add(b);
            }
            _buildings = unique.ToArray();
            if (_buildings.Length != allBuildings.Length)
                Debug.Log($"[ZoneController] {allBuildings.Length} InteractableBuildings found, deduped to {_buildings.Length}");

            // Find player
            var player = GameObject.FindWithTag("Player");
            if (player != null)
                _playerTransform = player.transform;

            // Initialize fog
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;

            // Show zone name on entry
            ShowZoneName();
            CompanionManager.Instance?.CheckUnlocks(0);
        }

        void Update()
        {
            if (_playerTransform == null)
            {
                _playerRetryTimer -= Time.deltaTime;
                if (_playerRetryTimer <= 0f)
                {
                    _playerRetryTimer = 0.5f;
                    var player = GameObject.FindWithTag("Player");
                    if (player != null)
                        _playerTransform = player.transform;
                }
                return;
            }

            CheckDiscoveries();
            if (_atmosphereDirty)
            {
                _atmosphereDirty = false;
                UpdateAtmosphere();
            }
            CheckIdleDialogue();
        }

        // ─── Discovery System ────────────────────────

        float _sceneLoadTime;
        const float DISCOVERY_GRACE_PERIOD = 5f;

        void CheckDiscoveries()
        {
            // Suppress discoveries during scene-load grace period
            if (Time.time - _sceneLoadTime <= DISCOVERY_GRACE_PERIOD) return;

            _discoveryTimer += Time.deltaTime;
            if (_discoveryTimer < discoveryCheckInterval) return;
            _discoveryTimer = 0f;

            if (_buildings == null) return;

            Vector3 playerPos = _playerTransform.position;

            foreach (var building in _buildings)
            {
                if (building == null || building.State != Gameplay.BuildingRestorationState.Buried)
                    continue;

                float dist = Vector3.Distance(playerPos, building.transform.position);
                if (dist <= discoveryRadius)
                {
                    building.Discover();
                    return; // One discovery per check cycle to prevent cinematic overlap
                }
            }
        }

        // ─── Atmosphere ──────────────────────────────

        /// <summary>
        /// Called by GameLoopController when RS changes.
        /// </summary>
        public void UpdateRS(float rs)
        {
            _currentRS = rs;
            _atmosphereDirty = true;
        }

        void UpdateAtmosphere()
        {
            float t = _currentRS / 100f;

            // Fog — thins and warms as RS rises
            RenderSettings.fogColor = t < 0.5f
                ? Color.Lerp(fogColorLow, fogColorMid, t * 2f)
                : Color.Lerp(fogColorMid, fogColorHigh, (t - 0.5f) * 2f);
            RenderSettings.fogDensity = Mathf.Lerp(fogDensityStart, fogDensityEnd, t);

            // Ambient light — brightens with RS
            RenderSettings.ambientLight = Color.Lerp(ambientLow, ambientHigh, t);

            // Directional light warmth
            var sun = RenderSettings.sun;
            if (sun != null)
            {
                sun.intensity = Mathf.Lerp(0.6f, 1.4f, t);
                sun.color = Color.Lerp(
                    SunColorCool,
                    SunColorWarm, t);
            }
        }

        // ─── Zone Name Display ───────────────────────

        void ShowZoneName()
        {
            if (_zoneNameShown) return;
            _zoneNameShown = true;

            HUDController.Instance?.SetZoneName($"{zoneName} — {zoneSubtitle}");
            Audio.AdaptiveMusicController.Instance?.SetZone(0);
        }

        // ─── Idle Dialogue ───────────────────────────

        float _idleTimer;
        const float IDLE_DIALOGUE_INTERVAL = 45f; // Milo speaks every ~45s when idle
        float _playerInputRetryTimer;

        void CheckIdleDialogue()
        {
            if (GameStateManager.Instance?.CurrentState != GameState.Exploration) return;

            _idleTimer += Time.deltaTime;
            if (_idleTimer >= IDLE_DIALOGUE_INTERVAL)
            {
                _idleTimer = 0;
                DialogueManager.Instance?.PlayContextDialogue("exploration_idle");
                MiloController.Instance?.RequestBanter();
            }

            // Reset idle timer on movement
            if (_playerInputHandler == null)
            {
                _playerInputRetryTimer += Time.deltaTime;
                if (_playerInputRetryTimer >= 2f)
                {
                    _playerInputRetryTimer = 0f;
                    _playerInputHandler = FindFirstObjectByType<Input.PlayerInputHandler>();
                }
            }
            if (_playerInputHandler != null && _playerInputHandler.IsMoving)
                _idleTimer = 0f;
        }

        // ─── Public API ──────────────────────────────

        public string ZoneName => zoneName;

        /// <summary>
        /// Returns how many buildings are fully restored in this zone.
        /// </summary>
        public int GetRestoredBuildingCount()
        {
            if (_buildings == null) return 0;
            int count = 0;
            foreach (var b in _buildings)
            {
                if (b != null && b.State == Gameplay.BuildingRestorationState.Active)
                    count++;
            }
            return count;
        }

        /// <summary>
        /// Returns total building count in zone.
        /// </summary>
        public int GetTotalBuildingCount()
        {
            return _buildings != null ? _buildings.Length : 0;
        }
    }
}
