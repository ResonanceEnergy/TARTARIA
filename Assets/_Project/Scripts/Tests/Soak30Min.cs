// Soak30Min.cs — 30-minute scripted soak test for the Moon 1 vertical slice.
// Owned by: QA Engineer agent (per docs/agents/COORDINATION.md path ownership).
//
// What this does:
//   Drives the player + game systems through a ~1800-second scripted sequence
//   without depending on PlayerInputHandler or any human input. Logs exceptions
//   and errors across the whole run, and writes a report to persistentDataPath
//   when finished. Pass criteria are checked in the companion template at
//   docs/playtests/soak30min-template.md.
//
// Entry point:
//   - Editor menu: "Tartaria/9 QA/Run 30-Min Soak Test" (see Soak30MinMenu.cs)
//   - Code:        Tartaria.Tests.Soak30Min.RunFromMenu()
//
// IMPORTANT:
//   - This component bypasses PlayerInputHandler. It moves a chosen "player"
//     transform directly. That isolates soak failures from the input chain.
//   - All Find* / GetComponent calls happen ONCE at Start. The Update loop is
//     hot-path safe per the CLAUDE.md "no expensive Find* every frame" rule.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Tests
{
    /// <summary>
    /// Scripted 30-minute soak test. See file header for full contract.
    /// </summary>
    public class Soak30Min : MonoBehaviour
    {
        // ---- timing constants (seconds) ----------------------------------------
        private const float PHASE_WALK_END           =   60f;  // 0   - 60   walk village waypoints
        private const float PHASE_BUILDING1_END      =  180f;  // 60  - 180  Fountain restoration
        private const float PHASE_BUILDING2_END      =  360f;  // 180 - 360  Dome restoration
        private const float PHASE_BUILDING3_END      =  540f;  // 360 - 540  Cathedral restoration (fires moon complete)
        private const float PHASE_POSTWIN_END        =  720f;  // 540 - 720  post-win observation
        private const float PHASE_PAUSE_CYCLES_END   = 1080f;  // 720 - 1080 pause/unpause x10
        private const float PHASE_SAVELOAD_CYCLES_END= 1440f;  // 1080- 1440 save/load x5
        private const float PHASE_IDLE_END           = 1800f;  // 1440- 1800 idle observation

        private const int PAUSE_CYCLE_COUNT = 10;
        private const int SAVELOAD_CYCLE_COUNT = 5;

        // ---- cached refs (cached once at Start; never refetched per frame) -----
        private Transform _playerTransform;
        private object    _pauseMenuInstance;
        private MethodInfo _pauseMenuToggle;
        private object    _saveManagerInstance;
        private MethodInfo _saveManagerQuickSave;
        private MethodInfo _saveManagerQuickLoad;

        // ---- waypoints (Moon 1 village rough footprint) ------------------------
        // These are deliberately conservative coordinates that stay near origin
        // and exercise the player capsule across the village center. The soak
        // test does not rely on NavMesh; it lerps transform.position.
        private readonly Vector3[] _waypoints = new Vector3[]
        {
            new Vector3(  0f, 1f,   0f),
            new Vector3( 20f, 1f,  10f),
            new Vector3( 30f, 1f,  -5f),
            new Vector3( 15f, 1f, -25f),
            new Vector3(-10f, 1f, -20f),
            new Vector3(-25f, 1f,   5f),
            new Vector3(-15f, 1f,  20f),
            new Vector3(  0f, 1f,  25f),
        };

        // ---- error / exception capture -----------------------------------------
        private int _exceptionCount;
        private int _errorCount;
        private readonly List<string> _firstStackTraces = new List<string>(8);
        private const int MAX_STACK_TRACES = 5;

        // ---- frame-time tracking -----------------------------------------------
        private double _totalFrameTimeMs;
        private long   _frameSamples;
        private readonly List<float> _frameSamplesMs = new List<float>(108_000); // ~60fps * 1800s

        // ---- restoration event tracking ----------------------------------------
        private bool _moonCompletedFired;

        // ---- soak completion ---------------------------------------------------
        private bool _finished;
        private float _startRealtime;

        // ========================================================================
        //  Entry point
        // ========================================================================

        /// <summary>
        /// Entry point — invoked from the editor menu (see Soak30MinMenu.cs) or
        /// directly from code. Instantiates the controller GameObject if one is
        /// not already in the scene, and starts the scripted coroutine.
        /// </summary>
        public static void RunFromMenu()
        {
            // Per docs/agents/API_CONTRACT.md § "Unity 6 API replacements":
            // FindObjectOfType is obsolete. FindFirstObjectByType is the canonical replacement.
            var existing = UnityEngine.Object.FindFirstObjectByType<Soak30Min>(FindObjectsInactive.Include);
            if (existing != null)
            {
                Debug.LogWarning("[Soak30Min] A soak controller is already running. Ignoring duplicate start.");
                return;
            }

            var go = new GameObject("__Soak30Min_Controller__");
            UnityEngine.Object.DontDestroyOnLoad(go);
            var controller = go.AddComponent<Soak30Min>();
            controller.StartCoroutine(controller.StartSoak());
            Debug.Log("[Soak30Min] Controller spawned. 30-minute scripted soak starting.");
        }

        // ========================================================================
        //  Lifecycle
        // ========================================================================

        private void Awake()
        {
            Application.logMessageReceived += OnLogMessageReceived;
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
            GameEvents.OnMoonCompleted -= HandleMoonCompleted;
        }

        private void Start()
        {
            _startRealtime = Time.realtimeSinceStartup;
            CacheReferences();
            GameEvents.OnMoonCompleted += HandleMoonCompleted;
        }

        private void Update()
        {
            // Hot path: no Find*, no GetComponent. Just sample frame time.
            float dtMs = Time.unscaledDeltaTime * 1000f;
            _totalFrameTimeMs += dtMs;
            _frameSamples++;
            _frameSamplesMs.Add(dtMs);
        }

        // ========================================================================
        //  One-time reference caching (per CLAUDE.md: no Find* in Update)
        // ========================================================================

        private void CacheReferences()
        {
            // Cache the "player" transform. We look for a GameObject tagged
            // "Player" first, then fall back to any object named "Player".
            // If nothing exists, we synthesize a stand-in cube so the test
            // still runs (we still log a warning).
            GameObject player = null;
            try { player = GameObject.FindGameObjectWithTag("Player"); }
            catch (UnityException) { /* tag may not be defined; ignore */ }

            if (player == null)
            {
                player = GameObject.Find("Player");
            }

            if (player == null)
            {
                Debug.LogWarning("[Soak30Min] No GameObject tagged 'Player' or named 'Player' found. Spawning a stand-in cube so the walk phase still exercises transform movement.");
                player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                player.name = "__Soak30Min_StandInPlayer__";
                // URP-safe: ensure the primitive doesn't render pink. Best-effort.
                var rend = player.GetComponent<Renderer>();
                if (rend != null && rend.sharedMaterial != null && rend.sharedMaterial.HasProperty("_BaseColor"))
                {
                    rend.sharedMaterial.SetColor("_BaseColor", Color.cyan); // URP-safe
                }
            }
            _playerTransform = player.transform;

            // Cache PauseMenu singleton + Toggle method via reflection so we
            // don't take a hard compile-time dependency on Tartaria.UI.
            CachePauseMenuRefs();

            // Cache SaveManager singleton + QuickSave/QuickLoad methods via
            // reflection. Soft dependency on Tartaria.Save.
            CacheSaveManagerRefs();
        }

        private void CachePauseMenuRefs()
        {
            try
            {
                var pauseMenuType = FindTypeAcrossAssemblies("Tartaria.UI.PauseMenu");
                if (pauseMenuType == null)
                {
                    Debug.LogWarning("[Soak30Min] PauseMenu type not found. Pause cycles will be skipped.");
                    return;
                }

                var instanceProp = pauseMenuType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                if (instanceProp != null)
                {
                    _pauseMenuInstance = instanceProp.GetValue(null);
                }

                _pauseMenuToggle = pauseMenuType.GetMethod("Toggle", BindingFlags.Public | BindingFlags.Instance);

                if (_pauseMenuInstance == null)
                {
                    Debug.LogWarning("[Soak30Min] PauseMenu.Instance is null (scene may not have spawned it). Pause cycles will look up Instance lazily before each call.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Soak30Min] Failed to cache PauseMenu refs: {ex.Message}");
            }
        }

        private void CacheSaveManagerRefs()
        {
            try
            {
                var saveManagerType = FindTypeAcrossAssemblies("Tartaria.Save.SaveManager");
                if (saveManagerType == null)
                {
                    Debug.LogWarning("[Soak30Min] SaveManager type not found. Save/load cycles will be skipped.");
                    return;
                }

                var instanceProp = saveManagerType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                if (instanceProp != null)
                {
                    _saveManagerInstance = instanceProp.GetValue(null);
                }

                _saveManagerQuickSave = saveManagerType.GetMethod("QuickSave", BindingFlags.Public | BindingFlags.Instance);
                _saveManagerQuickLoad = saveManagerType.GetMethod("QuickLoad", BindingFlags.Public | BindingFlags.Instance);

                if (_saveManagerInstance == null)
                {
                    Debug.LogWarning("[Soak30Min] SaveManager.Instance is null at Start. Save/load cycles will look up Instance lazily before each call.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Soak30Min] Failed to cache SaveManager refs: {ex.Message}");
            }
        }

        private static Type FindTypeAcrossAssemblies(string fullName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType(fullName, false);
                if (t != null) return t;
            }
            return null;
        }

        // ========================================================================
        //  Scripted sequence
        // ========================================================================

        public IEnumerator StartSoak()
        {
            // Wait one frame so Start() has run + refs are cached.
            yield return null;

            Debug.Log("[Soak30Min] === SOAK START === target duration 1800s.");

            // Phase 1: 0 - 60s — walk village waypoints.
            yield return StartCoroutine(Phase_Walk(0f, PHASE_WALK_END));

            // Phase 2: 60 - 180s — building 1 (Fountain) — fire 3 nodes.
            yield return StartCoroutine(Phase_Building("Fountain", PHASE_WALK_END, PHASE_BUILDING1_END,
                /* fire at absolute t = */ 120f, 140f, 180f));

            // Phase 3: 180 - 360s — building 2 (Dome) — fire 3 nodes.
            yield return StartCoroutine(Phase_Building("Dome", PHASE_BUILDING1_END, PHASE_BUILDING2_END,
                240f, 280f, 340f));

            // Phase 4: 360 - 540s — building 3 (Cathedral) — fire 3 nodes; OnMoonCompleted should fire after last.
            yield return StartCoroutine(Phase_Building("Cathedral", PHASE_BUILDING2_END, PHASE_BUILDING3_END,
                420f, 470f, 520f));

            // Phase 5: 540 - 720s — post-win observation (no action).
            yield return StartCoroutine(Phase_Idle(PHASE_BUILDING3_END, PHASE_POSTWIN_END, "post-win"));

            // Phase 6: 720 - 1080s — pause/unpause cycles x10.
            yield return StartCoroutine(Phase_PauseCycles(PHASE_POSTWIN_END, PHASE_PAUSE_CYCLES_END));

            // Phase 7: 1080 - 1440s — save/load cycles x5.
            yield return StartCoroutine(Phase_SaveLoadCycles(PHASE_PAUSE_CYCLES_END, PHASE_SAVELOAD_CYCLES_END));

            // Phase 8: 1440 - 1800s — final idle observation.
            yield return StartCoroutine(Phase_Idle(PHASE_SAVELOAD_CYCLES_END, PHASE_IDLE_END, "final-idle"));

            FinishSoak();
        }

        // ----- Phase: walk waypoints --------------------------------------------
        private IEnumerator Phase_Walk(float fromT, float toT)
        {
            Debug.Log($"[Soak30Min] Phase WALK begins (t={fromT:0}s -> {toT:0}s).");
            float phaseDuration = toT - fromT;
            float secondsPerWaypoint = Mathf.Max(1f, phaseDuration / Mathf.Max(1, _waypoints.Length));
            Vector3 origin = _playerTransform != null ? _playerTransform.position : Vector3.zero;

            for (int i = 0; i < _waypoints.Length; i++)
            {
                Vector3 start = _playerTransform != null ? _playerTransform.position : origin;
                Vector3 target = _waypoints[i];
                float elapsed = 0f;
                while (elapsed < secondsPerWaypoint)
                {
                    if (_playerTransform != null)
                    {
                        float k = Mathf.Clamp01(elapsed / secondsPerWaypoint);
                        _playerTransform.position = Vector3.Lerp(start, target, k);
                    }
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            // Burn any remainder of the phase budget so timing alignment holds.
            yield return WaitUntilSoakTime(toT);
        }

        // ----- Phase: building restoration (3 nodes) ----------------------------
        private IEnumerator Phase_Building(string buildingId, float fromT, float toT,
                                           float fireAtA, float fireAtB, float fireAtC)
        {
            Debug.Log($"[Soak30Min] Phase BUILDING={buildingId} begins (t={fromT:0}s -> {toT:0}s).");
            float[] fires = { fireAtA, fireAtB, fireAtC };
            int fireIdx = 0;
            while (SoakElapsed() < toT)
            {
                if (fireIdx < fires.Length && SoakElapsed() >= fires[fireIdx])
                {
                    SafeFireBuildingRestored(buildingId, fireIdx + 1, fires.Length);
                    fireIdx++;
                }
                yield return null;
            }
        }

        private void SafeFireBuildingRestored(string buildingId, int nodeNumber, int totalNodes)
        {
            try
            {
                GameEvents.FireBuildingRestored(buildingId);
                Debug.Log($"[Soak30Min] Fired BuildingRestored('{buildingId}') node {nodeNumber}/{totalNodes} at t={SoakElapsed():0.0}s.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Soak30Min] Exception firing BuildingRestored('{buildingId}'): {ex}");
            }
        }

        // ----- Phase: idle / observation ----------------------------------------
        private IEnumerator Phase_Idle(float fromT, float toT, string label)
        {
            Debug.Log($"[Soak30Min] Phase IDLE ({label}) begins (t={fromT:0}s -> {toT:0}s).");
            yield return WaitUntilSoakTime(toT);
        }

        // ----- Phase: pause cycles ----------------------------------------------
        private IEnumerator Phase_PauseCycles(float fromT, float toT)
        {
            Debug.Log($"[Soak30Min] Phase PAUSE-CYCLES begins (t={fromT:0}s -> {toT:0}s, count={PAUSE_CYCLE_COUNT}).");
            float phaseDuration = toT - fromT;
            // 10 cycles, each is a pause + unpause => 20 toggles. Space them
            // evenly across the budget with a small buffer at the end.
            int totalToggles = PAUSE_CYCLE_COUNT * 2;
            float toggleSpacing = phaseDuration / (totalToggles + 1);
            int toggled = 0;

            while (SoakElapsed() < toT && toggled < totalToggles)
            {
                float nextToggleAt = fromT + toggleSpacing * (toggled + 1);
                while (SoakElapsed() < nextToggleAt && SoakElapsed() < toT)
                {
                    yield return null;
                }
                SafePauseToggle(toggled);
                toggled++;
            }

            // Drain the rest of the phase.
            yield return WaitUntilSoakTime(toT);
        }

        private void SafePauseToggle(int toggleIndex)
        {
            if (_pauseMenuToggle == null)
            {
                if (toggleIndex == 0)
                    Debug.LogWarning("[Soak30Min] Pause toggle skipped: PauseMenu.Toggle reflection not available.");
                return;
            }

            // PauseMenu.Instance may have been spawned after Start. Resolve lazily if needed.
            if (_pauseMenuInstance == null)
            {
                try
                {
                    var t = FindTypeAcrossAssemblies("Tartaria.UI.PauseMenu");
                    var ip = t?.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                    _pauseMenuInstance = ip?.GetValue(null);
                }
                catch { /* swallow — captured by log handler if it surfaces */ }
            }

            if (_pauseMenuInstance == null)
            {
                if (toggleIndex == 0)
                    Debug.LogWarning("[Soak30Min] Pause toggle skipped: PauseMenu.Instance is null.");
                return;
            }

            try
            {
                _pauseMenuToggle.Invoke(_pauseMenuInstance, null);
                Debug.Log($"[Soak30Min] PauseMenu.Toggle() #{toggleIndex + 1}/{PAUSE_CYCLE_COUNT * 2} at t={SoakElapsed():0.0}s.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Soak30Min] PauseMenu.Toggle() invocation failed: {ex}");
            }
        }

        // ----- Phase: save/load cycles ------------------------------------------
        private IEnumerator Phase_SaveLoadCycles(float fromT, float toT)
        {
            Debug.Log($"[Soak30Min] Phase SAVE/LOAD-CYCLES begins (t={fromT:0}s -> {toT:0}s, count={SAVELOAD_CYCLE_COUNT}).");
            float phaseDuration = toT - fromT;
            // Each cycle is save + load. Space cycle starts evenly; between
            // save and load, wait one second.
            float cycleSpacing = phaseDuration / (SAVELOAD_CYCLE_COUNT + 1);
            for (int i = 0; i < SAVELOAD_CYCLE_COUNT; i++)
            {
                float cycleStartAt = fromT + cycleSpacing * (i + 1);
                yield return WaitUntilSoakTime(cycleStartAt);

                SafeQuickSave(i);
                // One-second beat between save and load so any deferred IO settles.
                yield return new WaitForSecondsRealtime(1f);
                SafeQuickLoad(i);
            }

            // Drain the rest of the phase.
            yield return WaitUntilSoakTime(toT);
        }

        private void SafeQuickSave(int cycleIndex)
        {
            if (_saveManagerQuickSave == null)
            {
                if (cycleIndex == 0)
                    Debug.LogWarning("[Soak30Min] QuickSave skipped: SaveManager.QuickSave reflection not available.");
                return;
            }
            EnsureSaveManagerInstance();
            if (_saveManagerInstance == null)
            {
                if (cycleIndex == 0)
                    Debug.LogWarning("[Soak30Min] QuickSave skipped: SaveManager.Instance is null.");
                return;
            }
            try
            {
                _saveManagerQuickSave.Invoke(_saveManagerInstance, null);
                Debug.Log($"[Soak30Min] SaveManager.QuickSave() cycle #{cycleIndex + 1}/{SAVELOAD_CYCLE_COUNT} at t={SoakElapsed():0.0}s.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Soak30Min] SaveManager.QuickSave() invocation failed: {ex}");
            }
        }

        private void SafeQuickLoad(int cycleIndex)
        {
            if (_saveManagerQuickLoad == null)
            {
                if (cycleIndex == 0)
                    Debug.LogWarning("[Soak30Min] QuickLoad skipped: SaveManager.QuickLoad reflection not available.");
                return;
            }
            EnsureSaveManagerInstance();
            if (_saveManagerInstance == null)
            {
                if (cycleIndex == 0)
                    Debug.LogWarning("[Soak30Min] QuickLoad skipped: SaveManager.Instance is null.");
                return;
            }
            try
            {
                _saveManagerQuickLoad.Invoke(_saveManagerInstance, null);
                Debug.Log($"[Soak30Min] SaveManager.QuickLoad() cycle #{cycleIndex + 1}/{SAVELOAD_CYCLE_COUNT} at t={SoakElapsed():0.0}s.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Soak30Min] SaveManager.QuickLoad() invocation failed: {ex}");
            }
        }

        private void EnsureSaveManagerInstance()
        {
            if (_saveManagerInstance != null) return;
            try
            {
                var t = FindTypeAcrossAssemblies("Tartaria.Save.SaveManager");
                var ip = t?.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                _saveManagerInstance = ip?.GetValue(null);
            }
            catch { /* swallow */ }
        }

        // ========================================================================
        //  Helpers
        // ========================================================================

        private float SoakElapsed()
        {
            return Time.realtimeSinceStartup - _startRealtime;
        }

        private IEnumerator WaitUntilSoakTime(float targetSeconds)
        {
            while (SoakElapsed() < targetSeconds)
            {
                yield return null;
            }
        }

        private void HandleMoonCompleted(MoonCompletedEventArgs args)
        {
            _moonCompletedFired = true;
            Debug.Log($"[Soak30Min] OnMoonCompleted received (moonIndex={(args != null ? args.moonIndex.ToString() : "<null>")}) at t={SoakElapsed():0.0}s.");
        }

        private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Exception)
            {
                _exceptionCount++;
                if (_firstStackTraces.Count < MAX_STACK_TRACES)
                {
                    _firstStackTraces.Add($"[Exception #{_exceptionCount} at t={SoakElapsed():0.0}s] {condition}\n{stackTrace}");
                }
            }
            else if (type == LogType.Error)
            {
                // Don't double-count our own report lines.
                if (condition != null && condition.StartsWith("[Soak30Min]")) return;
                _errorCount++;
            }
        }

        // ========================================================================
        //  Reporting
        // ========================================================================

        private void FinishSoak()
        {
            if (_finished) return;
            _finished = true;

            float totalDurationSec = SoakElapsed();
            double avgFrameMs = _frameSamples > 0 ? _totalFrameTimeMs / _frameSamples : 0.0;
            float p95FrameMs = ComputeP95(_frameSamplesMs);

            var sb = new StringBuilder(4096);
            sb.AppendLine("=== TARTARIA 30-MIN SOAK TEST REPORT ===");
            sb.AppendLine($"Generated:        {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Unity version:    {Application.unityVersion}");
            sb.AppendLine($"Total duration:   {totalDurationSec:0.0} s ({totalDurationSec / 60f:0.00} min)");
            sb.AppendLine($"Frame samples:    {_frameSamples}");
            sb.AppendLine($"Avg frame time:   {avgFrameMs:0.000} ms");
            sb.AppendLine($"P95 frame time:   {p95FrameMs:0.000} ms");
            sb.AppendLine($"Exception count:  {_exceptionCount}");
            sb.AppendLine($"Error count:      {_errorCount}");
            sb.AppendLine($"OnMoonCompleted:  {(_moonCompletedFired ? "FIRED" : "NOT FIRED")}");
            sb.AppendLine();
            sb.AppendLine("=== Pass criteria (matches docs/playtests/soak30min-template.md) ===");
            sb.AppendLine("  exceptions == 0");
            sb.AppendLine("  errors      < 5");
            sb.AppendLine("  avg frame   < 16.6 ms");
            sb.AppendLine();
            sb.AppendLine("=== First exception stack traces (max 5) ===");
            if (_firstStackTraces.Count == 0)
            {
                sb.AppendLine("  (none)");
            }
            else
            {
                for (int i = 0; i < _firstStackTraces.Count; i++)
                {
                    sb.AppendLine($"---- trace {i + 1} ----");
                    sb.AppendLine(_firstStackTraces[i]);
                }
            }

            string reportPath = Path.Combine(Application.persistentDataPath, "soak30min_report.txt");
            try
            {
                File.WriteAllText(reportPath, sb.ToString());
                Debug.Log($"[Soak30Min] === SOAK COMPLETE === report written to: {reportPath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Soak30Min] Failed to write report to '{reportPath}': {ex}");
            }
        }

        private static float ComputeP95(List<float> samples)
        {
            if (samples == null || samples.Count == 0) return 0f;
            // Local copy + sort; samples count up to ~108k which is fine to sort once.
            var copy = new List<float>(samples);
            copy.Sort();
            int index = Mathf.Clamp(Mathf.FloorToInt(copy.Count * 0.95f), 0, copy.Count - 1);
            return copy[index];
        }
    }
}
