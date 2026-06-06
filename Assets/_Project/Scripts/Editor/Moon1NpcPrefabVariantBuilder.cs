// P5.L1 — Build Moon 1 NPC prefab variants that point at the real Blender FBX
// assets (LFS-pulled). Sprint 11 L6 e9bbc612 found Lirael/Anastasia/Cassian/Milo
// shipping as unanimated capsules with zero MonoBehaviours. This tool overwrites
// each NPC prefab with a Prefab Variant whose root model is the FBX and whose
// added components are: CapsuleCollider, NavMeshAgent, the controller, tag=NPC.
//
// Invoke headless:
//   Unity.exe -batchmode -nographics -quit -projectPath <worktree>
//             -executeMethod Tartaria.Editor.Moon1NpcPrefabVariantBuilder.BuildAll

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Tartaria.Integration;

namespace Tartaria.Editor
{
    public static class Moon1NpcPrefabVariantBuilder
    {
        private struct NpcSpec
        {
            public string Name;
            public string FbxPath;
            public string PrefabPath;
            public Type ControllerType;
        }

        private static readonly NpcSpec[] Specs = new[]
        {
            new NpcSpec
            {
                Name = "Lirael",
                FbxPath = "Assets/_Project/Models/Blender/Moon1/LiraelGuardian.fbx",
                PrefabPath = "Assets/_Project/Prefabs/Characters/Lirael.prefab",
                ControllerType = typeof(LiraelController),
            },
            new NpcSpec
            {
                Name = "Anastasia",
                FbxPath = "Assets/_Project/Models/Blender/Moon1/AnastasiaPrincess.fbx",
                PrefabPath = "Assets/_Project/Prefabs/Characters/Anastasia.prefab",
                ControllerType = typeof(AnastasiaController),
            },
            new NpcSpec
            {
                Name = "Cassian",
                FbxPath = "Assets/_Project/Models/Blender/Moon1/CassianCarter.fbx",
                PrefabPath = "Assets/_Project/Prefabs/Characters/Cassian.prefab",
                ControllerType = typeof(CassianNPCController),
            },
            new NpcSpec
            {
                Name = "Milo",
                FbxPath = "Assets/_Project/Models/Blender/Moon1/MiloBoy.fbx",
                PrefabPath = "Assets/_Project/Prefabs/Characters/Milo.prefab",
                ControllerType = typeof(MiloController),
            },
        };

        [MenuItem("Tartaria/5 Phase 5/Build Moon 1 NPC Prefab Variants")]
        public static void BuildAll()
        {
            int built = 0;
            var report = new List<string>();
            report.Add("[P5.L1] NPC Prefab Variant build report");

            foreach (var spec in Specs)
            {
                try
                {
                    BuildOne(spec, report);
                    built++;
                }
                catch (Exception e)
                {
                    report.Add($"  FAIL {spec.Name}: {e.GetType().Name}: {e.Message}");
                    Debug.LogError($"[P5.L1] {spec.Name} failed: {e}");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            report.Add($"[P5.L1] Done. {built}/{Specs.Length} NPC variants built.");
            Debug.Log(string.Join("\n", report));
        }

        private static void BuildOne(NpcSpec spec, List<string> report)
        {
            // 1. Verify FBX is real (not LFS pointer)
            var fbxFull = Path.GetFullPath(spec.FbxPath);
            if (!File.Exists(fbxFull))
                throw new FileNotFoundException($"FBX missing: {spec.FbxPath}");
            var sz = new FileInfo(fbxFull).Length;
            if (sz < 10 * 1024)
                throw new InvalidDataException(
                    $"FBX is LFS pointer ({sz} B): {spec.FbxPath}. Run 'git lfs pull'.");

            // 2. Force animationType = Generic on the FBX import
            var importer = AssetImporter.GetAtPath(spec.FbxPath) as ModelImporter;
            if (importer == null)
                throw new InvalidOperationException(
                    $"No ModelImporter for {spec.FbxPath}");
            bool importerDirty = false;
            if (importer.animationType != ModelImporterAnimationType.Generic)
            {
                importer.animationType = ModelImporterAnimationType.Generic;
                importerDirty = true;
            }
            if (importerDirty)
            {
                importer.SaveAndReimport();
                report.Add($"  {spec.Name} FBX animationType -> Generic ({sz} B)");
            }
            else
            {
                report.Add($"  {spec.Name} FBX animationType already Generic ({sz} B)");
            }

            // 3. Load the model prefab and instantiate it as a variant base
            var modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(spec.FbxPath);
            if (modelPrefab == null)
                throw new InvalidOperationException(
                    $"Could not load model prefab at {spec.FbxPath}");

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab);
            if (instance == null)
                throw new InvalidOperationException(
                    $"InstantiatePrefab returned null for {spec.FbxPath}");

            try
            {
                instance.name = spec.Name;

                // 4. Tag the root NPC (skip if a future engine version lacks tag)
                try { instance.tag = "NPC"; }
                catch (Exception tagEx) { report.Add($"    tag set skipped: {tagEx.Message}"); }

                // 5. Compute bbox to scale the CapsuleCollider sensibly
                Bounds bbox = ComputeRendererBounds(instance);
                float height = Mathf.Max(1.0f, bbox.size.y);
                float radius = Mathf.Clamp(Mathf.Max(bbox.size.x, bbox.size.z) * 0.5f, 0.25f, 0.6f);

                // 6. CapsuleCollider
                var capsule = instance.GetComponent<CapsuleCollider>();
                if (capsule == null) capsule = instance.AddComponent<CapsuleCollider>();
                capsule.height = height;
                capsule.radius = radius;
                capsule.center = new Vector3(0f, height * 0.5f, 0f);
                capsule.direction = 1; // Y-axis

                // 7. NavMeshAgent
                var agent = instance.GetComponent<NavMeshAgent>();
                if (agent == null) agent = instance.AddComponent<NavMeshAgent>();
                agent.radius = 0.4f;
                agent.height = 2f;
                agent.speed = 3f;
                agent.angularSpeed = 240f;
                agent.acceleration = 8f;
                agent.stoppingDistance = 1.2f;

                // 8. Controller MonoBehaviour
                if (instance.GetComponent(spec.ControllerType) == null)
                {
                    instance.AddComponent(spec.ControllerType);
                }

                // 9. Save as a Prefab Variant (because instance came from a model prefab,
                //    SaveAsPrefabAssetAndConnect emits a Variant pointing at the FBX root)
                var variantDir = Path.GetDirectoryName(spec.PrefabPath);
                if (!Directory.Exists(variantDir)) Directory.CreateDirectory(variantDir);

                // Delete the existing binary prefab so the new asset is written fresh in
                // whatever serialization mode EditorSettings dictates.
                if (File.Exists(spec.PrefabPath))
                {
                    AssetDatabase.DeleteAsset(spec.PrefabPath);
                }

                var saved = PrefabUtility.SaveAsPrefabAssetAndConnect(
                    instance, spec.PrefabPath, InteractionMode.AutomatedAction,
                    out bool success);

                if (!success || saved == null)
                    throw new InvalidOperationException(
                        $"SaveAsPrefabAssetAndConnect failed for {spec.PrefabPath}");

                report.Add(
                    $"    -> {spec.PrefabPath} (variant of {Path.GetFileName(spec.FbxPath)}, " +
                    $"+{spec.ControllerType.Name}, capsule h={height:F2} r={radius:F2})");
            }
            finally
            {
                if (instance != null) UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        private static Bounds ComputeRendererBounds(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers == null || renderers.Length == 0)
            {
                return new Bounds(Vector3.up, new Vector3(0.8f, 1.8f, 0.8f));
            }
            var b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
            return b;
        }
    }
}