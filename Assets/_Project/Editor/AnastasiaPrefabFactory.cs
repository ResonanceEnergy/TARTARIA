using UnityEditor;
using UnityEngine;
using Tartaria.Integration;

namespace Tartaria.Editor
{
    /// <summary>
    /// Generates the Anastasia ghost prefab with required components.
    /// Menu: Tartaria > Build Assets > Anastasia Prefab
    /// </summary>
    public static class AnastasiaPrefabFactory
    {
        const string PrefabPath = "Assets/_Project/Prefabs/Characters";
        const string MaterialPath = "Assets/_Project/Materials";

        [MenuItem("Tartaria/Build Assets/Anastasia Prefab", false, 20)]
        public static void BuildAnastasiaPrefab()
        {
            EnsureDirectories();

            string prefabFile = $"{PrefabPath}/Anastasia.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabFile) != null)
            {
                Debug.Log("[AnastasiaPrefab] Prefab already exists, skipping.");
                return;
            }

            // Create ghost material (URP/Lit transparent)
            var ghostMat = CreateGhostMaterial();

            // Root object
            var root = new GameObject("Anastasia");

            // Visual placeholder (capsule — will be replaced with model)
            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "GhostVisual";
            visual.transform.SetParent(root.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(0.4f, 0.8f, 0.4f);

            // Apply ghost material
            var renderer = visual.GetComponent<MeshRenderer>();
            if (renderer != null && ghostMat != null)
                renderer.sharedMaterial = ghostMat;

            // Remove collider (ghost is non-physical)
            var collider = visual.GetComponent<Collider>();
            if (collider != null) Object.DestroyImmediate(collider);

            // Point light for ethereal glow
            var lightObj = new GameObject("EtherealGlow");
            lightObj.transform.SetParent(root.transform);
            lightObj.transform.localPosition = new Vector3(0, 1f, 0);
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.75f, 0.85f, 1f, 1f); // Pale blue
            light.intensity = 0.5f;
            light.range = 3f;

            // Particle system for golden mote attraction
            var particleObj = new GameObject("MoteParticles");
            particleObj.transform.SetParent(root.transform);
            particleObj.transform.localPosition = new Vector3(0, 1f, 0);
            var ps = particleObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.maxParticles = 20;
            main.startLifetime = 2f;
            main.startSpeed = 0.3f;
            main.startSize = 0.05f;
            main.startColor = new Color(1f, 0.843f, 0f, 0.6f); // Gold
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission;
            emission.rateOverTime = 3f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;

            // Tag for systems to find
            root.tag = "Untagged"; // Will be set to custom tag in scene

            // Save as prefab
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabFile);
            Object.DestroyImmediate(root);

            Debug.Log($"[AnastasiaPrefab] Created prefab at {prefabFile}");
        }

        static Material CreateGhostMaterial()
        {
            string matPath = $"{MaterialPath}/M_Anastasia_Ghost.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (existing != null) return existing;

            // Find URP Lit shader
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");

            var mat = new Material(shader);
            mat.name = "M_Anastasia_Ghost";

            // Set transparent mode
            mat.SetFloat("_Surface", 1); // Transparent
            mat.SetFloat("_Blend", 0);   // Alpha
            mat.SetFloat("_AlphaClip", 0);
            mat.SetColor("_BaseColor", new Color(0.75f, 0.85f, 1f, 0.35f));
            mat.SetColor("_EmissionColor", new Color(0.3f, 0.4f, 0.6f, 1f) * 0.5f);
            mat.EnableKeyword("_EMISSION");
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

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
                AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Characters");
            if (!AssetDatabase.IsValidFolder(MaterialPath))
                AssetDatabase.CreateFolder("Assets/_Project", "Materials");
        }
    }
}
