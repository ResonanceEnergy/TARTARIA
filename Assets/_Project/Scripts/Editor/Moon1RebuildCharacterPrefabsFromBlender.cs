#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon1RebuildCharacterPrefabsFromBlender — overwrites the 4 stub character prefabs
    /// (Milo / Cassian / Anastasia / Lirael) under Assets/_Project/Prefabs/Characters/
    /// with rebuilt prefabs whose root is the real Blender FBX model. Preserves each
    /// prefab's GUID so the 9 hardcoded path references in code keep working.
    ///
    /// The 10.6KB stub prefabs (primitive capsule + character controller + NPC script)
    /// become full-mesh prefabs backed by Models/Blender/Moon1/*.fbx.
    ///
    /// Per CLAUDE.md no-stubs mandate: characters must load as real models.
    /// </summary>
    public static class Moon1RebuildCharacterPrefabsFromBlender
    {
        // Map: stub-prefab-path → Blender FBX path (the source mesh + skeleton)
        static readonly (string stubPrefab, string blenderFBX, string componentNamespace)[] Map = {
            ("Assets/_Project/Prefabs/Characters/Milo.prefab",      "Assets/_Project/Models/Blender/Moon1/MiloBoy.fbx",           "Tartaria.Integration.MiloController"),
            ("Assets/_Project/Prefabs/Characters/Cassian.prefab",   "Assets/_Project/Models/Blender/Moon1/CassianCarter.fbx",     "Tartaria.Integration.CassianController"),
            ("Assets/_Project/Prefabs/Characters/Anastasia.prefab", "Assets/_Project/Models/Blender/Moon1/AnastasiaPrincess.fbx", "Tartaria.Integration.AnastasiaController"),
            ("Assets/_Project/Prefabs/Characters/Lirael.prefab",    "Assets/_Project/Models/Blender/Moon1/LiraelGuardian.fbx",    "Tartaria.Integration.LiraelController"),
        };

        [MenuItem("Tartaria/1 Build/Rebuild Character Prefabs from Blender FBX (Milo/Cassian/Anastasia/Lirael)", priority = 50)]
        public static void Rebuild()
        {
            var report = new System.Text.StringBuilder();
            int rebuilt = 0, skipped = 0;

            foreach (var (stubPath, fbxPath, controllerType) in Map)
            {
                if (!File.Exists(fbxPath))
                {
                    report.AppendLine($"✗ {Path.GetFileName(stubPath)} — FBX not found: {fbxPath}");
                    skipped++;
                    continue;
                }

                var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
                if (fbx == null)
                {
                    report.AppendLine($"✗ {Path.GetFileName(stubPath)} — FBX failed to load");
                    skipped++;
                    continue;
                }

                // Instantiate FBX root in scene (temporary)
                var go = (GameObject)PrefabUtility.InstantiatePrefab(fbx);
                if (go == null)
                {
                    report.AppendLine($"✗ {Path.GetFileName(stubPath)} — could not instantiate FBX");
                    skipped++;
                    continue;
                }

                // Unpack the FBX prefab connection so we can save it as a NEW prefab
                // (otherwise SaveAsPrefabAsset writes a variant link, not a flat prefab).
                PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

                // Rename root to the canonical character name (strip FBX suffix)
                go.name = Path.GetFileNameWithoutExtension(stubPath);

                // Wire essential gameplay components (best-effort via reflection — controllers
                // live in Tartaria.Integration asmdef which Editor can't always see at compile-time)
                EnsureCharacterController(go);
                EnsureNavMeshAgent(go);
                EnsureTagAndLayer(go, Path.GetFileNameWithoutExtension(stubPath));
                EnsureControllerByName(go, controllerType);
                EnsureAnimator(go);

                // Overwrite the stub prefab. PrefabUtility.SaveAsPrefabAsset OVERWRITES
                // the file at the destination path while keeping the same .meta (= GUID).
                var saved = PrefabUtility.SaveAsPrefabAsset(go, stubPath, out bool success);
                Object.DestroyImmediate(go);

                if (success && saved != null)
                {
                    report.AppendLine($"✓ {Path.GetFileName(stubPath)} ← {Path.GetFileName(fbxPath)}");
                    rebuilt++;
                }
                else
                {
                    report.AppendLine($"✗ {Path.GetFileName(stubPath)} — SaveAsPrefabAsset failed");
                    skipped++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Rebuild Character Prefabs from Blender",
                $"Rebuilt {rebuilt} / {Map.Length} prefabs.\n\n{report}\n" +
                "Hardcoded paths to /Prefabs/Characters/*.prefab now resolve to full-mesh\n" +
                "Blender models (GUIDs preserved). Hit Play to verify.",
                "OK");
        }

        // ─── Helpers ────────────────────────────────────────────────────────────

        static void EnsureCharacterController(GameObject go)
        {
            if (go.GetComponent<CharacterController>() != null) return;
            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.35f;
            cc.center = new Vector3(0f, 0.9f, 0f);
            cc.slopeLimit = 45f;
            cc.stepOffset = 0.3f;
        }

        static void EnsureNavMeshAgent(GameObject go)
        {
            var existing = go.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (existing != null) return;
            var agent = go.AddComponent<UnityEngine.AI.NavMeshAgent>();
            agent.speed = 2.5f;
            agent.angularSpeed = 240f;
            agent.acceleration = 12f;
            agent.stoppingDistance = 1.2f;
            agent.radius = 0.35f;
            agent.height = 1.8f;
        }

        static void EnsureAnimator(GameObject go)
        {
            // FBX import typically already adds an Animator if it has a rig.
            var anim = go.GetComponent<Animator>();
            if (anim == null) anim = go.AddComponent<Animator>();
            anim.applyRootMotion = false;
        }

        static void EnsureTagAndLayer(GameObject go, string charName)
        {
            // Set NPC layer if it exists, otherwise leave on Default
            int npcLayer = LayerMask.NameToLayer("NPC");
            if (npcLayer >= 0) go.layer = npcLayer;

            // Tag (only if defined)
            try
            {
                var tagged = (charName == "Lirael" || charName == "Cassian" || charName == "Milo" || charName == "Anastasia")
                    ? "NPC" : "Untagged";
                if (UnityEditorInternal.InternalEditorUtility.tags != null)
                {
                    foreach (var t in UnityEditorInternal.InternalEditorUtility.tags)
                        if (t == tagged) { go.tag = tagged; break; }
                }
            }
            catch { /* tag system unavailable in some contexts */ }
        }

        static void EnsureControllerByName(GameObject go, string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) return;
            // Try resolve through every loaded assembly (Tartaria.Integration etc.)
            System.Type type = null;
            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(typeName);
                if (type != null) break;
            }
            if (type == null) return; // controller not in scope (asmdef boundary) — skip silently
            if (go.GetComponent(type) != null) return;
            go.AddComponent(type);
        }
    }
}
#endif
