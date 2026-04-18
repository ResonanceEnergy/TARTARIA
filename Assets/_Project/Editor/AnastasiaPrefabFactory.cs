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

            // Force-rebuild every time (like Player) so visual updates propagate
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabFile);
            if (existing != null)
            {
                AssetDatabase.DeleteAsset(prefabFile);
                Debug.Log("[AnastasiaPrefab] Deleted stale prefab — rebuilding.");
            }

            // Create ghost material (URP/Lit transparent)
            var ghostMat = CreateGhostMaterial();
            var glowMat = CreateGlowMaterial();

            // Root object — Anastasia is a floating ghost companion
            var root = new GameObject("Anastasia");

            // --- Dress/Cloak (wide capsule body at bottom) ---
            var cloak = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            cloak.name = "Cloak";
            cloak.transform.SetParent(root.transform);
            cloak.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            cloak.transform.localScale = new Vector3(0.55f, 0.75f, 0.55f);
            Object.DestroyImmediate(cloak.GetComponent<Collider>());
            if (ghostMat != null) cloak.GetComponent<MeshRenderer>().sharedMaterial = ghostMat;

            // --- Torso (slender capsule) ---
            var torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            torso.name = "Torso";
            torso.transform.SetParent(root.transform);
            torso.transform.localPosition = new Vector3(0f, 1.55f, 0f);
            torso.transform.localScale = new Vector3(0.32f, 0.52f, 0.32f);
            Object.DestroyImmediate(torso.GetComponent<Collider>());
            if (ghostMat != null) torso.GetComponent<MeshRenderer>().sharedMaterial = ghostMat;

            // --- Head (sphere) ---
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(root.transform);
            head.transform.localPosition = new Vector3(0f, 2.18f, 0f);
            head.transform.localScale = new Vector3(0.33f, 0.36f, 0.33f);
            Object.DestroyImmediate(head.GetComponent<Collider>());
            if (ghostMat != null) head.GetComponent<MeshRenderer>().sharedMaterial = ghostMat;

            // --- Hair (flattened sphere on top of head) ---
            var hair = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hair.name = "Hair";
            hair.transform.SetParent(root.transform);
            hair.transform.localPosition = new Vector3(0f, 2.40f, -0.04f);
            hair.transform.localScale = new Vector3(0.36f, 0.18f, 0.34f);
            Object.DestroyImmediate(hair.GetComponent<Collider>());
            if (ghostMat != null) hair.GetComponent<MeshRenderer>().sharedMaterial = ghostMat;

            // --- Crown ornament (flattened torus-like sphere) ---
            var crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            crown.name = "Crown";
            crown.transform.SetParent(root.transform);
            crown.transform.localPosition = new Vector3(0f, 2.54f, 0f);
            crown.transform.localScale = new Vector3(0.28f, 0.07f, 0.28f);
            Object.DestroyImmediate(crown.GetComponent<Collider>());
            if (glowMat != null) crown.GetComponent<MeshRenderer>().sharedMaterial = glowMat;

            // --- Two glowing eyes ---
            var eyeL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eyeL.name = "EyeLeft";
            eyeL.transform.SetParent(root.transform);
            eyeL.transform.localPosition = new Vector3(-0.09f, 2.20f, 0.16f);
            eyeL.transform.localScale = new Vector3(0.055f, 0.055f, 0.055f);
            Object.DestroyImmediate(eyeL.GetComponent<Collider>());
            if (glowMat != null) eyeL.GetComponent<MeshRenderer>().sharedMaterial = glowMat;

            var eyeR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eyeR.name = "EyeRight";
            eyeR.transform.SetParent(root.transform);
            eyeR.transform.localPosition = new Vector3(0.09f, 2.20f, 0.16f);
            eyeR.transform.localScale = new Vector3(0.055f, 0.055f, 0.055f);
            Object.DestroyImmediate(eyeR.GetComponent<Collider>());
            if (glowMat != null) eyeR.GetComponent<MeshRenderer>().sharedMaterial = glowMat;

            // --- Ethereal glow point light ---
            var lightObj = new GameObject("EtherealGlow");
            lightObj.transform.SetParent(root.transform);
            lightObj.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.72f, 0.82f, 1f);
            light.intensity = 0.8f;
            light.range = 5f;
            light.shadows = LightShadows.None; // ghost light casts no shadows

            // --- Golden mote particles ---
            var particleObj = new GameObject("MoteParticles");
            particleObj.transform.SetParent(root.transform);
            particleObj.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            var ps = particleObj.AddComponent<ParticleSystem>();
            var psMain = ps.main;
            psMain.maxParticles = 20;
            psMain.startLifetime = 2.5f;
            psMain.startSpeed = 0.4f;
            psMain.startSize = 0.06f;
            psMain.startColor = new Color(1f, 0.85f, 0.3f, 0.7f);
            psMain.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission;
            emission.rateOverTime = 4f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.6f;

            // --- AnastasiaController component (required for spawn to work) ---
            root.AddComponent<AnastasiaController>();

            // --- Save as prefab ---
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabFile);
            Object.DestroyImmediate(root);

            Debug.Log($"[AnastasiaPrefab] Rebuilt articulated ghost prefab at {prefabFile}");
        }

        static Material CreateGlowMaterial()
        {
            string matPath = $"{MaterialPath}/M_Anastasia_Glow.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (existing != null) return existing;

            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(shader) { name = "M_Anastasia_Glow" };
            mat.SetColor("_BaseColor", new Color(0.8f, 0.92f, 1f, 0.9f));
            mat.SetColor("_EmissionColor", new Color(0.5f, 0.72f, 1f) * 2.5f);
            mat.EnableKeyword("_EMISSION");
            mat.SetFloat("_Smoothness", 0.85f);
            AssetDatabase.CreateAsset(mat, matPath);
            return mat;
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
