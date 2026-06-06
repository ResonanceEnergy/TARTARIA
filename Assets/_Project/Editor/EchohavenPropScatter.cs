using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tartaria.Editor
{
    /// <summary>
    /// Edit-time prop scatterer for Echohaven (Moon 1) village.
    /// Places curated, building-appropriate props as real scene PrefabInstances under a
    /// "Village_Props" root, marked Static. Idempotent: clears the root and rebuilds.
    ///
    /// This is NOT a runtime spawner (banned). It composes static content into the scene
    /// at edit time, per CLAUDE.md §4 ("Compose in scene YAML, not runtime new GameObject" /
    /// "Mark immovable env Static — Batching + GI + Occluder").
    ///
    /// Menu: Tartaria/1 Build/Scatter Village Props
    /// Batch: -executeMethod Tartaria.Editor.EchohavenPropScatter.RunBatch
    /// </summary>
    public static class EchohavenPropScatter
    {
        const string ScenePath = "Assets/_Project/Scenes/Echohaven_VerticalSlice.unity";
        const string PropsDir  = "Assets/_Project/Prefabs/Moon1/Blender/Props/";
        const string ArchDir   = "Assets/_Project/Prefabs/Moon1/Blender/Architecture/";
        const string RootName  = "Village_Props";
        const int    Seed       = 13371;

        // building name (Contains-match, case-insensitive) -> curated prop set.
        // All names verified to exist on disk (Props/ first, Architecture/ fallback).
        static readonly (string key, string[] props)[] Sets =
        {
            ("Smithy",     new[]{ "Anvil", "AnvilHorn", "Bellows", "Cauldron", "BarrelLarge", "BrickPile", "CartWheel" }),
            ("Bakery",     new[]{ "BarrelSmall", "BasketWoven", "CartFull", "Cauldron", "BrickPile" }),
            ("Apothecary", new[]{ "Alembic", "BeakerLarge", "BeakerMed", "BeakerSmall", "BrewingRack", "BigMortar", "AetherVial" }),
            ("Inn",        new[]{ "BarrelLarge", "BarrelSmall", "CandelabraTriple", "BasketWoven", "CartWagon" }),
            ("Mill",       new[]{ "CartFull", "BarrelLarge", "CartWheel", "BrickPile", "BasketWoven" }),
            ("Watchtower", new[]{ "ArrowBundle", "BannerPole", "BrickPile", "BarrelSmall" }),
            ("TownHall",   new[]{ "BannerPole", "CandelabraTriple", "BasketWoven", "FencePanel" }),
            ("Cottage",    new[]{ "BarrelSmall", "BasketWoven", "FencePanel", "CartWheel", "ChestStudded" }),
        };

        [MenuItem("Tartaria/1 Build/Scatter Village Props")]
        public static void ScatterMenu()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.path.EndsWith("Echohaven_VerticalSlice.unity"))
            {
                if (!EditorUtility.DisplayDialog("Scatter Village Props",
                        "Active scene is not Echohaven_VerticalSlice. Open it and scatter?", "Open + Scatter", "Cancel"))
                    return;
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
            int placed = Scatter(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[PropScatter] DONE — placed {placed} props. Scene saved.");
        }

        /// <summary>Headless entry: open scene, scatter, save, exit. For -executeMethod / CI.</summary>
        public static void RunBatch()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            int placed = Scatter(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            bool ok = EditorSceneManager.SaveScene(scene);
            Debug.Log($"[PropScatter] DONE — placed {placed} props. saved={ok}");
        }

        static int Scatter(Scene scene)
        {
            // Idempotent: drop any prior Village_Props root and rebuild from scratch.
            foreach (var go in scene.GetRootGameObjects())
            {
                if (go.name == RootName) { Object.DestroyImmediate(go); break; }
            }

            // Complete the village: place CottageB/C if the scene only has CottageA.
            EnsureCottages(scene);

            var root = new GameObject(RootName);
            SceneManager.MoveGameObjectToScene(root, scene);

            // Index every building transform in the scene by its set keyword.
            var buildings = new List<(Transform t, string[] props)>();
            foreach (var rootGo in scene.GetRootGameObjects())
            {
                foreach (var t in rootGo.GetComponentsInChildren<Transform>(true))
                {
                    string n = t.name;
                    foreach (var (key, props) in Sets)
                    {
                        if (n.IndexOf(key, System.StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            buildings.Add((t, props));
                            break; // first keyword match wins per transform
                        }
                    }
                }
            }

            var rng = new System.Random(Seed);
            int placed = 0;
            int skipped = 0;

            foreach (var (bt, props) in buildings)
            {
                Vector3 c = bt.position;
                int n = props.Length;
                for (int i = 0; i < n; i++)
                {
                    var prefab = LoadProp(props[i]);
                    if (prefab == null) { skipped++; continue; }

                    float ang = (i * (360f / Mathf.Max(1, n))) + (float)(rng.NextDouble() * 40.0 - 20.0);
                    float rad = 3.0f + (i % 2) * 1.4f + (float)(rng.NextDouble() * 0.8);
                    float theta = ang * Mathf.Deg2Rad;
                    Vector3 pos = c + new Vector3(Mathf.Cos(theta) * rad, 0f, Mathf.Sin(theta) * rad);
                    pos.y = c.y;

                    var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                    inst.transform.SetParent(root.transform, true);
                    inst.transform.position = pos;
                    inst.transform.rotation = Quaternion.Euler(0f, (float)(rng.NextDouble() * 360.0), 0f);
                    inst.name = $"{props[i]}_{bt.name}_{i}";
                    SetStatic(inst);
                    placed++;
                }
            }

            Debug.Log($"[PropScatter] buildings matched={buildings.Count} placed={placed} skipped(missing prefab)={skipped}");
            return placed;
        }

        // Place VillageCottageB / VillageCottageC near CottageA if they aren't in the scene yet.
        static void EnsureCottages(Scene scene)
        {
            PlaceBuildingIfMissing(scene, "VillageCottageB", new Vector3(-12f, 0f, 84f), 35f);
            PlaceBuildingIfMissing(scene, "VillageCottageC", new Vector3(-28f, 0f, 76f), -25f);
        }

        static void PlaceBuildingIfMissing(Scene scene, string prefabName, Vector3 pos, float yaw)
        {
            foreach (var rootGo in scene.GetRootGameObjects())
                foreach (var t in rootGo.GetComponentsInChildren<Transform>(true))
                    if (t.name.IndexOf(prefabName, System.StringComparison.OrdinalIgnoreCase) >= 0)
                        return; // already present

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ArchDir + prefabName + ".prefab");
            if (prefab == null) { Debug.LogWarning($"[PropScatter] building prefab missing: {prefabName}"); return; }

            var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            inst.transform.position = pos;
            inst.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            inst.name = prefabName;
            SetStatic(inst);
            Debug.Log($"[PropScatter] placed missing building {prefabName} at {pos}");
        }

        static GameObject LoadProp(string name)
        {
            var p = AssetDatabase.LoadAssetAtPath<GameObject>(PropsDir + name + ".prefab");
            if (p == null) p = AssetDatabase.LoadAssetAtPath<GameObject>(ArchDir + name + ".prefab");
            if (p == null) Debug.LogWarning($"[PropScatter] prop prefab not found: {name}");
            return p;
        }

        static void SetStatic(GameObject go)
        {
            var flags = StaticEditorFlags.BatchingStatic | StaticEditorFlags.ContributeGI |
                        StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic;
            foreach (var t in go.GetComponentsInChildren<Transform>(true))
                GameObjectUtility.SetStaticEditorFlags(t.gameObject, flags);
        }
    }
}
