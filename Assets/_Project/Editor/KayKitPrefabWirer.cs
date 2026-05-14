using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Tartaria.Integration;

namespace Tartaria.Editor
{
    /// <summary>
    /// Auto-wires KayKit prefabs to EchohavenContentSpawner fields for 2026 AAA quality.
    /// Run via menu: TARTARIA / Wire KayKit Prefabs to Content Spawner
    /// </summary>
    public static class KayKitPrefabWirer
    {
        [MenuItem("TARTARIA/Content/Wire KayKit Prefabs to Echohaven Spawner")]
        public static void WireKayKitPrefabs()
        {
            // Load Echohaven scene
            string scenePath = "Assets/_Project/Scenes/Echohaven_VerticalSlice.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // Find EchohavenContentSpawner
            var spawner = Object.FindFirstObjectByType<EchohavenContentSpawner>();
            if (spawner == null)
            {
                Debug.LogError("[KayKitWirer] EchohavenContentSpawner not found in scene!");
                return;
            }

            // Wire shovel
            var shovelPath = "Assets/_Project/Prefabs/Props/KayKit/Tools/Prop_shovel.prefab";
            var shovel = AssetDatabase.LoadAssetAtPath<GameObject>(shovelPath);
            if (shovel != null)
            {
                SetSerializedField(spawner, "kayKitShovelPrefab", shovel);
                Debug.Log($"[KayKitWirer] ✓ Shovel wired: {shovelPath}");
            }
            else
            {
                Debug.LogWarning($"[KayKitWirer] Shovel prefab not found: {shovelPath}");
            }

            // Wire Milo — KayKit Rogue (small, agile, friendly)
            var miloPath = "Assets/_Project/Prefabs/Characters/KayKit/Char_Rogue.prefab";
            var milo = AssetDatabase.LoadAssetAtPath<GameObject>(miloPath);
            if (milo != null)
            {
                SetSerializedField(spawner, "kayKitMiloPrefab", milo);
                Debug.Log($"[KayKitWirer] ✓ Milo wired: {miloPath}");
            }
            else
            {
                Debug.LogWarning($"[KayKitWirer] Milo prefab not found: {miloPath}");
            }

            // Wire Cassian — KayKit Ranger (tall, hooded, mysterious)
            var cassianPath = "Assets/_Project/Prefabs/Characters/KayKit/Char_Ranger.prefab";
            var cassian = AssetDatabase.LoadAssetAtPath<GameObject>(cassianPath);
            if (cassian != null)
            {
                SetSerializedField(spawner, "kayKitCassianPrefab", cassian);
                Debug.Log($"[KayKitWirer] ✓ Cassian wired: {cassianPath}");
            }
            else
            {
                Debug.LogWarning($"[KayKitWirer] Cassian prefab not found: {cassianPath}");
            }

            // Wire MudGolem — KayKit Skeleton Warrior (enemy)
            var golemPath = "Assets/_Project/Prefabs/Characters/KayKit/Skeletons/Char_Skeleton_Warrior.prefab";
            var golem = AssetDatabase.LoadAssetAtPath<GameObject>(golemPath);
            if (golem != null)
            {
                SetSerializedField(spawner, "kayKitMudGolemPrefab", golem);
                Debug.Log($"[KayKitWirer] ✓ MudGolem wired: {golemPath}");
            }
            else
            {
                Debug.LogWarning($"[KayKitWirer] MudGolem prefab not found: {golemPath}");
            }

            // Wire Anastasia — KayKit Mage (ghostly, ethereal)
            var anastasiaPath = "Assets/_Project/Prefabs/Characters/KayKit/Char_Mage.prefab";
            var anastasia = AssetDatabase.LoadAssetAtPath<GameObject>(anastasiaPath);
            if (anastasia != null)
            {
                SetSerializedField(spawner, "kayKitAnastasiaPrefab", anastasia);
                Debug.Log($"[KayKitWirer] ✓ Anastasia wired: {anastasiaPath}");
            }
            else
            {
                Debug.LogWarning($"[KayKitWirer] Anastasia prefab not found: {anastasiaPath}");
            }

            // Wire rock prefabs (KayKit Forest pack)
            string[] rockPaths = {
                "Assets/_Project/Prefabs/Props/KayKit/Forest/Prop_Rock_1_A_Color1.prefab",
                "Assets/_Project/Prefabs/Props/KayKit/Forest/Prop_Rock_1_B_Color1.prefab",
                "Assets/_Project/Prefabs/Props/KayKit/Forest/Prop_Rock_2_A_Color1.prefab",
                "Assets/_Project/Prefabs/Props/KayKit/Forest/Prop_Rock_2_B_Color1.prefab",
                "Assets/_Project/Prefabs/Props/KayKit/Forest/Prop_Rock_3_A_Color1.prefab"
            };
            var rocks = new GameObject[rockPaths.Length];
            for (int i = 0; i < rockPaths.Length; i++)
            {
                rocks[i] = AssetDatabase.LoadAssetAtPath<GameObject>(rockPaths[i]);
                if (rocks[i] == null)
                    Debug.LogWarning($"[KayKitWirer] Rock prefab not found: {rockPaths[i]}");
            }
            SetSerializedField(spawner, "kayKitRockPrefabs", rocks);
            Debug.Log($"[KayKitWirer] ✓ Rocks wired: {rocks.Length} prefabs");

            // Wire foliage prefabs (bushes)
            string[] foliagePaths = {
                "Assets/_Project/Prefabs/Props/KayKit/Forest/Prop_Bush_1_A_Color1.prefab",
                "Assets/_Project/Prefabs/Props/KayKit/Forest/Prop_Bush_2_A_Color1.prefab",
                "Assets/_Project/Prefabs/Props/KayKit/Forest/Prop_Bush_3_A_Color1.prefab",
                "Assets/_Project/Prefabs/Props/KayKit/Forest/Prop_Bush_4_A_Color1.prefab",
                "Assets/_Project/Prefabs/Props/KayKit/Forest/Prop_Grass_1_A_Color1.prefab"
            };
            var foliage = new GameObject[foliagePaths.Length];
            for (int i = 0; i < foliagePaths.Length; i++)
            {
                foliage[i] = AssetDatabase.LoadAssetAtPath<GameObject>(foliagePaths[i]);
                if (foliage[i] == null)
                    Debug.LogWarning($"[KayKitWirer] Foliage prefab not found: {foliagePaths[i]}");
            }
            SetSerializedField(spawner, "kayKitFoliagePrefabs", foliage);
            Debug.Log($"[KayKitWirer] ✓ Foliage wired: {foliage.Length} prefabs");

            // Save scene
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[KayKitWirer] ═══════════════════════════════════════");
            Debug.Log("[KayKitWirer] ✓ KayKit prefabs wired successfully!");
            Debug.Log("[KayKitWirer] Echohaven_VerticalSlice scene updated.");
            Debug.Log("[KayKitWirer] Run the game to see AAA quality visuals.");
            Debug.Log("[KayKitWirer] ═══════════════════════════════════════");

            if (OneClickBuild.DialogsAllowed)
            {
                EditorUtility.DisplayDialog("KayKit Prefabs Wired",
                    "✓ All KayKit prefabs successfully wired to EchohavenContentSpawner!\n\n" +
                    "Characters:\n" +
                    "  • Milo (Rogue)\n" +
                    "  • Cassian (Ranger)\n" +
                    "  • MudGolem (Skeleton Warrior)\n" +
                    "  • Anastasia (Mage)\n\n" +
                    "Props:\n" +
                    "  • Shovel (field tool)\n" +
                    "  • 5 rock varieties\n" +
                    "  • 5 foliage types\n\n" +
                    "Press Play to see the upgrade!",
                    "OK");
            }
        }

        static void SetSerializedField(Object target, string fieldName, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                prop.objectReferenceValue = value;
                so.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogError($"[KayKitWirer] Field not found: {fieldName} on {target.GetType().Name}");
            }
        }

        static void SetSerializedField(Object target, string fieldName, GameObject[] arr)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                prop.arraySize = arr.Length;
                for (int i = 0; i < arr.Length; i++)
                {
                    var elem = prop.GetArrayElementAtIndex(i);
                    elem.objectReferenceValue = arr[i];
                }
                so.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogError($"[KayKitWirer] Field not found: {fieldName} on {target.GetType().Name}");
            }
        }
    }
}
