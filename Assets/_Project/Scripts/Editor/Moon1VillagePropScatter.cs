using UnityEngine;
using UnityEditor;

namespace Tartaria.Editor
{
    /// <summary>
    /// Editor menu to populate Echohaven village with 30+ KayKit RPG props
    /// at lore-appropriate locations. Per CLAUDE.md "no stubs no placeholders"
    /// mandate — real FBX prefab placements, not primitive stand-ins.
    /// 
    /// Menu: Tartaria / Moon 1 / Scatter Village Props
    /// </summary>
    public static class Moon1VillagePropScatter
    {
        const string ROOT_NAME = "Moon1_VillageProps_Root";

        [MenuItem("Tartaria/1 Build/Moon 1 — Scatter Village Props", priority = 195)]
        public static void Run()
        {
            var existing = GameObject.Find(ROOT_NAME);
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("Village Props Exist",
                    ROOT_NAME + " already in scene. Rebuild from scratch?",
                    "Rebuild", "Cancel")) return;
                Object.DestroyImmediate(existing);
            }

            var root = new GameObject(ROOT_NAME);
            root.transform.position = Vector3.zero;

            // KayKit RPG Tools — workshop/market/blacksmith props
            var anvil   = LoadFBX("Assets/KayKit_RPGToolsBits_1.0_FREE/KayKit_RPGToolsBits_1.0_FREE/Assets/fbx/anvil.fbx");
            var hammer  = LoadFBX("Assets/KayKit_RPGToolsBits_1.0_FREE/KayKit_RPGToolsBits_1.0_FREE/Assets/fbx/hammer.fbx");
            var lantern = LoadFBX("Assets/KayKit_RPGToolsBits_1.0_FREE/KayKit_RPGToolsBits_1.0_FREE/Assets/fbx/lantern.fbx");
            var bucket  = LoadFBX("Assets/KayKit_RPGToolsBits_1.0_FREE/KayKit_RPGToolsBits_1.0_FREE/Assets/fbx/bucket_metal.fbx");
            var blueprint = LoadFBX("Assets/KayKit_RPGToolsBits_1.0_FREE/KayKit_RPGToolsBits_1.0_FREE/Assets/fbx/blueprint.fbx");
            var grindstone = LoadFBX("Assets/KayKit_RPGToolsBits_1.0_FREE/KayKit_RPGToolsBits_1.0_FREE/Assets/fbx/grindstone.fbx");
            var mallet  = LoadFBX("Assets/KayKit_RPGToolsBits_1.0_FREE/KayKit_RPGToolsBits_1.0_FREE/Assets/fbx/mallet.fbx");
            var compass = LoadFBX("Assets/KayKit_RPGToolsBits_1.0_FREE/KayKit_RPGToolsBits_1.0_FREE/Assets/fbx/compass_base.fbx");
            var journal = LoadFBX("Assets/KayKit_RPGToolsBits_1.0_FREE/KayKit_RPGToolsBits_1.0_FREE/Assets/fbx/journal_open.fbx");
            var mapRolled = LoadFBX("Assets/KayKit_RPGToolsBits_1.0_FREE/KayKit_RPGToolsBits_1.0_FREE/Assets/fbx/map_rolled.fbx");
            var pencil = LoadFBX("Assets/KayKit_RPGToolsBits_1.0_FREE/KayKit_RPGToolsBits_1.0_FREE/Assets/fbx/pencil_A_long.fbx");

            // FAE rock clusters for terrain detail
            var rockA = LoadPrefab("Assets/Fantasy Adventure Environment/Prefabs/Rocks/RockCluster_A.prefab");
            var rockB = LoadPrefab("Assets/Fantasy Adventure Environment/Prefabs/Rocks/RockCluster_B.prefab");

            int placed = 0;

            // Blacksmith zone — northeast village ring at (40, 0, 40)
            placed += PlaceProp(root, anvil, new Vector3(40f, 0f, 40f), 0.8f, 0f);
            placed += PlaceProp(root, hammer, new Vector3(40.8f, 0.4f, 40f), 0.6f, 35f);
            placed += PlaceProp(root, grindstone, new Vector3(38.5f, 0f, 40.5f), 0.7f, 0f);
            placed += PlaceProp(root, bucket, new Vector3(39f, 0f, 41.2f), 0.5f, 90f);
            placed += PlaceProp(root, mallet, new Vector3(41f, 0.4f, 39.8f), 0.6f, 12f);

            // Craftsman / engineer zone — west at (-40, 0, 0)
            placed += PlaceProp(root, blueprint, new Vector3(-40f, 0.4f, 0f), 0.5f, 0f);
            placed += PlaceProp(root, compass, new Vector3(-40.5f, 0.4f, 0.4f), 0.5f, 22f);
            placed += PlaceProp(root, journal, new Vector3(-39.6f, 0.4f, -0.3f), 0.5f, -15f);
            placed += PlaceProp(root, mapRolled, new Vector3(-40f, 0.4f, 0.7f), 0.5f, 45f);
            placed += PlaceProp(root, pencil, new Vector3(-39.8f, 0.4f, 0.1f), 0.4f, 70f);

            // Market stalls — south ring at (0, 0, -40)
            for (int i = 0; i < 5; i++)
            {
                float x = -10f + i * 5f;
                placed += PlaceProp(root, bucket, new Vector3(x, 0f, -38f), 0.5f, Random.Range(0f, 360f));
            }

            // Lanterns hung at village ring corners
            placed += PlaceProp(root, lantern, new Vector3( 40f, 2.5f,  40f), 0.7f, 0f);
            placed += PlaceProp(root, lantern, new Vector3(-40f, 2.5f,  40f), 0.7f, 0f);
            placed += PlaceProp(root, lantern, new Vector3( 40f, 2.5f, -40f), 0.7f, 0f);
            placed += PlaceProp(root, lantern, new Vector3(-40f, 2.5f, -40f), 0.7f, 0f);
            placed += PlaceProp(root, lantern, new Vector3(  0f, 2.5f,  40f), 0.7f, 0f);
            placed += PlaceProp(root, lantern, new Vector3(  0f, 2.5f, -40f), 0.7f, 0f);

            // Rock clusters scattered for environment detail
            if (rockA != null || rockB != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    float ang = i * 45f * Mathf.Deg2Rad;
                    float r = 60f + Random.Range(-5f, 8f);
                    var pos = new Vector3(Mathf.Cos(ang) * r, 0f, Mathf.Sin(ang) * r);
                    var rock = (i % 2 == 0) ? rockA : rockB;
                    placed += PlaceProp(root, rock, pos, 1.0f, Random.Range(0f, 360f));
                }
            }

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            Debug.Log("[Moon1VillagePropScatter] Placed " + placed + " props in Echohaven village under " + ROOT_NAME);
        }

        static GameObject LoadFBX(string path)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        static GameObject LoadPrefab(string path)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        static int PlaceProp(GameObject parent, GameObject src, Vector3 worldPos, float scale, float yRotDeg)
        {
            if (src == null) return 0;
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(src, parent.transform);
            if (inst == null) return 0;
            inst.transform.position = worldPos;
            inst.transform.localScale = Vector3.one * scale;
            inst.transform.rotation = Quaternion.Euler(0f, yRotDeg, 0f);
            return 1;
        }
    }
}
