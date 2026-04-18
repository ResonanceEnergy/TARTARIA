using UnityEditor;
using UnityEngine;
using Tartaria.Gameplay;

namespace Tartaria.Editor
{
    /// <summary>
    /// Generates the Golden Mote collectible prefab.
    /// Menu: Tartaria > Build Assets > Golden Mote Prefab
    /// </summary>
    public static class GoldenMotePrefabFactory
    {
        const string PrefabPath = "Assets/_Project/Prefabs/Collectibles";
        const string MaterialPath = "Assets/_Project/Materials";

        [MenuItem("Tartaria/Build Assets/Golden Mote Prefab", false, 21)]
        public static void BuildMotePrefab()
        {
            EnsureDirectories();

            string prefabFile = $"{PrefabPath}/GoldenMote.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabFile) != null)
            {
                Debug.Log("[GoldenMote] Prefab already exists, skipping.");
                return;
            }

            // Create golden material
            var goldMat = CreateGoldMaterial();

            // Root with sphere visual
            var root = new GameObject("GoldenMote");

            var visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.name = "MoteVisual";
            visual.transform.SetParent(root.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);

            var renderer = visual.GetComponent<MeshRenderer>();
            if (renderer != null && goldMat != null)
                renderer.sharedMaterial = goldMat;

            // Replace collider with trigger
            var collider = visual.GetComponent<Collider>();
            if (collider != null) Object.DestroyImmediate(collider);

            var trigger = root.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 0.75f; // Generous pickup radius

            // Point light for glow
            var lightObj = new GameObject("MoteGlow");
            lightObj.transform.SetParent(root.transform);
            lightObj.transform.localPosition = Vector3.zero;
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.843f, 0f, 1f); // Gold
            light.intensity = 1f;
            light.range = 2f;
            light.shadows = LightShadows.None; // Prevent shadow atlas overflow

            // Particle trail
            var trailObj = new GameObject("TrailParticles");
            trailObj.transform.SetParent(root.transform);
            trailObj.transform.localPosition = Vector3.zero;
            var ps = trailObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.maxParticles = 30;
            main.startLifetime = 1f;
            main.startSpeed = 0.1f;
            main.startSize = 0.03f;
            main.startColor = new Color(1f, 0.9f, 0.4f, 0.8f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission;
            emission.rateOverTime = 8f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f;

            // Bob animation placeholder (will use simple script)
            var bob = root.AddComponent<MoteBobAnimation>();

            // Save prefab
            PrefabUtility.SaveAsPrefabAsset(root, prefabFile);
            Object.DestroyImmediate(root);

            Debug.Log($"[GoldenMote] Created prefab at {prefabFile}");
        }

        static Material CreateGoldMaterial()
        {
            string matPath = $"{MaterialPath}/M_GoldenMote.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (existing != null) return existing;

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");

            var mat = new Material(shader);
            mat.name = "M_GoldenMote";
            mat.SetColor("_BaseColor", new Color(1f, 0.843f, 0f, 1f));
            mat.SetFloat("_Metallic", 0.8f);
            mat.SetFloat("_Smoothness", 0.9f);
            mat.SetColor("_EmissionColor", new Color(1f, 0.7f, 0f, 1f) * 2f);
            mat.EnableKeyword("_EMISSION");

            if (!AssetDatabase.IsValidFolder(MaterialPath))
                AssetDatabase.CreateFolder("Assets/_Project", "Materials");

            AssetDatabase.CreateAsset(mat, matPath);
            return mat;
        }

        static void EnsureDirectories()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs"))
                AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");
            if (!AssetDatabase.IsValidFolder(PrefabPath))
                AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Collectibles");
            if (!AssetDatabase.IsValidFolder(MaterialPath))
                AssetDatabase.CreateFolder("Assets/_Project", "Materials");
        }
    }
}
