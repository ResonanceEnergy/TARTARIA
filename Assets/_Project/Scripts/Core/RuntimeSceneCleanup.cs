using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tartaria.Core
{
    /// <summary>
    /// RuntimeSceneCleanup — fires once after every scene load to fix two
    /// recurring runtime issues that crop up because Unity spawns DontDestroyOnLoad
    /// + Player + prefab-attached AudioListeners we can't easily edit ourselves:
    ///
    ///   1. AudioListener dedup. Keeps the one on the active Camera (Camera.main
    ///      or the first found), destroys all extras. Stops the per-frame
    ///      "There are N audio listeners" warning flood.
    ///   2. Magenta material probe (Editor + dev builds only). Logs every Renderer
    ///      whose material has shader == null or "Hidden/InternalErrorShader".
    ///      One-shot at scene load — gives a precise list so the offending prefabs
    ///      can be fixed.
    /// </summary>
    public static class RuntimeSceneCleanup
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnSceneLoaded()
        {
            // Run twice — once immediately, and once 1 second later to catch
            // late-spawned listeners (Player prefab spawns on PlayerSpawner.Start).
            var runner = new GameObject("RuntimeSceneCleanup_Runner");
            Object.DontDestroyOnLoad(runner);
            runner.AddComponent<RuntimeSceneCleanupRunner>();
        }

        public static void DedupAudioListeners()
        {
            var listeners = Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (listeners.Length <= 1) return;

            // Prefer the one on Camera.main, else the first
            AudioListener keeper = null;
            var mainCam = UnityEngine.Camera.main;
            if (mainCam != null)
            {
                foreach (var l in listeners)
                {
                    if (l.gameObject == mainCam.gameObject) { keeper = l; break; }
                }
            }
            if (keeper == null) keeper = listeners[0];

            int removed = 0;
            foreach (var l in listeners)
            {
                if (l == keeper) continue;
                Object.Destroy(l);
                removed++;
            }
            Debug.Log($"[RuntimeSceneCleanup] AudioListener dedup: kept {keeper.gameObject.name}, removed {removed} duplicate(s).");
        }

        public static void ProbeMagentaMaterials()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var renderers = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int magenta = 0;
            var bad = new List<string>();
            foreach (var r in renderers)
            {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    var m = mats[i];
                    if (m == null) continue;
                    string shaderName = m.shader != null ? m.shader.name : "<null>";
                    // Magenta in URP includes:
                    //   - broken/null shaders
                    //   - Built-in Standard / Legacy / Mobile (URP can't render them → magenta)
                    bool isMagentaSource =
                        shaderName == "<null>" ||
                        shaderName == "Hidden/InternalErrorShader" ||
                        shaderName == "Standard" ||
                        shaderName == "Standard (Specular setup)" ||
                        shaderName.StartsWith("Legacy Shaders/") ||
                        shaderName.StartsWith("Mobile/Diffuse") ||
                        shaderName == "Diffuse";
                    if (isMagentaSource)
                    {
                        magenta++;
                        bad.Add($"{r.gameObject.name} [{m.name}] shader={shaderName}");
                    }
                }
            }
            if (magenta == 0)
            {
                Debug.Log("[RuntimeSceneCleanup] Magenta probe: no broken-shader renderers in scene at load time.");
            }
            else
            {
                Debug.LogWarning($"[RuntimeSceneCleanup] Magenta probe: {magenta} renderer(s) with broken/non-URP shaders:\n  " + string.Join("\n  ", bad));
            }
#endif
        }
    }

    /// <summary>Hidden runner that does deferred cleanup forever (catches late-spawned content).</summary>
    public class RuntimeSceneCleanupRunner : MonoBehaviour
    {
        IEnumerator Start()
        {
            // First pass immediately (catches scene-saved listeners + obvious magenta)
            RuntimeSceneCleanup.DedupAudioListeners();
            RuntimeSceneCleanup.ProbeMagentaMaterials();

            // 2nd pass at 1.5s (catches Player prefab spawn)
            yield return new WaitForSeconds(1.5f);
            RuntimeSceneCleanup.DedupAudioListeners();
            RuntimeSceneCleanup.ProbeMagentaMaterials();

            // 3rd pass at 4s (catches combat arena wave 1)
            yield return new WaitForSeconds(2.5f);
            RuntimeSceneCleanup.DedupAudioListeners();
            RuntimeSceneCleanup.ProbeMagentaMaterials();

            // Continuous dedup loop — every 5s forever — catches late-spawned NPCs/Mud Golems/etc.
            // Only the dedup runs frequently; the probe is more expensive and runs every 30s.
            int loops = 0;
            while (true)
            {
                yield return new WaitForSeconds(5f);
                RuntimeSceneCleanup.DedupAudioListeners();
                loops++;
                if (loops % 6 == 0) RuntimeSceneCleanup.ProbeMagentaMaterials(); // ~30s
            }
        }
    }
}
