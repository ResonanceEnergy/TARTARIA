using UnityEngine;
using UnityEngine.SceneManagement;
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
    public class ZoneTransitionSystem : MonoBehaviour, IZoneTransitionService
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
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            ServiceLocator.ZoneTransition = this;
        }

        void OnDestroy()
        {
            StopAllCoroutines();
            _transitioning = false;
            if (Instance == this) Instance = null;
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

            // RS gate check — ensure player has enough RS to unlock next zone
            var nextZone = zones[next];
            if (nextZone.rsRequirementToUnlock > 0f)
            {
                float currentRS = AetherFieldManager.Instance != null
                    ? AetherFieldManager.Instance.ResonanceScore : 0f;
                if (currentRS < nextZone.rsRequirementToUnlock)
                {
                    HUDController.Instance?.ShowInteractionPrompt(
                        $"Zone locked. Requires RS {nextZone.rsRequirementToUnlock:F0}. Current: {currentRS:F0}");
                    Debug.Log($"[ZoneTransition] RS gate failed: need {nextZone.rsRequirementToUnlock}, have {currentRS}");
                    return;
                }
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
            var prevState = GameStateManager.Instance?.CurrentState ?? GameState.Exploration;
            GameStateManager.Instance?.TransitionTo(GameState.Loading);

            bool success = false;
            try
            {
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

                success = true;
            }
            finally
            {
                _transitioning = false;
                // Return to exploration
                GameStateManager.Instance?.TransitionTo(GameState.Exploration);
            }

            if (success)
            {
                // Show zone subtitle + lore intro + haptic
                ShowZoneSubtitle(targetZone);
            }
        }

        void LoadZone(int index)
        {
            _currentZoneIndex = index;
            var zone = zones[index];

            // Load zone scene additively if specified
            if (!string.IsNullOrEmpty(zone.sceneName))
            {
                var scene = SceneManager.GetSceneByName(zone.sceneName);
                if (!scene.isLoaded)
                    SceneManager.LoadSceneAsync(zone.sceneName, LoadSceneMode.Additive);
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
            var zone = zones[index];
            if (!string.IsNullOrEmpty(zone.sceneName))
            {
                var scene = SceneManager.GetSceneByName(zone.sceneName);
                if (scene.isLoaded)
                    SceneManager.UnloadSceneAsync(zone.sceneName);
            }
            Debug.Log($"[ZoneTransition] Unloaded zone: {zone.zoneName}");
        }

        System.Collections.IEnumerator FadeScreen(float targetAlpha, float duration)
        {
            // Animate fade via UIManager CanvasGroup
            float startAlpha = targetAlpha > 0.5f ? 0f : 1f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                float alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                UIManager.Instance?.SetFadeAlpha(alpha);
                UIManager.Instance?.UpdateLoadingProgress(t);
                yield return null;
            }

            UIManager.Instance?.SetFadeAlpha(targetAlpha);
        }

        /// <summary>
        /// Show zone subtitle with lore intro after transition completes.
        /// </summary>
        void ShowZoneSubtitle(ZoneDefinition zone)
        {
            if (zone == null) return;

            string subtitle = $"{zone.zoneName} -- {zone.subtitle}";
            HUDController.Instance?.SetZoneName(subtitle);

            if (!string.IsNullOrEmpty(zone.loreIntro))
            {
                DialogueManager.Instance?.PlayContextDialogue(
                    $"zone_{zone.zoneName.ToLowerInvariant().Replace(' ', '_')}");
            }

            // Moon-specific haptic on zone entry
            Input.HapticFeedbackManager.Instance?.PlayMoonHaptic(
                zone.zoneIndex, Input.HapticContext.ZoneTransition);
        }
    }
}
