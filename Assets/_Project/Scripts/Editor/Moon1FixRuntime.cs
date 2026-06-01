#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Tartaria.Integration;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon1FixRuntime — bundles three one-click fixes for issues that surfaced
    /// during the 2026-05-30 Play-mode smoke test:
    ///
    ///   1. AudioListener missing on Main Camera → flood of "no audio listeners" warnings
    ///   2. Magenta materials (KayKit/vendor assets using Built-in Standard shader)
    ///      → reassign Standard / Diffuse / Mobile-Diffuse to URP/Lit so they render
    ///   3. EchohavenContentSpawner GameObject with broken script reference
    ///      ("Tartaria.Integration.Tartaria.Integration.EchohavenContentSpawner") →
    ///      delete + re-add the active class
    /// </summary>
    public static class Moon1FixRuntime
    {
        // ─── 1. Audio listener fix ───

        [MenuItem("Tartaria/8 Fix/Ensure Exactly One AudioListener", priority = 820)]
        public static void FixAudioListener()
        {
            var listeners = UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            // Case 1: zero listeners — add one to the main camera
            if (listeners.Length == 0)
            {
                var cam = UnityEngine.Camera.main;
                if (cam == null)
                    cam = UnityEngine.Object.FindFirstObjectByType<UnityEngine.Camera>(FindObjectsInactive.Include);
                if (cam == null)
                {
                    EditorUtility.DisplayDialog("AudioListener", "No Camera found in scene.", "OK");
                    return;
                }
                Undo.AddComponent<AudioListener>(cam.gameObject);
                EditorSceneManager.MarkSceneDirty(cam.gameObject.scene);
                Debug.Log($"[Moon1FixRuntime] AudioListener added to {cam.gameObject.name} (was 0)");
                return;
            }

            // Case 2: exactly one — nothing to do
            if (listeners.Length == 1)
            {
                Debug.Log($"[Moon1FixRuntime] AudioListener OK: 1 on {listeners[0].gameObject.name}");
                return;
            }

            // Case 3: multiple — keep the one on the main camera (or the first), delete the rest
            var preferred = System.Array.Find(listeners, l =>
                l.gameObject == UnityEngine.Camera.main?.gameObject);
            if (preferred == null) preferred = listeners[0];

            int removed = 0;
            foreach (var l in listeners)
            {
                if (l == preferred) continue;
                Debug.Log($"[Moon1FixRuntime] Removing duplicate AudioListener from {l.gameObject.name}");
                Undo.DestroyObjectImmediate(l);
                removed++;
            }
            EditorSceneManager.MarkSceneDirty(preferred.gameObject.scene);
            Debug.Log($"[Moon1FixRuntime] AudioListener cleaned: kept on {preferred.gameObject.name}, removed {removed}");
        }

        // ─── 2. URP material fix ───

        [MenuItem("Tartaria/8 Fix/Convert Magenta Materials → URP-Lit", priority = 810)]
        public static void FixMagentaMaterials()
        {
            var urpLit = Shader.Find("Universal Render Pipeline/Lit");
            var urpSimpleLit = Shader.Find("Universal Render Pipeline/Simple Lit");
            var urpUnlit = Shader.Find("Universal Render Pipeline/Unlit");
            if (urpLit == null)
            {
                EditorUtility.DisplayDialog("URP Conversion",
                    "Could not find Universal Render Pipeline/Lit shader. Is URP installed?", "OK");
                return;
            }

            var renderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int converted = 0;
            int alreadyURP = 0;
            int nullSkipped = 0;
            var seen = new HashSet<Material>();

            foreach (var r in renderers)
            {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    var m = mats[i];
                    if (m == null) { nullSkipped++; continue; }
                    if (!seen.Add(m)) continue;

                    string shaderName = m.shader != null ? m.shader.name : null;
                    // Anything that ISN'T explicitly URP/Built-in URP shader is treated as
                    // potential magenta. Catches Built-in Standard, Legacy, Mobile/Diffuse,
                    // Surface Shaders, and broken/null shaders.
                    bool isURP = shaderName != null && (
                        shaderName.StartsWith("Universal Render Pipeline/") ||
                        shaderName.StartsWith("Shader Graphs/") ||
                        shaderName.StartsWith("UI/") ||
                        shaderName.StartsWith("TextMeshPro/") ||
                        shaderName.StartsWith("Sprites/") ||
                        shaderName.StartsWith("Hidden/Universal Render Pipeline/") ||
                        shaderName == "Skybox/Procedural" ||
                        shaderName.StartsWith("Skybox/")
                    );

                    if (isURP) { alreadyURP++; continue; }

                    // Preserve common texture properties when reassigning shader
                    Texture mainTex = m.HasProperty("_MainTex") ? m.GetTexture("_MainTex") : null;
                    Color col = m.HasProperty("_Color") ? m.GetColor("_Color") : Color.white;

                    Debug.Log($"[Moon1FixRuntime] Converting material '{m.name}' (shader='{shaderName}') to URP/Lit");
                    m.shader = urpLit;
                    if (mainTex != null && m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", mainTex);
                    if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", col);
                    EditorUtility.SetDirty(m);
                    converted++;
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[Moon1FixRuntime] Material conversion: {converted} converted to URP/Lit, {alreadyURP} already URP, {nullSkipped} null slots.");
            EditorUtility.DisplayDialog("Material Conversion",
                $"{converted} materials reassigned to URP/Lit\n{alreadyURP} already on URP\n{nullSkipped} null slots skipped",
                "OK");
        }

        [MenuItem("Tartaria/7 Diagnose/List All Material Shaders In Scene", priority = 750)]
        public static void DiagnoseMaterials()
        {
            var renderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var counts = new Dictionary<string, int>();
            var seen = new HashSet<Material>();
            int nullMaterials = 0;
            int nullShaders = 0;
            int nullMainTex = 0;

            foreach (var r in renderers)
            {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    var m = mats[i];
                    if (m == null) { nullMaterials++; continue; }
                    if (!seen.Add(m)) continue;

                    string key = m.shader != null ? m.shader.name : "<NULL SHADER>";
                    if (m.shader == null) nullShaders++;
                    counts[key] = counts.TryGetValue(key, out var c) ? c + 1 : 1;

                    if (m.HasProperty("_BaseMap") && m.GetTexture("_BaseMap") == null &&
                        m.HasProperty("_MainTex") && m.GetTexture("_MainTex") == null)
                        nullMainTex++;
                }
            }

            var lines = new List<string>();
            lines.Add($"=== Material shader census ({seen.Count} unique) ===");
            foreach (var kv in counts)
                lines.Add($"  {kv.Value,4}× {kv.Key}");
            lines.Add($"  --");
            lines.Add($"  Null material slots: {nullMaterials}");
            lines.Add($"  Materials with null shader: {nullShaders}");
            lines.Add($"  Materials with null BaseMap+MainTex: {nullMainTex}");
            Debug.Log(string.Join("\n", lines));
        }

        // ─── 3. Re-attach EchohavenContentSpawner script ───

        [MenuItem("Tartaria/8 Fix/Re-attach EchohavenContentSpawner Script", priority = 830)]
        public static void FixSpawnerScript()
        {
            // Find any GameObject named EchohavenContentSpawner (broken refs still exist as MonoBehaviour-less components)
            var candidates = GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            GameObject brokenGO = null;
            foreach (var go in candidates)
            {
                if (go.name == "EchohavenContentSpawner")
                {
                    brokenGO = go;
                    break;
                }
            }

            if (brokenGO == null)
            {
                EditorUtility.DisplayDialog("Spawner Fix",
                    "No GameObject named 'EchohavenContentSpawner' found in scene. Use Tartaria → Wire Echohaven Content Spawner to create one.", "OK");
                return;
            }

            // Delete and recreate to clear stale serialized script ref
            Vector3 pos = brokenGO.transform.position;
            string name = brokenGO.name;
            Undo.DestroyObjectImmediate(brokenGO);

            var fresh = new GameObject(name);
            fresh.transform.position = pos;
            var spawner = fresh.AddComponent<EchohavenContentSpawner>();
            Undo.RegisterCreatedObjectUndo(fresh, "Recreate EchohavenContentSpawner");

            // Auto-assign MudGolem prefab via SerializedObject
            const string MUD_GOLEM_PATH = "Assets/_Project/Prefabs/Enemies/Moon1_MudGolem/MudGolem.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MUD_GOLEM_PATH);
            if (prefab != null)
            {
                var so = new SerializedObject(spawner);
                var prop = so.FindProperty("mudGolemPrefab");
                if (prop != null)
                {
                    prop.objectReferenceValue = prefab;
                    so.ApplyModifiedProperties();
                }
            }

            EditorSceneManager.MarkSceneDirty(fresh.scene);
            Selection.activeGameObject = fresh;
            Debug.Log("[Moon1FixRuntime] EchohavenContentSpawner GameObject recreated with fresh script reference.");
        }

        // ─── Convenience: do all three at once ───

        [MenuItem("Tartaria/8 Fix/ALL Moon 1 Runtime Issues", priority = 800)]
        public static void FixAll()
        {
            FixAudioListener();
            FixMagentaMaterials();
            FixSpawnerScript();
            Debug.Log("[Moon1FixRuntime] All Moon 1 runtime fixes attempted. Save the scene (Ctrl+S) and Play.");
        }
    }
}
#endif
