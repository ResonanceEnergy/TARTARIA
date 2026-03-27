using System.Collections;
using UnityEngine;
using Tartaria.Core;
using Tartaria.UI;
using Tartaria.Save;

namespace Tartaria.Integration
{
    /// <summary>
    /// Day Out of Time (DotT) — The secret 14th zone that exists outside the 13-moon calendar.
    /// Triggered after completing Moon 13's Planetary Nexus and collecting all 13 golden motes.
    ///
    /// From doc 17 (Day Out of Time):
    ///   - A single 10-minute real-time event
    ///   - All restored buildings pulse in sync
    ///   - Anastasia delivers her final lines before solidification
    ///   - The world transforms: fog lifts, full golden palette, 432 Hz drone
    ///   - Player walks through all 13 zones in a compressed memory corridor
    ///   - Ends with Anastasia's final solidification and the True Ending
    ///
    /// Anastasia DotT lines (from doc 18):
    ///   ID 102: "I can feel the ground remembering what it was."
    ///   ID 103: "Ten seconds. After a thousand years, ten seconds is all it takes."
    /// </summary>
    [DisallowMultipleComponent]
    public class DayOutOfTimeController : MonoBehaviour
    {
        public static DayOutOfTimeController Instance { get; private set; }

        [Header("DotT Configuration")]
        [SerializeField] float eventDurationSeconds = 600f; // 10 minutes
        [SerializeField] float memoryCorridorSpeed = 12f;
        [SerializeField] Color dottSkyColor = new(0.95f, 0.85f, 0.5f, 1f);
        [SerializeField] Color dottFogColor = new(0.9f, 0.8f, 0.4f, 1f);
        [SerializeField] float dottFogDensity = 0.005f;

        bool _eventActive;
        bool _eventCompleted;
        float _eventTimer;
        int _currentMemoryZone;

        public bool IsEventActive => _eventActive;
        public bool IsEventCompleted => _eventCompleted;
        public float EventProgress => _eventActive ? Mathf.Clamp01(_eventTimer / eventDurationSeconds) : 0f;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        /// <summary>
        /// Check if DotT can be triggered (all 13 motes collected + Moon 13 complete).
        /// </summary>
        public bool CanTrigger()
        {
            if (_eventCompleted || _eventActive) return false;

            var ana = AnastasiaController.Instance;
            if (ana == null) return false;

            // Need all 13 golden motes
            for (int i = 0; i < 13; i++)
            {
                if (!ana.IsMoteCollected(i)) return false;
            }

            // Need to be in Moon 13 or beyond
            return ana.CurrentMoon >= 13;
        }

        /// <summary>
        /// Begin the Day Out of Time event.
        /// </summary>
        public void TriggerEvent()
        {
            if (!CanTrigger()) return;

            _eventActive = true;
            _eventTimer = 0f;
            _currentMemoryZone = 0;

            GameStateManager.Instance?.TransitionTo(GameState.Cinematic);
            Debug.Log("[DotT] Day Out of Time has begun.");

            StartCoroutine(DotTSequence());
        }

        IEnumerator DotTSequence()
        {
            // Phase 1: World transformation (first 30 seconds)
            yield return StartCoroutine(WorldTransformation());

            // Phase 2: Anastasia's first DotT line
            AnastasiaController.Instance?.TryDeliverLine("dott_start");
            yield return new WaitForSeconds(8f);

            // Phase 3: Memory corridor — walk through 13 compressed zones
            for (int i = 0; i < 13; i++)
            {
                _currentMemoryZone = i + 1;
                yield return StartCoroutine(MemoryZoneFlash(i));
            }

            // Phase 4: Anastasia's final line
            AnastasiaController.Instance?.TryDeliverLine("dott_final");
            yield return new WaitForSeconds(10f);

            // Phase 5: Solidification
            var ana = AnastasiaController.Instance;
            if (ana != null && ana.CurrentSolidPhase == AnastasiaController.SolidificationPhase.NotTriggered)
            {
                ana.TriggerSolidification();
                // Wait for solidification to complete (~30s based on AnastasiaController)
                yield return new WaitUntil(() =>
                    ana.CurrentSolidPhase == AnastasiaController.SolidificationPhase.Return ||
                    ana.CurrentSolidPhase == AnastasiaController.SolidificationPhase.NotTriggered);
                yield return new WaitForSeconds(5f);
            }

            // Phase 6: True Ending
            _eventActive = false;
            _eventCompleted = true;

            HUDController.Instance?.ShowInteractionPrompt(
                "THE DAY OUT OF TIME\nResonance Restored. The world remembers.");

            yield return new WaitForSeconds(10f);
            HUDController.Instance?.HideInteractionPrompt();

            GameStateManager.Instance?.TransitionTo(GameState.Exploration);
            SaveManager.Instance?.MarkDirty();

            Debug.Log("[DotT] Day Out of Time complete. True Ending reached.");
        }

        IEnumerator WorldTransformation()
        {
            float duration = 30f;
            float elapsed = 0f;

            Color startFog = RenderSettings.fogColor;
            Color startAmbient = RenderSettings.ambientLight;
            float startDensity = RenderSettings.fogDensity;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

                RenderSettings.fogColor = Color.Lerp(startFog, dottFogColor, t);
                RenderSettings.ambientLight = Color.Lerp(startAmbient, dottSkyColor, t);
                RenderSettings.fogDensity = Mathf.Lerp(startDensity, dottFogDensity, t);

                _eventTimer += Time.deltaTime;
                yield return null;
            }
        }

        IEnumerator MemoryZoneFlash(int zoneIndex)
        {
            // Each zone gets ~35 seconds of the corridor (13 zones * ~35s = ~7.5 minutes)
            float zoneDuration = (eventDurationSeconds - 150f) / 13f; // minus transformation + ending time

            HUDController.Instance?.ShowInteractionPrompt(
                $"Memory {zoneIndex + 1} of 13");

            // Zone atmosphere flash
            float elapsed = 0f;
            while (elapsed < zoneDuration)
            {
                elapsed += Time.deltaTime;
                _eventTimer += Time.deltaTime;
                yield return null;
            }

            HUDController.Instance?.HideInteractionPrompt();
        }
    }
}
