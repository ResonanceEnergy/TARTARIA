// Moon1MudGolemRewireOneShot.cs — 2026-06-04 LATE LOCK-DOWN
//
// Auto-fires once on next Unity Editor launch to:
//   1. Load the new Moon1/MudGolem.fbx (real 2.5m Blender mesh, replaces primitive cluster)
//   2. Instantiate, add Tartaria.AI.MudGolemAI + Health + LootDrop, NavMeshAgent, 2.5m CapsuleCollider
//   3. Save as Prefabs/Characters/MudGolem.prefab AND Resources/Enemies/MudGolem.prefab
//   4. Mark EditorPref so it doesn't re-fire
//
// Once it runs successfully it self-disables. NATRIX can delete the file after verifying.
// If something goes wrong, EditorPref reset via Tartaria/8 Fix/Reset MudGolem Rewire OneShot

using UnityEngine;
using UnityEditor;
using System.IO;

namespace Tartaria.Editor.OneShots
{
    [InitializeOnLoad]
    public static class Moon1MudGolemRewireOneShot
    {
        const string PREF_KEY = "Tartaria.OneShot.MudGolemRewire.2026-06-04";
        const string FBX_PATH = "Assets/_Project/Models/Blender/Moon1/MudGolem.fbx";
        const string PREFAB_A = "Assets/_Project/Prefabs/Characters/MudGolem.prefab";
        const string PREFAB_B = "Assets/_Project/Resources/Enemies/MudGolem.prefab";

        static Moon1MudGolemRewireOneShot()
        {
            if (EditorPrefs.GetBool(PREF_KEY, false)) return;
            EditorApplication.delayCall += () => Run();
        }

        [MenuItem("Tartaria/8 Fix/Reset MudGolem Rewire OneShot", priority = 999)]
        static void ResetFlag()
        {
            EditorPrefs.DeleteKey(PREF_KEY);
            Debug.Log("[MudGolemRewireOneShot] Flag cleared. Will fire again on next domain reload.");
        }

        [MenuItem("Tartaria/8 Fix/Run MudGolem Rewire NOW", priority = 998)]
        static void RunNow() => Run();

        static void Run()
        {
            if (EditorPrefs.GetBool(PREF_KEY, false)) {
                Debug.Log("[MudGolemRewireOneShot] Already ran this session. Skip.");
                return;
            }

            // 1. Load FBX
            var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(FBX_PATH);
            if (fbx == null) {
                Debug.LogError($"[MudGolemRewireOneShot] FBX not found at {FBX_PATH}. Aborting.");
                return;
            }

            // 2. Verify the FBX has a Renderer (real mesh, not LFS pointer)
            var rs = fbx.GetComponentsInChildren<Renderer>(true);
            if (rs == null || rs.Length == 0) {
                Debug.LogError($"[MudGolemRewireOneShot] FBX has no Renderer. Possibly LFS pointer. Aborting.");
                return;
            }
            var bounds = rs[0].bounds;
            for (int i=1; i<rs.Length; i++) bounds.Encapsulate(rs[i].bounds);
            Debug.Log($"[MudGolemRewireOneShot] FBX bounds = {bounds.size} (target ~2.5m Y)");

            // 3. Instantiate fresh
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(fbx);
            try {
                instance.name = "MudGolem";

                // Add AI components by type (avoid hard ref to Tartaria.AI types in case asmdef boundary)
                AddIfMissing(instance, "Tartaria.AI.MudGolemAI");
                AddIfMissing(instance, "Tartaria.AI.MudGolemHealth");
                AddIfMissing(instance, "Tartaria.AI.MudGolemLootDrop");

                // NavMeshAgent
                if (instance.GetComponent<UnityEngine.AI.NavMeshAgent>() == null) {
                    var na = instance.AddComponent<UnityEngine.AI.NavMeshAgent>();
                    na.radius = 0.6f;
                    na.height = 2.5f;
                    na.baseOffset = 0f;
                    na.speed = 2.5f;
                    na.angularSpeed = 180f;
                    na.acceleration = 6f;
                    na.stoppingDistance = 1.5f;
                }

                // CapsuleCollider 2.5m
                var cc = instance.GetComponent<CapsuleCollider>();
                if (cc == null) cc = instance.AddComponent<CapsuleCollider>();
                cc.height = 2.5f;
                cc.center = new Vector3(0f, 1.25f, 0f);
                cc.radius = 0.6f;
                cc.direction = 1; // Y-axis

                // Rigidbody for combat physics (kinematic to avoid drift)
                var rb = instance.GetComponent<Rigidbody>();
                if (rb == null) rb = instance.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.useGravity = false;

                // 4. Save as 2 prefabs
                EnsureDir(PREFAB_A);
                EnsureDir(PREFAB_B);
                PrefabUtility.SaveAsPrefabAsset(instance, PREFAB_A);
                PrefabUtility.SaveAsPrefabAsset(instance, PREFAB_B);
                Debug.Log($"[MudGolemRewireOneShot] OK Wrote {PREFAB_A} + {PREFAB_B}");
            }
            finally {
                Object.DestroyImmediate(instance);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorPrefs.SetBool(PREF_KEY, true);
            Debug.Log("[MudGolemRewireOneShot] Complete. Flag set; will not re-fire.");
        }

        static void AddIfMissing(GameObject go, string typeFullName)
        {
            var existing = go.GetComponent(typeFullName);
            if (existing != null) return;
            var t = System.Type.GetType(typeFullName + ", Tartaria.AI")
                 ?? System.Type.GetType(typeFullName);
            if (t == null) {
                // Try across all loaded assemblies
                foreach (var a in System.AppDomain.CurrentDomain.GetAssemblies()) {
                    t = a.GetType(typeFullName);
                    if (t != null) break;
                }
            }
            if (t != null) {
                go.AddComponent(t);
                Debug.Log($"[MudGolemRewireOneShot]   + {typeFullName}");
            }
            else {
                Debug.LogWarning($"[MudGolemRewireOneShot] Type {typeFullName} not found - skipped.");
            }
        }

        static void EnsureDir(string assetPath)
        {
            var dir = Path.GetDirectoryName(assetPath).Replace("\\", "/");
            if (!AssetDatabase.IsValidFolder(dir)) {
                var parts = dir.Split('/');
                var build = parts[0];
                for (int i=1; i<parts.Length; i++) {
                    var next = build + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(build, parts[i]);
                    build = next;
                }
            }
        }
    }
}
