using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon 1 itch.io marketing screenshot capture.
    /// Walks a virtual Game-view camera through 8 hand-authored shots in the
    /// Echohaven_VerticalSlice scene and writes PNGs to Builds/itch_assets/.
    ///
    /// Menu: Tartaria/Marketing/Capture itch Screenshots
    /// Batchmode entry: Tartaria.Editor.Moon1ItchScreenshotCapture.CaptureFromBatchmode
    ///
    /// NOTE: ScreenCapture.CaptureScreenshot writes the Game view, which means a
    /// display surface is required. -batchmode -nographics will NOT produce shots;
    /// the wrapper script (scripts/dev/capture-itch-screenshots.ps1) launches Unity
    /// WITHOUT -nographics for that reason.
    ///
    /// Shot list (per Sprint 6 Lane 8 brief):
    ///   00 Cathedral exterior dusk
    ///   01 Star Dome lit
    ///   02 Spire pulsing
    ///   03 Village center wide
    ///   04 Mud Pool POI
    ///   05 Lirael grotto
    ///   06 Aether Vision overlay sample
    ///   07 Full Moon 1 vista
    /// </summary>
    public static class Moon1ItchScreenshotCapture
    {
        private const string Tag = "[Moon1ItchScreenshotCapture]";
        private const string ScenePath = "Assets/_Project/Scenes/Echohaven_VerticalSlice.unity";
        private const string OutputDirRelative = "Builds/itch_assets";
        private const int SuperSize = 1; // ScreenCapture super-size multiplier

        /// <summary>
        /// One shot specification. Authored by hand — these positions/rotations
        /// frame the buried-cathedral hero shots from the build spec.
        /// Coordinates correspond to Moon1BuildOutBuildings spawn layout.
        /// </summary>
        private struct Shot
        {
            public string label;
            public Vector3 position;
            public Vector3 eulerRotation;
            public float fieldOfView;
            public bool toggleAetherVision;
            public string lookAtNameHint; // optional: re-aim at named object if present

            public Shot(string label, Vector3 pos, Vector3 rot, float fov, bool aether = false, string lookAt = null)
            {
                this.label = label;
                this.position = pos;
                this.eulerRotation = rot;
                this.fieldOfView = fov;
                this.toggleAetherVision = aether;
                this.lookAtNameHint = lookAt;
            }
        }

        // 8 hand-authored framings. Positions are in world space relative to the
        // Echohaven spawn at origin. Heights chosen to clear vegetation but stay
        // below the dome roofline for the buried-civilization silhouette.
        private static readonly Shot[] Shots = new[]
        {
            new Shot("00_cathedral_exterior_dusk",  new Vector3(-22f,  6f, -28f), new Vector3(8f,   35f, 0f), 55f, false, "Moon1_StarDome"),
            new Shot("01_star_dome_lit",            new Vector3(  0f, 14f, -34f), new Vector3(14f,   0f, 0f), 50f, false, "Moon1_StarDome"),
            new Shot("02_spire_pulsing",            new Vector3( 24f,  9f, -18f), new Vector3(6f,  -45f, 0f), 50f, false, "Moon1_CrystalSpire"),
            new Shot("03_village_center_wide",      new Vector3(  0f, 18f,  10f), new Vector3(28f,  180f, 0f), 60f, false, null),
            new Shot("04_mud_pool_poi",             new Vector3( 38f,  4f,  22f), new Vector3(12f, -120f, 0f), 55f, false, "Moon1_MudPool"),
            new Shot("05_lirael_grotto",            new Vector3(-36f,  3f,  18f), new Vector3(5f,   95f, 0f), 50f, false, "Moon1_LiraelGrotto"),
            new Shot("06_aether_vision_overlay",    new Vector3(  0f, 10f, -22f), new Vector3(10f,   0f, 0f), 55f, true,  "Moon1_HarmonicFountain"),
            new Shot("07_full_moon1_vista",         new Vector3(-48f, 28f, -52f), new Vector3(20f,  42f, 0f), 65f, false, null),
        };

        [MenuItem("Tartaria/Marketing/Capture itch Screenshots", priority = 800)]
        public static void CaptureFromMenu()
        {
            EditorCoroutineLite.Start(CaptureRoutine(fromBatchmode: false));
        }

        /// <summary>
        /// Batchmode entry point. The PS wrapper invokes this via -executeMethod.
        /// NOTE: requires a display surface; do not call with -nographics.
        /// </summary>
        public static void CaptureFromBatchmode()
        {
            Debug.Log($"{Tag} Batchmode capture starting (display required).");
            // Drive the routine synchronously in batchmode because EditorApplication.update
            // does not pump frames the same way without a Play loop.
            var routine = CaptureRoutine(fromBatchmode: true);
            while (routine.MoveNext())
            {
                // Block on yielded WaitForSeconds by sleeping the equivalent.
                if (routine.Current is WaitForSecondsLite w)
                {
                    System.Threading.Thread.Sleep((int)(w.seconds * 1000f));
                }
                // Force the editor to repaint/update so capture has the latest frame.
                EditorApplication.QueuePlayerLoopUpdate();
                SceneView.RepaintAll();
            }
            Debug.Log($"{Tag} Batchmode capture finished.");
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(0);
            }
        }

        private static System.Collections.IEnumerator CaptureRoutine(bool fromBatchmode)
        {
            // 1. Open the scene.
            if (!File.Exists(ScenePath))
            {
                string msg = $"Scene not found at {ScenePath}. Cannot capture.";
                Debug.LogError($"{Tag} {msg}");
                if (!fromBatchmode) EditorUtility.DisplayDialog("Capture aborted", msg, "OK");
                if (fromBatchmode) EditorApplication.Exit(2);
                yield break;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.path != ScenePath)
            {
                Debug.Log($"{Tag} Opening {ScenePath} (was {activeScene.path}).");
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            // 2. Ensure output dir exists.
            string outputDirAbs = Path.Combine(
                Path.GetDirectoryName(Application.dataPath) ?? Directory.GetCurrentDirectory(),
                OutputDirRelative);
            Directory.CreateDirectory(outputDirAbs);
            Debug.Log($"{Tag} Output dir: {outputDirAbs}");

            // 3. Find or create a capture camera. We use a dedicated GO so we do not
            //    permanently move the scene's MainCamera.
            const string captureCamName = "Moon1ItchCaptureCamera";
            var camGO = GameObject.Find(captureCamName);
            if (camGO == null)
            {
                camGO = new GameObject(captureCamName);
                camGO.hideFlags = HideFlags.DontSave;
            }
            var cam = camGO.GetComponent<Camera>();
            if (cam == null) cam = camGO.AddComponent<Camera>();

            // Mirror MainCamera's clear flags / culling / skybox so URP renders normally.
            var mainCam = Camera.main;
            if (mainCam != null)
            {
                cam.clearFlags = mainCam.clearFlags;
                cam.cullingMask = mainCam.cullingMask;
                cam.backgroundColor = mainCam.backgroundColor;
                cam.nearClipPlane = mainCam.nearClipPlane;
                cam.farClipPlane = mainCam.farClipPlane;
            }
            cam.tag = "Untagged"; // do not steal MainCamera tag
            cam.depth = 100;       // render last so Game view picks this one
            cam.enabled = true;

            // 4. Walk the shot list.
            int captured = 0;
            int failed = 0;
            for (int i = 0; i < Shots.Length; i++)
            {
                var s = Shots[i];

                // Optional: re-aim at a named scene object if present.
                Vector3 pos = s.position;
                Vector3 eul = s.eulerRotation;
                if (!string.IsNullOrEmpty(s.lookAtNameHint))
                {
                    var target = GameObject.Find(s.lookAtNameHint);
                    if (target != null)
                    {
                        // Keep authored position; rotate toward the target.
                        Vector3 dir = target.transform.position - pos;
                        if (dir.sqrMagnitude > 0.001f)
                        {
                            eul = Quaternion.LookRotation(dir.normalized, Vector3.up).eulerAngles;
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"{Tag} Shot {i:D2} ({s.label}): lookAt hint '{s.lookAtNameHint}' not found — using authored rotation.");
                    }
                }

                cam.transform.position = pos;
                cam.transform.rotation = Quaternion.Euler(eul);
                cam.fieldOfView = s.fieldOfView;

                // Aether Vision toggle (shot 06) — best-effort via reflection so this
                // file does not hard-depend on the gameplay assembly.
                if (s.toggleAetherVision)
                {
                    TryToggleAetherVision(true);
                }

                // Repaint so any post-process / Aether overlay settles.
                SceneView.RepaintAll();
                EditorApplication.QueuePlayerLoopUpdate();

                yield return new WaitForSecondsLite(1.0f);

                string shotPath = Path.Combine(outputDirAbs, $"shot_{i:D2}_{s.label}.png");
                try
                {
                    // Delete stale so we know this run wrote it.
                    if (File.Exists(shotPath)) File.Delete(shotPath);
                    ScreenCapture.CaptureScreenshot(shotPath, SuperSize);
                    Debug.Log($"{Tag} Shot {i:D2}: {s.label} -> {shotPath} (pos={pos} fov={s.fieldOfView})");
                }
                catch (Exception ex)
                {
                    failed++;
                    Debug.LogError($"{Tag} Shot {i:D2} ({s.label}) FAILED at {shotPath}: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                    continue;
                }

                // Give ScreenCapture another frame to actually flush the PNG.
                yield return new WaitForSecondsLite(0.5f);

                if (File.Exists(shotPath))
                {
                    captured++;
                }
                else
                {
                    failed++;
                    Debug.LogError($"{Tag} Shot {i:D2} ({s.label}): file not written at {shotPath} after capture call. " +
                                   "Display surface required — ensure Unity is NOT launched with -nographics.");
                }

                if (s.toggleAetherVision)
                {
                    TryToggleAetherVision(false);
                }
            }

            // 5. Tear down the capture camera (DontSave flag means it won't persist,
            //    but destroy explicitly for tidiness).
            UnityEngine.Object.DestroyImmediate(camGO);

            string summary = $"Captured {captured}/{Shots.Length} shots ({failed} failed) -> {outputDirAbs}";
            Debug.Log($"{Tag} {summary}");

            if (!fromBatchmode)
            {
                EditorUtility.DisplayDialog("Itch Screenshots", summary, "OK");
                EditorUtility.RevealInFinder(outputDirAbs);
            }
            else if (failed > 0)
            {
                EditorApplication.Exit(1);
            }
        }

        /// <summary>
        /// Best-effort Aether Vision toggle via reflection. Tartaria.UI or
        /// Tartaria.Gameplay may expose an AetherVisionOverlay singleton; if not,
        /// log a warning and continue — the shot still gets captured, just without
        /// the overlay effect.
        /// </summary>
        private static void TryToggleAetherVision(bool on)
        {
            try
            {
                var overlayType = FindTypeByFullName("Tartaria.UI.AetherVisionOverlay")
                                 ?? FindTypeByFullName("Tartaria.Gameplay.AetherVisionOverlay")
                                 ?? FindTypeByFullName("AetherVisionOverlay");
                if (overlayType == null)
                {
                    Debug.LogWarning($"{Tag} AetherVisionOverlay type not found — capturing shot without overlay.");
                    return;
                }
                // Look for a static SetActive(bool) or Instance.SetActive(bool).
                var staticMethod = overlayType.GetMethod("SetActive", new[] { typeof(bool) });
                if (staticMethod != null && staticMethod.IsStatic)
                {
                    staticMethod.Invoke(null, new object[] { on });
                    Debug.Log($"{Tag} AetherVision (static) -> {on}");
                    return;
                }
                var instanceProp = overlayType.GetProperty("Instance");
                var instance = instanceProp?.GetValue(null);
                if (instance != null)
                {
                    var setActive = overlayType.GetMethod("SetActive", new[] { typeof(bool) });
                    if (setActive != null)
                    {
                        setActive.Invoke(instance, new object[] { on });
                        Debug.Log($"{Tag} AetherVision (instance) -> {on}");
                        return;
                    }
                }
                Debug.LogWarning($"{Tag} AetherVisionOverlay found ({overlayType.FullName}) but no SetActive(bool) — capturing without toggling.");
            }
            catch (Exception ex)
            {
                // Loud log per no-silent-fail mandate; do not rethrow because shot
                // capture must continue even if overlay wiring is broken.
                Debug.LogError($"{Tag} TryToggleAetherVision threw {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static Type FindTypeByFullName(string fullName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType(fullName, throwOnError: false, ignoreCase: false);
                if (t != null) return t;
            }
            return null;
        }

        // ---- Minimal editor-side coroutine helpers ----
        // We avoid taking a dependency on Unity.EditorCoroutines.Editor because not
        // every checkout has that package. These mimic just enough to walk a
        // sequence of yields with a one-second delay between shots.

        private class WaitForSecondsLite
        {
            public readonly float seconds;
            public WaitForSecondsLite(float seconds) { this.seconds = seconds; }
        }

        private static class EditorCoroutineLite
        {
            private static readonly List<RunningRoutine> _running = new List<RunningRoutine>();

            private class RunningRoutine
            {
                public System.Collections.IEnumerator routine;
                public double resumeAt;
            }

            public static void Start(System.Collections.IEnumerator routine)
            {
                var r = new RunningRoutine { routine = routine, resumeAt = EditorApplication.timeSinceStartup };
                _running.Add(r);
                EditorApplication.update -= Pump;
                EditorApplication.update += Pump;
            }

            private static void Pump()
            {
                for (int i = _running.Count - 1; i >= 0; i--)
                {
                    var r = _running[i];
                    if (EditorApplication.timeSinceStartup < r.resumeAt) continue;
                    bool moved;
                    try
                    {
                        moved = r.routine.MoveNext();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"{Tag} Capture routine threw {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                        _running.RemoveAt(i);
                        continue;
                    }
                    if (!moved)
                    {
                        _running.RemoveAt(i);
                        continue;
                    }
                    if (r.routine.Current is WaitForSecondsLite w)
                    {
                        r.resumeAt = EditorApplication.timeSinceStartup + w.seconds;
                    }
                }
                if (_running.Count == 0)
                {
                    EditorApplication.update -= Pump;
                }
            }
        }
    }
}
