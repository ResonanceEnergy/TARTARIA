using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Tartaria.Editor
{
    /// <summary>
    /// Auto-Setup Tool for Moon1LevelBuilder
    /// Automatically assigns PBR materials and KayKit prefabs from imported assets
    /// Unity Menu: Tartaria → Auto-Setup Moon1 Level Builder
    /// </summary>
    public static class Moon1LevelBuilderAutoSetup
    {
        [MenuItem("Tartaria/Auto-Setup Moon1 Level Builder", false, 102)]
        public static void AutoSetupLevelBuilder()
        {
            // Find Moon1LevelBuilder component in scene
            var levelBuilder = Object.FindObjectOfType<Tartaria.Integration.Moon1LevelBuilder>();
            if (levelBuilder == null)
            {
                Debug.LogError("[Moon1LevelBuilderAutoSetup] No Moon1LevelBuilder found in scene! Make sure the component exists first.");
                return;
            }

            Debug.Log("[Moon1LevelBuilderAutoSetup] Found Moon1LevelBuilder, auto-assigning assets...");

            // Use SerializedObject for proper undo/redo support
            var serializedObject = new SerializedObject(levelBuilder);

            // Assign PBR Materials
            AssignMaterial(serializedObject, "rocksMaterial", "Assets/_Project/Materials/PBR/Rocks023.mat");
            AssignMaterial(serializedObject, "pavingStonesMaterial", "Assets/_Project/Materials/PBR/PavingStones150.mat");
            AssignMaterial(serializedObject, "marbleMaterial", "Assets/_Project/Materials/PBR/Marble006.mat");
            AssignMaterial(serializedObject, "bricksMaterial", "Assets/_Project/Materials/PBR/Bricks075A.mat");
            AssignMaterial(serializedObject, "groundMaterial", "Assets/_Project/Materials/PBR/Ground037.mat");
            AssignMaterial(serializedObject, "plasterMaterial", "Assets/_Project/Materials/PBR/Plaster001.mat");
            AssignMaterial(serializedObject, "woodMaterial", "Assets/_Project/Materials/PBR/Wood063.mat");

            // Assign KayKit Rock Prefabs (all Rock_* variants)
            var rockPrefabs = FindKayKitAssets("Rock_", "fbx");
            AssignPrefabArray(serializedObject, "rockPrefabs", rockPrefabs);

            // Assign KayKit Tree Prefabs (Tree_* variants)
            var treePrefabs = FindKayKitAssets("Tree_", "fbx");
            AssignPrefabArray(serializedObject, "treePrefabs", treePrefabs);

            // Assign KayKit Bush Prefabs (Bush_* variants)
            var bushPrefabs = FindKayKitAssets("Bush_", "fbx");
            AssignPrefabArray(serializedObject, "bushPrefabs", bushPrefabs);

            // Assign KayKit Grass Prefabs (Grass_* variants)
            var grassPrefabs = FindKayKitAssets("Grass_", "fbx");
            AssignPrefabArray(serializedObject, "grassPrefabs", grassPrefabs);

            // Assign Hero Building Prefabs
            AssignPrefab(serializedObject, "starDomePrefab", "Assets/_Project/Prefabs/Buildings/Echohaven_StarDome.prefab");
            AssignPrefab(serializedObject, "fountainPrefab", "Assets/_Project/Prefabs/Buildings/Echohaven_HarmonicFountain.prefab");
            AssignPrefab(serializedObject, "spirePrefab", "Assets/_Project/Prefabs/Buildings/Echohaven_CrystalSpire.prefab");

            // Apply changes
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(levelBuilder);

            Debug.Log("[Moon1LevelBuilderAutoSetup] ✅ Auto-setup complete!");
            Debug.Log($"  • 7 PBR materials assigned");
            Debug.Log($"  • {rockPrefabs.Length} rock prefabs assigned");
            Debug.Log($"  • {treePrefabs.Length} tree prefabs assigned");
            Debug.Log($"  • {bushPrefabs.Length} bush prefabs assigned");
            Debug.Log($"  • {grassPrefabs.Length} grass prefabs assigned");
            Debug.Log($"  • 3 hero building prefabs assigned");
        }

        static void AssignMaterial(SerializedObject obj, string propertyName, string assetPath)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (material != null)
            {
                var prop = obj.FindProperty(propertyName);
                if (prop != null)
                {
                    prop.objectReferenceValue = material;
                    Debug.Log($"  ✓ Assigned {propertyName}: {material.name}");
                }
            }
            else
            {
                Debug.LogWarning($"  ✗ Material not found: {assetPath}");
            }
        }

        static void AssignPrefab(SerializedObject obj, string propertyName, string assetPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab != null)
            {
                var prop = obj.FindProperty(propertyName);
                if (prop != null)
                {
                    prop.objectReferenceValue = prefab;
                    Debug.Log($"  ✓ Assigned {propertyName}: {prefab.name}");
                }
            }
            else
            {
                Debug.LogWarning($"  ✗ Prefab not found: {assetPath}");
            }
        }

        static void AssignPrefabArray(SerializedObject obj, string propertyName, GameObject[] prefabs)
        {
            var prop = obj.FindProperty(propertyName);
            if (prop != null && prop.isArray)
            {
                prop.ClearArray();
                prop.arraySize = prefabs.Length;
                for (int i = 0; i < prefabs.Length; i++)
                {
                    var element = prop.GetArrayElementAtIndex(i);
                    element.objectReferenceValue = prefabs[i];
                }
                Debug.Log($"  ✓ Assigned {propertyName}: {prefabs.Length} prefabs");
            }
        }

        static GameObject[] FindKayKitAssets(string namePrefix, string extension)
        {
            // Search in KayKit Forest Nature Pack
            string searchPath = "Assets/KayKit_Forest_Nature_Pack_1.0_FREE";
            
            var guids = AssetDatabase.FindAssets($"{namePrefix} t:GameObject", new[] { searchPath });
            var prefabs = guids
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Where(path => path.EndsWith($".{extension}"))
                .Select(path => AssetDatabase.LoadAssetAtPath<GameObject>(path))
                .Where(obj => obj != null)
                .OrderBy(obj => obj.name)
                .ToArray();

            if (prefabs.Length == 0)
            {
                Debug.LogWarning($"  ✗ No KayKit assets found matching: {namePrefix}*.{extension}");
            }

            return prefabs;
        }
    }
}
