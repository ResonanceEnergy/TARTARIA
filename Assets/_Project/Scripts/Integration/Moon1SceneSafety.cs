// Moon1SceneSafety.cs
// 2026-06-03 ANTI-CIRCLING MANDATE — converted from runtime daemon to one-shot sentinel.
//
// Per CLAUDE.md (2026-06-03 late-evening anti-circling mandate): runtime daemons are
// debt, not fixes. All 6 prior guards (G3/G4/G7/G8/G9/G10) are now obsolete or no-op:
//
//   G3/G10 (focus/runInBackground)  → RunInBackgroundGuard.cs (BeforeSceneLoad) owns.
//   G4     (duplicate AudioListeners) → root cause was the dup Main Camera (G8),
//                                       which itself was caused by Moon1PlayerSetup.cs
//                                       compiling `AddComponent<MonoBehaviour>()` from
//                                       a broken `/* DISABLED: */` comment block.
//                                       Now fixed to `AddComponent<CameraController>()`.
//   G7     (FadeImage opaque overlay) → no FadeImage GameObject exists in
//                                       Echohaven_VerticalSlice.unity. Always no-op.
//   G8     (duplicate Main Camera)    → fixed at the source (Moon1PlayerSetup.cs).
//   G9     (brown fog)                → scene already authored at
//                                       fogColor=(0.85,0.72,0.52), fogDensity=0.005,
//                                       which is below the 0.008 trigger. Always no-op.
//
// This file now exists only as a SENTINEL: it runs ONCE ~3s after AfterSceneLoad,
// inspects the 5 conditions, and LogWarnings if any regression is detected — but
// performs zero mutation. The goal is to catch a future regression cheaply instead
// of papering over it with a per-frame correction.
//
// Sibling component Moon1PlayerSafety was deleted 2026-06-03 once Player.prefab
// was authored to nest Char_Knight as the _CharacterVisual child (no more capsule
// mesh, no more Player_Limbs material → both G1/G2 sentinels obsolete).

using UnityEngine;

namespace Tartaria.Integration
{
    internal static class Moon1SceneSafety
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            var go = new GameObject("Moon1SceneSentinel");
            go.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            UnityEngine.Object.DontDestroyOnLoad(go);
            go.AddComponent<Sentinel>();
        }

        internal class Sentinel : MonoBehaviour
        {
            void Start()
            {
                // Wait ~3s so other AfterSceneLoad bootstraps (PlayerSpawner, content
                // spawners, HUD builders) have finished populating the scene. Then
                // inspect ONCE and never touch anything again.
                Invoke(nameof(ValidateOnce), 3.0f);
            }

            void ValidateOnce()
            {
                CheckDuplicateCameras();
                CheckDuplicateAudioListeners();
                CheckFadeOverlay();
                CheckFog();
                CheckRunInBackground();

                // Self-destruct — the sentinel has done its single job.
                Destroy(gameObject);
            }

            // --- duplicate Main Cameras (was G8) ----------------------------------------
            void CheckDuplicateCameras()
            {
                var cams = UnityEngine.Object.FindObjectsByType<UnityEngine.Camera>(FindObjectsSortMode.None);
                int mainCount = 0;
                foreach (var c in cams)
                {
                    if (c == null) continue;
                    if (!c.enabled || !c.gameObject.activeInHierarchy) continue;
                    if (c.CompareTag("MainCamera")) mainCount++;
                }
                if (mainCount > 1)
                {
                    Debug.LogWarning(
                        $"[Moon1SceneSafety SENTINEL] {mainCount} duplicate Main Cameras found in scene — " +
                        "Moon1PlayerSetup.cs spawning extras? See CLAUDE.md anti-circling mandate.");
                }
            }

            // --- duplicate AudioListeners (was G4) --------------------------------------
            void CheckDuplicateAudioListeners()
            {
                var listeners = UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
                int enabledCount = 0;
                foreach (var l in listeners)
                {
                    if (l == null) continue;
                    if (l.enabled && l.gameObject.activeInHierarchy) enabledCount++;
                }
                if (enabledCount > 1)
                {
                    Debug.LogWarning(
                        $"[Moon1SceneSafety SENTINEL] {enabledCount} enabled AudioListeners — root cause: spawn-side bug.");
                }
            }

            // --- fullscreen opaque fade overlay (was G7) --------------------------------
            void CheckFadeOverlay()
            {
                var images = UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Image>(FindObjectsSortMode.None);
                foreach (var img in images)
                {
                    if (img == null) continue;
                    if (img.name.IndexOf("Fade", System.StringComparison.OrdinalIgnoreCase) < 0) continue;
                    if (img.color.a < 0.99f) continue;

                    var rt = img.rectTransform;
                    if (rt == null) continue;
                    var corners = new Vector3[4];
                    rt.GetWorldCorners(corners);
                    float w = (corners[2] - corners[1]).magnitude;
                    float h = (corners[1] - corners[0]).magnitude;
                    if (w < 400 || h < 300) continue;

                    Debug.LogWarning(
                        $"[Moon1SceneSafety SENTINEL] Fullscreen opaque fade overlay '{img.name}' detected " +
                        "(alpha=1, size>=400x300) — root cause: scene authoring or fade animator default.");
                    return; // one warning is enough
                }
            }

            // --- brown heavy fog (was G9) -----------------------------------------------
            void CheckFog()
            {
                if (!RenderSettings.fog) return;
                var c = RenderSettings.fogColor;
                bool isBrownish = c.r > 0.20f && c.g > 0.18f && c.b < 0.30f && c.r >= c.b && c.g >= c.b;
                if (!isBrownish) return;
                if (RenderSettings.fogDensity <= 0.008f) return;

                Debug.LogWarning(
                    $"[Moon1SceneSafety SENTINEL] Heavy brown fog (density={RenderSettings.fogDensity:F4}) — " +
                    "fix in scene RenderSettings.");
            }

            // --- runInBackground (was G3/G10) -------------------------------------------
            void CheckRunInBackground()
            {
                if (!Application.runInBackground)
                {
                    Debug.LogWarning(
                        "[Moon1SceneSafety SENTINEL] runInBackground=false — RunInBackgroundGuard should own this.");
                }
            }
        }
    }
}
