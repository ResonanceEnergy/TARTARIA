using UnityEngine;
using Tartaria.Core;
using Tartaria.UI;
using Tartaria.Save;

namespace Tartaria.Integration
{
    /// <summary>
    /// Zone Transition System -- handles loading/unloading zones,
    /// transition effects, and zone data management.
    ///
    /// Phase 1: Echohaven (start) -> Solara (second zone at RS 100).
    /// Future: Ley Line Map for non-linear zone selection.
    /// </summary>
    [DisallowMultipleComponent]
    public class ZoneTransitionSystem : MonoBehaviour
    {
        public static ZoneTransitionSystem Instance { get; private set; }

        [Header("Zone Definitions")]
        [SerializeField] ZoneDefinition[] zones;
        [SerializeField] int startingZoneIndex;

        [Header("Transition")]
        [SerializeField] float fadeOutDuration = 1.5f;
        [SerializeField] float fadeInDuration = 1.0f;
        [SerializeField] float minimumLoadingTime = 2.0f;

        int _currentZoneIndex = -1;
        bool _transitioning;

        public ZoneDefinition CurrentZone =>
            _currentZoneIndex >= 0 && _currentZoneIndex < zones.Length
                ? zones[_currentZoneIndex] : null;

        public int CurrentZoneIndex => _currentZoneIndex;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            if (zones != null && zones.Length > 0)
                LoadZone(startingZoneIndex);
        }

        /// <summary>
        /// Transition to the next zone in sequence (called when current zone hits RS 100).
        /// </summary>
        public void TransitionToNextZone()
        {
            int next = _currentZoneIndex + 1;
            if (zones == null || next >= zones.Length)
            {
                Debug.Log("[ZoneTransition] No more zones -- game complete!");
                return;
            }
            StartCoroutine(TransitionSequence(next));
        }

        /// <summary>
        /// Transition to a specific zone by index.
        /// </summary>
        public void TransitionToZone(int zoneIndex)
        {
            if (zones == null || zoneIndex < 0 || zoneIndex >= zones.Length) return;
            if (zoneIndex == _currentZoneIndex) return;
            StartCoroutine(TransitionSequence(zoneIndex));
        }

        /// <summary>
        /// Transition to a specific zone by name.
        /// </summary>
        public void TransitionToZone(string zoneName)
        {
            if (zones == null) return;
            for (int i = 0; i < zones.Length; i++)
            {
                if (zones[i] != null && zones[i].zoneName == zoneName)
                {
                    TransitionToZone(i);
                    return;
                }
            }
            Debug.LogWarning($"[ZoneTransition] Zone not found: {zoneName}");
        }

        System.Collections.IEnumerator TransitionSequence(int targetZoneIndex)
        {
            if (_transitioning) yield break;
            _transitioning = true;

            var targetZone = zones[targetZoneIndex];
            Debug.Log($"[ZoneTransition] {CurrentZone?.zoneName ?? "None"} -> {targetZone.zoneName}");

            // Enter loading state
            var prevState = GameStateManager.Instance.CurrentState;
            GameStateManager.Instance.TransitionTo(GameState.Loading);

            // Fade out
            UIManager.Instance?.UpdateLoadingProgress(0f, targetZone.loadingTip);
            yield return FadeScreen(1f, fadeOutDuration);

            // Auto-save before zone switch
            SaveManager.Instance?.MarkDirty();

            float loadStart = Time.realtimeSinceStartup;

            // Unload current zone
            if (_currentZoneIndex >= 0 && CurrentZone != null)
                UnloadZone(_currentZoneIndex);

            // Load new zone
            LoadZone(targetZoneIndex);
            UIManager.Instance?.UpdateLoadingProgress(0.5f, targetZone.loadingTip);

            // Ensure minimum loading time (so tip is readable)
            float elapsed = Time.realtimeSinceStartup - loadStart;
            if (elapsed < minimumLoadingTime)
                yield return new WaitForSecondsRealtime(minimumLoadingTime - elapsed);

            UIManager.Instance?.UpdateLoadingProgress(1f);

            // Fade in
            yield return FadeScreen(0f, fadeInDuration);

            // Return to exploration
            GameStateManager.Instance.TransitionTo(GameState.Exploration);
            _transitioning = false;

            // Show zone name
            HUDController.Instance?.SetZoneName(
                $"{targetZone.zoneName} -- {targetZone.subtitle}");
        }

        void LoadZone(int index)
        {
            _currentZoneIndex = index;
            var zone = zones[index];

            // Configure ZoneController
            if (ZoneController.Instance != null)
            {
                var so = ZoneController.Instance;
                // ZoneController will read from its own serialized fields
                // In a full implementation, we'd load the scene additively
            }

            // Configure atmosphere from zone definition
            if (zone.fogColorLow != default)
            {
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.ExponentialSquared;
                RenderSettings.fogColor = zone.fogColorLow;
                RenderSettings.fogDensity = zone.startingFogDensity;
                RenderSettings.ambientLight = zone.ambientLow;
            }

            // Move player to spawn point
            var player = GameObject.FindWithTag("Player");
            if (player != null && zone.playerSpawnPosition != Vector3.zero)
                player.transform.position = zone.playerSpawnPosition;

            Debug.Log($"[ZoneTransition] Loaded zone: {zone.zoneName}");
        }

        void UnloadZone(int index)
        {
            // Clean up zone-specific entities and objects
            // In a full implementation, we'd unload the additively-loaded scene
            Debug.Log($"[ZoneTransition] Unloaded zone: {zones[index].zoneName}");
        }

        System.Collections.IEnumerator FadeScreen(float targetAlpha, float duration)
        {
            // Placeholder -- in production, animate a CanvasGroup alpha
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                UIManager.Instance?.UpdateLoadingProgress(progress);
                yield return null;
            }
        }
    }

    /// <summary>
    /// ScriptableObject holding zone configuration data.
    /// </summary>
    [CreateAssetMenu(menuName = "Tartaria/Zone Definition")]
    public class ZoneDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string zoneName;
        public string subtitle;
        [TextArea(2, 4)]
        public string loreIntro;

        [Header("Gameplay")]
        public int zoneIndex;
        public float rsRequirementToUnlock;
        public int buildingCount = 3;
        public Vector3 playerSpawnPosition;

        [Header("Atmosphere")]
        public Color fogColorLow = new(0.3f, 0.25f, 0.2f);
        public Color fogColorHigh = new(0.8f, 0.75f, 0.5f);
        public float startingFogDensity = 0.03f;
        public Color ambientLow = new(0.15f, 0.12f, 0.1f);
        public Color ambientHigh = new(0.6f, 0.55f, 0.4f);

        [Header("Loading Screen")]
        [TextArea(1, 2)]
        public string loadingTip;
    }
}
