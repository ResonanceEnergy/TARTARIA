using UnityEngine;
using UnityEditor;
using System.IO;

namespace Tartaria.Editor
{
    /// <summary>
    /// Factory for creating fallback building placeholder prefabs.
    /// Menu: Tartaria → Build → Create Fallback Building Prefabs
    /// CLI: Unity -executeMethod Tartaria.Editor.FallbackBuildingPrefabFactory.CreateAll
    /// 
    /// Note: Placeholders are name-based fallbacks used by BuildingSpawner/EchohavenContentSpawner.
    /// No runtime tag component needed (removed to avoid editor-only script references in prefabs).
    /// </summary>
    public static class FallbackBuildingPrefabFactory
    {
        const string PrefabDir = "Assets/_Project/Prefabs/Props";

        [MenuItem("Tartaria/Build/Create Fallback Building Prefabs")]
        public static void CreateAll()
        {
            if (!Directory.Exists(PrefabDir))
            {
                Directory.CreateDirectory(PrefabDir);
                AssetDatabase.Refresh();
            }

            CreateStarDomePlaceholder();
            CreateHarmonicFountainPlaceholder();
            CreateCrystalSpirePlaceholder();

            AssetDatabase.SaveAssets();
            Debug.Log("[FallbackBuildingPrefabFactory] Created 3 fallback building prefabs.");
        }

        static void CreateStarDomePlaceholder()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "StarDome_Placeholder";
            go.transform.localScale = new Vector3(8f, 6f, 8f);

            // Emissive cyan material
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.name = "M_StarDome_Placeholder";
            mat.SetColor("_BaseColor", new Color(0.3f, 0.8f, 1f, 1f));
            mat.SetColor("_EmissionColor", new Color(0.3f, 0.8f, 1f, 1f) * 2f);
            mat.EnableKeyword("_EMISSION");

            var renderer = go.GetComponent<Renderer>();
            renderer.sharedMaterial = mat;

            SavePrefab(go, "StarDome_Placeholder.prefab");
        }

        static void CreateHarmonicFountainPlaceholder()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "HarmonicFountain_Placeholder";
            go.transform.localScale = new Vector3(4f, 3f, 4f);

            // Emissive magenta material
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.name = "M_HarmonicFountain_Placeholder";
            mat.SetColor("_BaseColor", new Color(1f, 0.3f, 0.8f, 1f));
            mat.SetColor("_EmissionColor", new Color(1f, 0.3f, 0.8f, 1f) * 2f);
            mat.EnableKeyword("_EMISSION");

            var renderer = go.GetComponent<Renderer>();
            renderer.sharedMaterial = mat;

            SavePrefab(go, "HarmonicFountain_Placeholder.prefab");
        }

        static void CreateCrystalSpirePlaceholder()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "CrystalSpire_Placeholder";
            go.transform.localScale = new Vector3(3f, 12f, 3f);

            // Emissive yellow material
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.name = "M_CrystalSpire_Placeholder";
            mat.SetColor("_BaseColor", new Color(1f, 0.9f, 0.3f, 1f));
            mat.SetColor("_EmissionColor", new Color(1f, 0.9f, 0.3f, 1f) * 2f);
            mat.EnableKeyword("_EMISSION");

            var renderer = go.GetComponent<Renderer>();
            renderer.sharedMaterial = mat;

            SavePrefab(go, "CrystalSpire_Placeholder.prefab");
        }

        static void SavePrefab(GameObject go, string filename)
        {
            string path = $"{PrefabDir}/{filename}";
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            Debug.Log($"[FallbackBuildingPrefabFactory] Saved {path}");
        }
    }
}
