using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Core.Time
{
    /// <summary>
    /// Drives the in-game 24-hour cycle for Moon 1 (and beyond).
    /// Fires <see cref="GameEvents.FireTartarianHourChanged"/> on each integer-hour rollover and
    /// <see cref="GameEvents.FireSeventeenthHour"/> when the cycle hits hour 17 (cathedral light eruption beat).
    /// Bootstraps automatically after scene load via <see cref="RuntimeInitializeOnLoadMethodAttribute"/>.
    ///
    /// Per CLAUDE.md no-debt mandate:
    /// - No silent fails: missing directional light logs a warning with identifier.
    /// - No silent catch: exceptions in GameEvents fan-out are logged AND rethrown.
    /// - No stubs: every method has a real body and observable side effect.
    /// </summary>
    [DisallowMultipleComponent]
    public class MoonCycleController : MonoBehaviour
    {
        public static MoonCycleController Instance { get; private set; }

        [Header("Cycle scale")]
        [Tooltip("How many real-world minutes equal one in-game 24-hour day. 1.0 = a full day every minute of real time.")]
        [SerializeField] float realMinutesPerInGameDay = 1f;

        [Tooltip("Starting in-game hour at scene load (0-24). 12 = noon.")]
        [Range(0f, 24f)]
        [SerializeField] float startHour = 12f;

        /// <summary>Current in-game hour as a continuous float (0 inclusive, 24 exclusive).</summary>
        public float CurrentHour { get; private set; }

        int _lastHour = -1;

        [Header("Sky")]
        [Tooltip("Directional light rotated to match in-game hour. Auto-found at Awake if null.")]
        [SerializeField] Light directionalLight;

        // ─── Bootstrap ────────────────────────────────────────────────

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject(nameof(MoonCycleController));
            DontDestroyOnLoad(go);
            // AddComponent triggers Awake which sets Instance.
            go.AddComponent<MoonCycleController>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"[MoonCycleController] Duplicate instance on '{gameObject.name}' — destroying. Existing instance on '{Instance.gameObject.name}'.");
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            CurrentHour = Mathf.Clamp(startHour, 0f, 23.999f);
            _lastHour = Mathf.FloorToInt(CurrentHour);

            TryFindDirectionalLight();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ─── Scene wiring ─────────────────────────────────────────────

        void TryFindDirectionalLight()
        {
            if (directionalLight != null) return;

            // FindObjectsByType (Unity 6) — explicit sort mode avoids sort cost.
            var lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i].type == LightType.Directional)
                {
                    directionalLight = lights[i];
                    Debug.Log($"[MoonCycleController] Auto-bound directional light '{directionalLight.gameObject.name}' (path: {GetHierarchyPath(directionalLight.transform)}).");
                    break;
                }
            }

            if (directionalLight == null)
            {
                Debug.LogWarning("[MoonCycleController] No directional Light found in scene — sky rotation disabled this session. Assign 'directionalLight' in inspector or add a Light with type=Directional.");
            }
        }

        static string GetHierarchyPath(Transform t)
        {
            if (t == null) return "<null>";
            var path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return path;
        }

        // ─── Update ───────────────────────────────────────────────────

        void Update()
        {
            // Use unscaledDeltaTime so the clock keeps ticking through pause/timescale changes
            // for dialogue and HUD beats — gameplay slow-mo should not stop the day.
            float secondsPerDay = Mathf.Max(0.0001f, realMinutesPerInGameDay * 60f);
            float hoursPerSecond = 24f / secondsPerDay;

            CurrentHour = (CurrentHour + hoursPerSecond * UnityEngine.Time.unscaledDeltaTime) % 24f;

            int hourNow = Mathf.FloorToInt(CurrentHour);
            if (hourNow != _lastHour)
            {
                _lastHour = hourNow;
                FireHourTransition(hourNow);
            }

            if (directionalLight != null)
            {
                // Hour 0 = midnight (sun below horizon), hour 6 = sunrise, hour 12 = noon, hour 18 = sunset.
                float angle = (CurrentHour / 24f) * 360f - 90f;
                directionalLight.transform.rotation = Quaternion.Euler(angle, 30f, 0f);
            }
        }

        // ─── Event fan-out ────────────────────────────────────────────

        void FireHourTransition(int hour)
        {
            Debug.Log($"[MoonCycleController] Hour transitioned -> {hour:00}:00 (CurrentHour={CurrentHour:F2})");

            // Direct call — events live in the same assembly (Tartaria.Core).
            // Per rule 3: no silent catch. GameEvents.Fire* already log on internal exceptions for
            // its OnTartarianHourChanged path, but we still wrap our fan-out so we can attribute
            // the failure to this controller and rethrow.
            try
            {
                GameEvents.FireTartarianHourChanged(hour);

                // 17th-hour cathedral light eruption beat (Moon 1).
                if (hour == 17)
                {
                    Debug.Log("[MoonCycleController] Hour 17 reached -> firing OnSeventeenthHour (cathedral light eruption beat).");
                    GameEvents.FireSeventeenthHour();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MoonCycleController] Exception while firing hour-transition events for hour {hour}: {ex}");
                throw; // rule 3: no silent catch
            }
        }

        // ─── QA / Debug API ───────────────────────────────────────────

        /// <summary>
        /// QA helper: instantly skip time to a target in-game hour.
        /// If the hour rolls past one or more integer hours, a single FireHourTransition is raised
        /// for the destination hour (intermediate hours are skipped intentionally — this is a debug
        /// teleport, not a fast-forward).
        /// </summary>
        public void SkipToHour(float hour)
        {
            float clamped = Mathf.Clamp(hour, 0f, 23.999f);
            int oldFloor = Mathf.FloorToInt(CurrentHour);
            CurrentHour = clamped;
            int newFloor = Mathf.FloorToInt(CurrentHour);
            Debug.Log($"[MoonCycleController] SkipToHour: {oldFloor:00}:00 -> {newFloor:00}:00 (requested={hour:F2}, clamped={clamped:F2})");
            if (newFloor != oldFloor)
            {
                _lastHour = newFloor;
                FireHourTransition(newFloor);
            }
        }
    }
}
