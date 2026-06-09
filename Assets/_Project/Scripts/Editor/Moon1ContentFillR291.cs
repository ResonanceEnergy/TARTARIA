using UnityEngine;
using UnityEditor;

namespace Tartaria.Editor
{
    /// <summary>
    /// R291 — Moon 1 content fill replay menu.
    /// Re-applies the 5 GameObjects from R288-R289 that were lost when Unity MCP bridge disconnected:
    /// 1. Overlook_South POI @ (0, 3, -55) — southern ridge vista per spec §7
    /// 2. RootChamber POI @ (-25, -2, 25) — underground cavity per spec §7
    /// 3. Prop_GiantSkeleton @ (0, 0, 17) — 17th-hour beat per docs/03 §1
    /// 4. SkeletonRemains_BuriedHand @ (45, 0, 25) — Buried Beacon per docs/03 §1
    /// 5. AnastasiaRocker @ (18, 0, 5) — beside Anastasia NPC
    ///
    /// Menu: Tartaria → 1 Build → Moon 1 Content Fill R291
    /// </summary>
    public static class Moon1ContentFillR291
    {
        const string CanonDir = "Assets/_Project/Models/Buildings/Blender_canon/";

        [MenuItem("Tartaria/1 Build/Moon 1 Content Fill R291 (POIs + 17th-hour + Rocker)")]
        public static void Run()
        {
            int placed = 0;

            placed += PlaceOverlookSouth();
            placed += PlaceRootChamber();
            placed += PlaceGiantSkeleton();
            placed += PlaceSkeletonRemains();
            placed += PlaceAnastasiaRocker();

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            EditorUtility.DisplayDialog(
                "Moon 1 Content Fill R291",
                $"Placed {placed} of 5 GameObjects. Save the scene (Ctrl+S) to persist.",
                "OK");
        }

        static int PlaceOverlookSouth()
        {
            var existing = GameObject.Find("Overlook_South");
            if (existing != null) Object.DestroyImmediate(existing);

            var overlook = new GameObject("Overlook_South");
            overlook.transform.position = new Vector3(0, 3, -55);
            overlook.transform.rotation = Quaternion.Euler(0, 180, 0);

            var platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.name = "Overlook_StonePlatform";
            platform.transform.SetParent(overlook.transform, false);
            platform.transform.localPosition = new Vector3(0, -0.5f, 0);
            platform.transform.localScale = new Vector3(8, 0.4f, 4);

            for (int i = 0; i < 4; i++)
            {
                var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pillar.name = "Overlook_Pillar_" + i;
                pillar.transform.SetParent(overlook.transform, false);
                float x = (i % 2 == 0 ? -3 : 3);
                float z = (i < 2 ? -1.5f : 1.5f);
                pillar.transform.localPosition = new Vector3(x, 0.8f, z);
                pillar.transform.localScale = new Vector3(0.3f, 1.2f, 0.3f);
            }

            var lightGO = new GameObject("Overlook_VistaLight");
            lightGO.transform.SetParent(overlook.transform, false);
            lightGO.transform.localPosition = new Vector3(0, 2.5f, 0);
            var l = lightGO.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1.0f, 0.85f, 0.45f);
            l.intensity = 2.5f;
            l.range = 8;

            GameObjectUtility.SetStaticEditorFlags(overlook,
                StaticEditorFlags.BatchingStatic | StaticEditorFlags.NavigationStatic |
                StaticEditorFlags.ContributeGI);
            Debug.Log("[Moon1ContentFillR291] Overlook_South placed @ (0, 3, -55)");
            return 1;
        }

        static int PlaceRootChamber()
        {
            var existing = GameObject.Find("RootChamber");
            if (existing != null) Object.DestroyImmediate(existing);

            var root = new GameObject("RootChamber");
            root.transform.position = new Vector3(-25, -2, 25);

            var floor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            floor.name = "RootChamber_Floor";
            floor.transform.SetParent(root.transform, false);
            floor.transform.localPosition = new Vector3(0, 0, 0);
            floor.transform.localScale = new Vector3(4, 0.2f, 4);

            for (int i = 0; i < 3; i++)
            {
                float ang = i * 120f * Mathf.Deg2Rad;
                var rootCol = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                rootCol.name = "RootChamber_Pillar_" + i;
                rootCol.transform.SetParent(root.transform, false);
                rootCol.transform.localPosition = new Vector3(Mathf.Cos(ang) * 2.5f, 1.5f, Mathf.Sin(ang) * 2.5f);
                rootCol.transform.localScale = new Vector3(0.4f, 2.5f, 0.4f);
            }

            var glowGO = new GameObject("RootChamber_AetherGlow");
            glowGO.transform.SetParent(root.transform, false);
            glowGO.transform.localPosition = new Vector3(0, 1.5f, 0);
            var glow = glowGO.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.color = new Color(0.55f, 0.85f, 1.0f);  // Aether Cyan
            glow.intensity = 3.0f;
            glow.range = 6;

            GameObjectUtility.SetStaticEditorFlags(root,
                StaticEditorFlags.BatchingStatic | StaticEditorFlags.NavigationStatic |
                StaticEditorFlags.ContributeGI);
            Debug.Log("[Moon1ContentFillR291] RootChamber placed @ (-25, -2, 25)");
            return 1;
        }

        static int PlaceGiantSkeleton()
        {
            var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(CanonDir + "Prop_GiantSkeleton.fbx");
            if (fbx == null) { Debug.LogWarning("Prop_GiantSkeleton.fbx not found"); return 0; }

            var existing = GameObject.Find("GiantSkeleton");
            if (existing != null) Object.DestroyImmediate(existing);

            var sk = (GameObject)PrefabUtility.InstantiatePrefab(fbx);
            sk.name = "GiantSkeleton";
            sk.transform.position = new Vector3(0, 0, 17);
            sk.transform.localScale = Vector3.one * 1.5f;

            GameObjectUtility.SetStaticEditorFlags(sk,
                StaticEditorFlags.BatchingStatic | StaticEditorFlags.NavigationStatic |
                StaticEditorFlags.ContributeGI);
            Debug.Log("[Moon1ContentFillR291] GiantSkeleton placed @ (0, 0, 17) — 17th-hour beat ready");
            return 1;
        }

        static int PlaceSkeletonRemains()
        {
            var path = "Assets/_Project/Models/Blender/Moon1/SkeletonRemains.fbx";
            var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (fbx == null) { Debug.LogWarning("SkeletonRemains.fbx not found"); return 0; }

            var existing = GameObject.Find("SkeletonRemains_BuriedHand");
            if (existing != null) Object.DestroyImmediate(existing);

            var rem = (GameObject)PrefabUtility.InstantiatePrefab(fbx);
            rem.name = "SkeletonRemains_BuriedHand";
            rem.transform.position = new Vector3(45, 0, 25);

            GameObjectUtility.SetStaticEditorFlags(rem,
                StaticEditorFlags.BatchingStatic | StaticEditorFlags.ContributeGI);
            Debug.Log("[Moon1ContentFillR291] SkeletonRemains_BuriedHand placed @ (45, 0, 25)");
            return 1;
        }

        static int PlaceAnastasiaRocker()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Moon1/AnastasiaRocker.prefab");
            if (prefab == null) { Debug.LogWarning("AnastasiaRocker.prefab not found"); return 0; }

            var existing = GameObject.Find("AnastasiaRocker");
            if (existing != null) Object.DestroyImmediate(existing);

            var rocker = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            rocker.name = "AnastasiaRocker";
            rocker.transform.position = new Vector3(18, 0, 5);
            rocker.transform.rotation = Quaternion.Euler(0, -90, 0);

            GameObjectUtility.SetStaticEditorFlags(rocker,
                StaticEditorFlags.BatchingStatic | StaticEditorFlags.NavigationStatic |
                StaticEditorFlags.ContributeGI);
            Debug.Log("[Moon1ContentFillR291] AnastasiaRocker placed @ (18, 0, 5)");
            return 1;
        }
    }
}
