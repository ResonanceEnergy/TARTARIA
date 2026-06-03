#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tartaria.Editor
{
    public static class EchohavenContentBaker
    {
        const string ECHOHAVEN_SCENE_PATH = "Assets/_Project/Scenes/Echohaven_VerticalSlice.unity";
        const string BAKED_ROOT_NAME = "BakedEchohavenContent";

        struct BakeEntry
        {
            public string objectName;
            public string prefabPath;
            public Vector3 position;
            public Vector3 eulerRotation;
            public float uniformScale;
            public string label;
        }

        [MenuItem("Tartaria/6 Bake/Bake Echohaven Content Into Scene", priority = 600)]
        public static void Bake()
        {
            if (!System.IO.File.Exists(ECHOHAVEN_SCENE_PATH))
            {
                EditorUtility.DisplayDialog("Echohaven Baker",
                    "Scene not found: " + ECHOHAVEN_SCENE_PATH, "OK");
                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Echohaven Baker",
                    "Cannot bake while Play mode is active.", "OK");
                return;
            }

            var scene = EditorSceneManager.OpenScene(ECHOHAVEN_SCENE_PATH, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError("[EchohavenContentBaker] Failed to open Echohaven scene.");
                return;
            }

            var existingRoot = GameObject.Find(BAKED_ROOT_NAME);
            if (existingRoot != null)
            {
                Object.DestroyImmediate(existingRoot);
                Debug.Log("[EchohavenContentBaker] Cleared prior bake root.");
            }

            var bakedRoot = new GameObject(BAKED_ROOT_NAME);
            SceneManager.MoveGameObjectToScene(bakedRoot, scene);

            var entries = BuildEntries();

            int placed = 0;
            int skipped = 0;
            var missing = new List<string>();
            foreach (var e in entries)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(e.prefabPath);
                if (prefab == null)
                {
                    Debug.LogWarning("[EchohavenContentBaker] MISSING prefab: " + e.prefabPath + " (" + e.label + ") skipping");
                    missing.Add(e.prefabPath);
                    skipped++;
                    continue;
                }

                var existing = GameObject.Find(e.objectName);
                if (existing != null && existing != bakedRoot)
                {
                    Debug.Log("[EchohavenContentBaker] " + e.objectName + " already in scene skipping.");
                    skipped++;
                    continue;
                }

                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                instance.name = e.objectName;
                instance.transform.SetParent(bakedRoot.transform, false);
                instance.transform.localPosition = e.position;
                instance.transform.localRotation = Quaternion.Euler(e.eulerRotation);
                if (e.uniformScale > 0f)
                    instance.transform.localScale = Vector3.one * e.uniformScale;
                placed++;
                Debug.Log("[EchohavenContentBaker] BAKED " + e.label + " at " + e.position + " (" + e.prefabPath + ")");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[EchohavenContentBaker] DONE baked " + placed + ", skipped " + skipped + ". Scene saved.");
            if (missing.Count > 0)
            {
                Debug.LogWarning("[EchohavenContentBaker] " + missing.Count + " prefab(s) missing see above.");
            }
            EditorUtility.DisplayDialog("Echohaven Content Baker",
                "Baked " + placed + " prefab(s). Skipped " + skipped + ". Scene saved.", "OK");
        }

        static List<BakeEntry> BuildEntries()
        {
            return new List<BakeEntry>
            {
                NewEntry("Milo", "Assets/_Project/Prefabs/Characters/Milo.prefab",
                    new Vector3(14f, 1f, 3f), Vector3.zero, 0.85f, "Milo (companion)"),
                NewEntry("Cassian", "Assets/_Project/Prefabs/Characters/Cassian.prefab",
                    new Vector3(-10f, 0f, 15f), new Vector3(0f, 180f, 0f), 1.1f, "Cassian (NPC)"),
                NewEntry("Anastasia", "Assets/_Project/Prefabs/Characters/Anastasia.prefab",
                    new Vector3(-4f, 0f, 8f), Vector3.zero, 0.9f, "Anastasia (ghost)"),
                NewEntry("Lirael", "Assets/_Project/Prefabs/Characters/Lirael.prefab",
                    new Vector3(0f, 0f, 6f), Vector3.zero, 0.95f, "Lirael (echo)"),
                NewEntry("MudGolem_Baked", "Assets/_Project/Prefabs/Characters/MudGolem.prefab",
                    new Vector3(25f, 0f, 25f), new Vector3(0f, -45f, 0f), 1f, "MudGolem (anchor enemy)"),
                NewEntry("ShovelPickup", "Assets/_Project/Prefabs/Moon1/Blender/Shovel.prefab",
                    new Vector3(12f, 1f, 7f), new Vector3(45f, 0f, 0f), 1.2f, "ShovelPickup"),
                NewEntry("RuinedColumn_Baked_A", "Assets/_Project/Prefabs/Moon1/Blender/RuinedColumn.prefab",
                    new Vector3(18f, 0f, 12f), new Vector3(0f, 15f, 0f), 1f, "RuinedColumn A"),
                NewEntry("RuinedColumn_Baked_B", "Assets/_Project/Prefabs/Moon1/Blender/RuinedColumn.prefab",
                    new Vector3(-12f, 0f, 22f), new Vector3(0f, -22f, 0f), 1f, "RuinedColumn B"),
                NewEntry("CrystalCluster_Baked", "Assets/_Project/Prefabs/Moon1/Blender/CrystalCluster.prefab",
                    new Vector3(8f, 0.6f, -6f), Vector3.zero, 1f, "CrystalCluster"),
                NewEntry("Villager_Baked_A", "Assets/_Project/Prefabs/Moon1/Blender/Villager_GenericA.prefab",
                    new Vector3(-2f, 0f, 12f), new Vector3(0f, 90f, 0f), 1f, "Villager A"),
            };
        }

        static BakeEntry NewEntry(string name, string path, Vector3 pos, Vector3 rot, float scale, string label)
        {
            return new BakeEntry { objectName = name, prefabPath = path, position = pos, eulerRotation = rot, uniformScale = scale, label = label };
        }
    }
}
#endif
